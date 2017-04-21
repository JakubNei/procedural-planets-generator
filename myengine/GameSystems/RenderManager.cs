using MyEngine;
using MyEngine.Components;
using Neitri;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyEngine
{
	public class RenderManager : SingletonsPropertyAccesor
	{

		public RenderContext RenderContext { get; set; } = RenderContext.Geometry;

		DeferredGBuffer gBuffer;

		public Cubemap SkyboxCubeMap { get; set; }
		Shader FinalDrawShader => Factory.GetShader("internal/finalDraw.glsl");

		CVar DrawLines => Debug.GetCVar("rendering / debug / draw lines");
		CVar EnablePostProcessEffects => Debug.GetCVar("rendering / enable post process effects");
		CVar DebugBounds => Debug.GetCVar("rendering / debug / draw mesh bouding boxes");
		CVar ShadowsEnabled => Debug.GetCVar("rendering / shadows enabled");

		CVar EnableCulling => Debug.GetCVar("rendering / occlusion culling / enabled", true);
		CVar EnableRasterizerRasterization => Debug.GetCVar("rendering / occlusion culling / enable rasterizer rasterization", true);
		CVar EnableRasterizerCulling => Debug.GetCVar("rendering / occlusion culling / enable rasterizer culling", true);
		CVar ShowRasterizerContents => Debug.GetCVar("rendering / debug / show rasterizer contents");
		CVar SortRenderables => Debug.GetCVar("rendering / sort renderables", true);
		CVar DoParallelize => Debug.GetCVar("rendering / parallelize render prepare", true);

		CVar RenderOnlyFront => Debug.GetCVar("rendering / render only front of triangles", true);

		ParallerRunner paraller;

		public RenderManager(int width, int height)
		{
			gBuffer = new DeferredGBuffer(width, height);

			EventSystem.On<Events.WindowResized>(evt =>
			{
				if (gBuffer == null || evt.NewPixelWidth != gBuffer.Width || evt.NewPixelHeight != gBuffer.Height)
				{
					if (gBuffer != null) gBuffer.Dispose();
					gBuffer = new DeferredGBuffer(evt.NewPixelWidth, evt.NewPixelHeight);
				}
			});

			EnableRasterizerRasterization.OnChangedAndNow(c =>
			{
				if (c.Bool) rasterizer = new SoftwareDepthRasterizer(200, 100);
				else rasterizer = null;
			});

			ShowRasterizerContents.OnChangedAndNow(c =>
			{
				if (c.Bool) rasterizer?.Show();
				else rasterizer?.Hide();
			});

			paraller = new ParallerRunner(Environment.ProcessorCount - 2, "render manager", ThreadPriority.Highest);
		}

		class GLState
		{
			public void SetDefaults()
			{
				// set these default values only once
				GL.DepthRange(0, 1); MyGL.Check();
				GL.DepthFunc(DepthFunction.Lequal); MyGL.Check();

				GL.FrontFace(FrontFaceDirection.Ccw); MyGL.Check();
				GL.CullFace(CullFaceMode.Back); MyGL.Check();
				GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill); MyGL.Check();
			}

			public void DepthWrite(bool enabled)
			{
				GL.DepthMask(enabled); MyGL.Check();
			}
			public void DepthTest(bool enabled)
			{
				if (enabled)
				{
					GL.Enable(EnableCap.DepthTest); MyGL.Check();
				}
				else
				{
					GL.Disable(EnableCap.DepthTest); MyGL.Check();
				}
			}
			public void Blend(bool enabled)
			{
				if (enabled)
				{
					GL.Enable(EnableCap.Blend); MyGL.Check();
				}
				else
				{
					GL.Disable(EnableCap.Blend); MyGL.Check();
				}
			}
			public void DrawLinesOnly(bool linesOnly, bool drawFrontOnly = true)
			{
				if (linesOnly)
				{
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line); MyGL.Check();
					GL.Disable(EnableCap.CullFace); MyGL.Check();
				}
				else
				{
					if (drawFrontOnly)
					{
						GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill); MyGL.Check();
						GL.Enable(EnableCap.CullFace); MyGL.Check();
					}
					else
					{
						GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill); MyGL.Check();
						GL.Disable(EnableCap.CullFace); MyGL.Check();
					}
				}
			}
		}

		GLState gl = new GLState();

		CameraData prepardWithCameraData;

		public void RenderAll(UniformBlock ubo, IList<ILight> allLights, IEnumerable<IPostProcessEffect> postProcessEffect)
		{
			gl.SetDefaults();


			var camera = prepardWithCameraData;

			camera.UploadCameraDataToUBO(ubo); // bind camera view params and matrices only once

			RenderGBuffer(ubo, camera);

			RenderLights(ubo, camera, allLights);


			// FORWARD RENDERING, TRANSPARENT OBJECTS
			{
				gl.DepthWrite(false);
				gl.DepthTest(true);
				gl.Blend(true);

				GL.BlendEquation(BlendEquationMode.FuncAdd); MyGL.Check();
				GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha); MyGL.Check();

				gl.DrawLinesOnly(DrawLines, false);

				for (int i = 0; i < toRenderTransparentCount; i++)
				{
					var renderable = toRenderTransparent[i];
					renderable.Material.BeforeBindCallback();
					renderable.Material.Uniforms.SendAllUniformsTo(renderable.Material.RenderShader.Uniforms);
					gBuffer.BindForTransparentPass(renderable.Material.RenderShader);
					renderable.Material.RenderShader.Bind();
					renderable.UploadUBOandDraw(camera, ubo);
				}
			}


			RenderPostProcessEffects(ubo, postProcessEffect);


			// FINAL DRAW TO SCREEN
			{
				gl.DepthWrite(false);
				gl.DepthTest(false);
				gl.Blend(false);
				gl.DrawLinesOnly(false);

				GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0); MyGL.Check();
				GL.Viewport(0, 0, camera.PixelWidth, camera.PixelHeight); MyGL.Check();

				FinalDrawShader.Uniforms.Set("finalDrawTexture", gBuffer.FinalTextureToRead);
				if (FinalDrawShader.Bind())
				{
					Factory.QuadMesh.Draw();
				}
			}


			if (DebugBounds) RenderDebugBounds(ubo, camera);

			if (Debug.GetCVar("rendering / debug / draw normal buffer contents")) gBuffer.DebugDrawNormal();
			if (Debug.GetCVar("rendering / debug / draw gbuffer contents")) gBuffer.DebugDrawContents();
			//if (drawShadowMapContents) DebugDrawTexture(shadowMap.depthMap, new Vector4(0.5f, 0.5f, 1, 1), new Vector4(0.5f,0.5f,0,1), 1, 0);

			ErrorCode glError;
			while ((glError = GL.GetError()) != ErrorCode.NoError)
				Log.Error("GL Error: " + glError);

		}

		private void SetPolygonMode()
		{
			gl.DrawLinesOnly(DrawLines, RenderOnlyFront);
		}

		private void RenderGBuffer(UniformBlock ubo, CameraData camera)
		{
			// G BUFFER GRAB PASS
			{
				gBuffer.BindAllFrameBuffersForDrawing();

				// SKYBOX PASS
				if (Debug.GetCVar("rendering / debug / render white background"))
				{
					GL.ClearColor(System.Drawing.Color.White); MyGL.Check();
					GL.DepthMask(true); MyGL.Check();
					GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); MyGL.Check();
				}
				else
				{
					GL.ClearColor(System.Drawing.Color.Black); MyGL.Check();
					GL.DepthMask(true); MyGL.Check();
					GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); MyGL.Check();

					if (SkyboxCubeMap != null)
					{
						GL.Disable(EnableCap.DepthTest); MyGL.Check();
						GL.DepthMask(false); MyGL.Check();
						GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill); MyGL.Check();

						var shader = Factory.GetShader("internal/deferred.skybox.shader");
						shader.Uniforms.Set("skyboxCubeMap", SkyboxCubeMap);
						shader.Bind();

						Factory.SkyBoxMesh.Draw();
					}
				}

				SetPolygonMode();

				// RENDER ALL OBJECTS
				{
					gl.DepthWrite(true);
					gl.DepthTest(true);
					gl.Blend(false);

					for (int i = 0; i < toRenderDefferredCount; i++)
					{
						var renderable = toRenderDefferred[i];
						renderable.Material.BeforeBindCallback();
						renderable.Material.Uniforms.SendAllUniformsTo(renderable.Material.RenderShader.Uniforms);
						renderable.Material.RenderShader.Bind();
						renderable.UploadUBOandDraw(camera, ubo);
					}
					// GL.MultiDrawElementsIndirect
				}

				gBuffer.Unbind();
			}
		}

		private void RenderLights(UniformBlock ubo, CameraData camera, IList<ILight> allLights)
		{

			lock (allLights)
			{
				for (int lightIndex = 0; lightIndex < allLights.Count; lightIndex++)
				{
					var light = allLights[lightIndex];
					if (light == null) continue;

					var shadowMap = light.ShadowMap;

					#region SHADOW MAAPING

					/*
					if (shadowsEnabled && light.HasShadows)
					{
						//GL.Enable(EnableCap.CullFace);
						//GL.CullFace(CullFaceMode.Back);

						shadowMap.FrameBufferForWriting();

						GL.Enable(EnableCap.DepthTest); My.Check();
						GL.DepthMask(true); My.Check();

						shadowMap.Clear();

						shadowMap.shadowViewCamera.UploadDataToUBO(ubo);

						for (int i = 0; i < allRenderers.Count; i++)
						{
							var renderer = allRenderers[i];
							if (renderer == null) continue;

							//if (renderer.CanBeFrustumCulled == false || GeometryUtility.TestPlanesAABB(frustrumPlanes, renderer.bounds))
							{
								renderer.Material.BeforeBindCallback();
								renderer.Material.Uniforms.SendAllUniformsTo(renderer.Material.DepthGrabShader.Uniforms);
								renderer.Material.DepthGrabShader.Bind();
								renderer.UploadUBOandDraw(shadowMap.shadowViewCamera, ubo);
							}
						}
					}*/

					#endregion SHADOW MAAPING

					camera.UploadCameraDataToUBO(ubo); // bind camera view params

					// G BUFFER LIGHT PASS

					{
						gl.DepthWrite(false);
						gl.DepthTest(false);
						gl.DrawLinesOnly(false);

						light.UploadUBOdata(camera, ubo, lightIndex);

						var shader = Factory.GetShader("internal/deferred.oneLight.shader");
						gBuffer.BindForLightPass(shader);

						if (lightIndex == 0)
						{
							GL.Clear(ClearBufferMask.ColorBufferBit); MyGL.Check();
						}

						if (ShadowsEnabled && light.HasShadows)
						{
							shadowMap.BindUniforms(shader);
						}

						if (shader.Bind())
						{

							//GL.Enable(EnableCap.Blend);
							//GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
							//GL.BlendFunc(BlendingFactorSrc.SrcColor, BlendingFactorDest.SrcColor);
							GL.BlendEquation(BlendEquationMode.FuncAdd); MyGL.Check();
							GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One); MyGL.Check();
							Factory.QuadMesh.Draw();
							GL.Disable(EnableCap.Blend); MyGL.Check();

						}

						gBuffer.Unbind();
					}
				}
			}

		}

		private void RenderPostProcessEffects(UniformBlock ubo, IEnumerable<IPostProcessEffect> postProcessEffects)
		{
			// POST PROCESS EFFECTs
			if (EnablePostProcessEffects)
			{
				gl.DepthTest(false);
				gl.DepthWrite(false);
				gl.Blend(false);
				gl.DrawLinesOnly(false);

				foreach (var pe in postProcessEffects)
				{
					if (pe.IsEnabled == false) continue;
					pe.BeforeBindCallBack();
					gBuffer.BindForPostProcessEffects(pe);
					pe.Shader.Bind();
					Factory.QuadMesh.Draw();
				}

				gBuffer.Unbind();
			}
		}

		private void RenderDebugBounds(UniformBlock ubo, CameraData camera)
		{
			if (Factory.GetShader("internal/debugDrawBounds.shader").Bind())
			{
				gl.DepthTest(false);
				gl.DepthWrite(false);
				gl.Blend(false);
				gl.DrawLinesOnly(true, false);

				var camPos = camera.ViewPointPosition;
				for (int i = 0; i < toRenderDefferredCount; i++)
				{
					var renderable = toRenderDefferred[i];
					var bounds = renderable.GetFloatingOriginSpaceBounds(camPos);

					var modelMat = Matrix4.CreateScale(bounds.Extents) * Matrix4.CreateTranslation(bounds.Center);
					var modelViewMat = modelMat * camera.GetRotationMatrix();

					ubo.model.modelMatrix = modelMat;
					ubo.model.modelViewMatrix = modelViewMat;
					ubo.model.modelViewProjectionMatrix = modelViewMat * camera.GetProjectionMatrix();
					ubo.modelUBO.UploadToGPU();
					Factory.SkyBoxMesh.Draw(false);
				}
			}
		}


		public float SuggestedCameraZNear { get; private set; }
		public float SuggestedCameraZFar { get; private set; }
		SoftwareDepthRasterizer rasterizer;

		const int maxToRenderAtOnce = 10000;

		IRenderable[] passedFrustumCulling = new IRenderable[maxToRenderAtOnce];
		IRenderable[] passedRasterizationCulling = new IRenderable[maxToRenderAtOnce];

		IRenderable[] toRenderDefferred = new IRenderable[maxToRenderAtOnce];
		int toRenderDefferredCount = 0;

		IRenderable[] toRenderTransparent = new IRenderable[maxToRenderAtOnce];
		int toRenderTransparentCount = 0;


		void WorkLoad(int fromInclusive, int toExclusive, Action<int> work)
		{
			if (DoParallelize)
			{
				paraller.ForUseThisThreadToo(fromInclusive, toExclusive, (i) =>
				{
					work(i);
				});
			}
			else
			{
				for (int i = fromInclusive; i < toExclusive; i++)
				{
					work(i);
				}
			}
		}

		float[] distancesToCameraDeferred = new float[maxToRenderAtOnce];
		float[] distancesToCameraTransparent = new float[maxToRenderAtOnce];

		int lastTotalPossible = 0;
		public void PrepareRender(RenderableData data, Camera camera)
		{
			var dataVersion = data.Version;

			var cameraData = prepardWithCameraData = camera.GetDataCopy();

			// without Parallel.ForEach = 130 fps
			// with ConcurrentBag = 180 fps
			// with ConcurrentQueue = 200 fps
			// with lock List = 200 fps

			var frustum = prepardWithCameraData.GetFrustum();
			var camPos = prepardWithCameraData.ViewPointPosition;

			var possibleRenderables = data.Renderers;
			var possibleRenderablesCount = possibleRenderables.Count;

			IRenderable[] toRender;
			int toRenderCount;

			Debug.AddValue("rendering / meshes / total possible", possibleRenderablesCount);

			// clear references to IRenderables in part of the array that will not be used, there is a chance that it might hang onto references thus stopping GC from collecting IRenderables
			if (lastTotalPossible > possibleRenderablesCount)
			{
				Array.Clear(passedFrustumCulling, possibleRenderablesCount, lastTotalPossible - possibleRenderablesCount);
				Array.Clear(passedRasterizationCulling, possibleRenderablesCount, lastTotalPossible - possibleRenderablesCount);
				Array.Clear(toRenderDefferred, possibleRenderablesCount, lastTotalPossible - possibleRenderablesCount);
				Array.Clear(toRenderTransparent, possibleRenderablesCount, lastTotalPossible - possibleRenderablesCount);
			}
			lastTotalPossible = possibleRenderablesCount;

			rasterizer?.Clear();


			if (EnableCulling)
			{
				int wantToBeRendered = 0;
				int passedFrustumCullingIndex = 0;
				{
					Action<IRenderable> work = renderable =>
					{
						if (renderable.ShouldRenderInContext(camera, RenderContext))
						{
							Interlocked.Increment(ref wantToBeRendered);
							if (renderable.ForcePassFrustumCulling)
							{
								var newIndex = Interlocked.Increment(ref passedFrustumCullingIndex) - 1;
								passedFrustumCulling[newIndex] = renderable;
								rasterizer?.AddTriangles(renderable.GetCameraSpaceOccluderTriangles(cameraData));
							}
							else
							{
								var bounds = renderable.GetFloatingOriginSpaceBounds(camPos);
								if (
									frustum.VsSphere(bounds.Center, bounds.Extents.LengthFast)
									&& frustum.VsBounds(bounds)
								)
								{
									var newIndex = Interlocked.Increment(ref passedFrustumCullingIndex) - 1;
									passedFrustumCulling[newIndex] = renderable;
									rasterizer?.AddTriangles(renderable.GetCameraSpaceOccluderTriangles(cameraData));
								}
								else
								{
									renderable.SetCameraRenderStatusFeedback(camera, RenderStatus.NotRendered);
								}
							}
						}
					};


					WorkLoad(0, possibleRenderables.Count, i =>
					{
						IRenderable renderable;
						if (possibleRenderables.TryGetAtIndex(i, out renderable))
							work(renderable);
					});

					Debug.AddValue("rendering / meshes / want to be rendered", wantToBeRendered);
				}


				Debug.AddValue("rendering / meshes / passed frustum culling", passedFrustumCullingIndex);

				if (rasterizer != null && EnableRasterizerCulling)
				{

					int passedRasterizationCullingIndex = 0;

					Action<IRenderable> work = renderable =>
					{
						if (renderable.ForcePassRasterizationCulling)
						{
							var newIndex = Interlocked.Increment(ref passedRasterizationCullingIndex) - 1;
							passedRasterizationCulling[newIndex] = renderable;
						}
						else
						{
							var bounds = renderable.GetCameraSpaceBounds(cameraData);
							if (rasterizer.AreBoundsVisible(bounds))
							{
								var newIndex = Interlocked.Increment(ref passedRasterizationCullingIndex) - 1;
								passedRasterizationCulling[newIndex] = renderable; ;
							}
							else
							{
								renderable.SetCameraRenderStatusFeedback(camera, RenderStatus.NotRendered);
							}
						}
					};


					WorkLoad(0, passedFrustumCullingIndex, (i) =>
					{
						work(passedFrustumCulling[i]);
					});

					//var range = cameraData.FarClipPlane - cameraData.NearClipPlane;

					//SuggestedCameraZNear = cameraData.NearClipPlane + rasterizer.totalMinZ.Clamp(0, 1) * range;
					//SuggestedCameraZFar = cameraData.NearClipPlane + rasterizer.totalMaxZ.Clamp(0, 1) * range;

					//if (SuggestedCameraZNear + 1 > SuggestedCameraZFar) SuggestedCameraZFar = SuggestedCameraZNear + 1;

					//cameraData.NearClipPlane = SuggestedCameraZNear * 0.5f;
					//cameraData.FarClipPlane = SuggestedCameraZFar;
					//cameraData.RecalculateProjectionMatrix();


					Debug.AddValue("rendering / meshes / passed rasterization culling", passedRasterizationCullingIndex);

					toRender = passedRasterizationCulling;
					toRenderCount = passedRasterizationCullingIndex;
				}
				else
				{
					var a = passedFrustumCulling.Where(renderable => renderable != null && renderable.ShouldRenderInContext(camera, RenderContext)).ToArray();
					toRender = a;
					toRenderCount = a.Length;
				}

			}
			else
			{
				var a = possibleRenderables.Where(renderable => renderable != null && renderable.ShouldRenderInContext(camera, RenderContext)).ToArray();
				toRender = a;
				toRenderCount = a.Length;
			}

			// final prepare step
			{
				toRenderDefferredCount = 0;
				toRenderTransparentCount = 0;

				Action<IRenderable> work = renderable =>
				{
					if (renderable.Material.RenderShader.IsTransparent)
					{
						renderable.SetCameraRenderStatusFeedback(camera, RenderStatus.RenderedAndVisible);
						var newIndex = Interlocked.Increment(ref toRenderTransparentCount) - 1;
						toRenderTransparent[newIndex] = renderable;
						distancesToCameraTransparent[newIndex] = -renderable.GetCameraSortDistance(cameraData);
					}
					else
					{
						renderable.SetCameraRenderStatusFeedback(camera, RenderStatus.RenderedAndVisible);
						var newIndex = Interlocked.Increment(ref toRenderDefferredCount) - 1;
						toRenderDefferred[newIndex] = renderable;
						distancesToCameraDeferred[newIndex] = renderable.GetCameraSortDistance(cameraData);
					}
				};

				WorkLoad(0, toRenderCount, (i) =>
				{
					work(toRender[i]);
				});

				if (SortRenderables)
				{
					// sort renderables so closest to camera are first
					// could use paraller sort:  e.g.: https://gist.github.com/wieslawsoltes/6592526
					Array.Sort(distancesToCameraDeferred, toRenderDefferred, 0, toRenderDefferredCount, Comparer<float>.Default);
					Array.Sort(distancesToCameraTransparent, toRenderTransparent, 0, toRenderTransparentCount, Comparer<float>.Default);
				}
			}


			if (dataVersion != data.Version)
			{
				Log.Warn("started render prepare with different data to render version");
			}

		}

	}
}