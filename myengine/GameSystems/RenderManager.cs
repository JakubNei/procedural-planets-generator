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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyEngine
{
	public class RenderManager
	{
		public RenderContext RenderContext { get; set; } = RenderContext.Geometry;

		public DeferredGBuffer GBuffer { get; private set; }

		public Cubemap SkyboxCubeMap { get; set; }

		int toRenderCount = 0;
		IRenderable[] toRender = new IRenderable[10000];
		//List<IRenderable> toRender = new List<IRenderable>();


		Shader FinalDrawShader => factory.GetShader("internal/finalDraw.glsl");

		public bool drawLines { get { return debug.CommonCVars.DebugRenderWithLines().Bool; } }
		public bool enablePostProcessEffects { get { return debug.CommonCVars.EnablePostProcessEffects().Bool; } }
		public bool debugBounds { get { return debug.CommonCVars.DrawDebugBounds().Bool; } }
		public bool shadowsEnabled { get { return debug.CommonCVars.ShadowsDisabled().Bool == false; } }

		readonly Factory factory;
		readonly Debug debug;

		public RenderManager(Events.EventSystem eventSystem, Factory factory, Debug debug)
		{
			this.factory = factory;
			this.debug = debug;

			eventSystem.Register<Events.WindowResized>(evt =>
			{
				if (GBuffer != null) GBuffer.Dispose();
				GBuffer = new DeferredGBuffer(factory, evt.NewPixelWidth, evt.NewPixelHeight);
			});
		}

		public void RenderAll(UniformBlock ubo, Camera camera, IList<ILight> allLights, IEnumerable<IPostProcessEffect> postProcessEffect)
		{
			camera.UploadDataToUBO(ubo); // bind camera view params
										 //GL.BeginQuery(QueryTarget.)

			RenderGBuffer(ubo, camera);
			RenderLights(ubo, camera, allLights);
			if (drawLines)
			{
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill); MyGL.Check();
			}
			RenderPostProcessEffects(ubo, postProcessEffect);


			GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0); MyGL.Check();
			GL.Viewport(0, 0, camera.pixelWidth, camera.pixelHeight); MyGL.Check();

			// FINAL DRAW TO SCREEN
			{
				//DebugDrawTexture(gBuffer.finalTextureToRead);
				GL.Disable(EnableCap.DepthTest); MyGL.Check();
				GL.Disable(EnableCap.CullFace); MyGL.Check();
				GL.Disable(EnableCap.Blend); MyGL.Check();

				FinalDrawShader.Uniforms.Set("finalDrawTexture", GBuffer.finalTextureToRead);
				if (FinalDrawShader.Bind())
				{
					factory.QuadMesh.Draw();
				}
			}

			if (debugBounds)
			{
				if (factory.GetShader("internal/debugDrawBounds.shader").Bind())
				{
					GL.DepthMask(false); MyGL.Check();
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line); MyGL.Check();
					GL.Disable(EnableCap.DepthTest); MyGL.Check();
					GL.Disable(EnableCap.CullFace); MyGL.Check();
					GL.Disable(EnableCap.Blend); MyGL.Check();
					var camPos = camera.ViewPointPosition;
					for (int i = 0; i < toRenderCount; i++)
					{
						var renderable = toRender[i];
						var bounds = renderable.GetCameraSpaceBounds(camPos);

						var modelMat = Matrix4.CreateScale(bounds.Extents) * Matrix4.CreateTranslation(bounds.Center);
						var modelViewMat = modelMat * camera.GetRotationMatrix();

						ubo.model.modelMatrix = modelMat;
						ubo.model.modelViewMatrix = modelViewMat;
						ubo.model.modelViewProjectionMatrix = modelViewMat * camera.GetProjectionMat();
						ubo.modelUBO.UploadData();
						factory.SkyBoxMesh.Draw(false);
					}
					GL.DepthMask(true); MyGL.Check();
					GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill); MyGL.Check();
					GL.Enable(EnableCap.DepthTest); MyGL.Check();
					GL.Enable(EnableCap.CullFace); MyGL.Check();
					GL.Disable(EnableCap.Blend); MyGL.Check();
				}
			}

			if (debug.CommonCVars.DebugDrawNormalBufferContents().Bool) GBuffer.DebugDrawNormal();
			if (debug.CommonCVars.DebugDrawGBufferContents().Bool) GBuffer.DebugDrawContents();
			//if (drawShadowMapContents) DebugDrawTexture(shadowMap.depthMap, new Vector4(0.5f, 0.5f, 1, 1), new Vector4(0.5f,0.5f,0,1), 1, 0);


			ErrorCode glError;
			while ((glError = GL.GetError()) != ErrorCode.NoError)
				debug.Error("GL Error: " + glError, false);
		}

		public void RenderGBuffer(UniformBlock ubo, Camera camera)
		{
			// G BUFFER GRAB PASS
			{
				GBuffer.BindAllFrameBuffersForDrawing();

				GL.Enable(EnableCap.DepthTest); MyGL.Check();
				GL.DepthMask(true); MyGL.Check();
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit); MyGL.Check();

				// SKYBOX PASS

				if (SkyboxCubeMap != null)
				{
					GL.DepthRange(0.999, 1); MyGL.Check();
					GL.DepthMask(false); MyGL.Check();

					var shader = factory.GetShader("internal/deferred.skybox.shader");
					shader.Uniforms.Set("skyboxCubeMap", SkyboxCubeMap);
					shader.Bind();

					factory.SkyBoxMesh.Draw();
					GL.DepthRange(0, 1); MyGL.Check();
				}


				if (drawLines)
				{
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line); MyGL.Check();
				}
				else
				{
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill); MyGL.Check();
				}

				// RENDER ALL OBJECTS
				{
					GL.DepthMask(true); MyGL.Check();

					GL.Enable(EnableCap.CullFace); MyGL.Check();
					GL.Disable(EnableCap.Blend); MyGL.Check();
					GL.CullFace(CullFaceMode.Back); MyGL.Check();
					for (int i = 0; i < toRenderCount; i++)
					{
						var renderable = toRender[i];
						renderable.Material.BeforeBindCallback();
						renderable.Material.Uniforms.SendAllUniformsTo(renderable.Material.GBufferShader.Uniforms);
						renderable.Material.GBufferShader.Bind();
						renderable.UploadUBOandDraw(camera, ubo);
					}
					// GL.MultiDrawElementsIndirect
				}

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill); MyGL.Check();
				GBuffer.Unbind();
			}
		}

		public void RenderLights(UniformBlock ubo, Camera camera, IList<ILight> allLights)
		{
			#region Lights rendering

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

					camera.UploadDataToUBO(ubo); // bind camera view params

					// G BUFFER LIGHT PASS

					{
						GL.Disable(EnableCap.CullFace); MyGL.Check();
						//GL.CullFace(CullFaceMode.Back);

						GL.Disable(EnableCap.DepthTest); MyGL.Check();
						GL.DepthMask(false); MyGL.Check();

						light.UploadUBOdata(camera, ubo, lightIndex);

						var shader = factory.GetShader("internal/deferred.oneLight.shader");
						GBuffer.BindForLightPass(shader);

						if (lightIndex == 0)
						{
							GL.Clear(ClearBufferMask.ColorBufferBit); MyGL.Check();
						}

						if (shadowsEnabled && light.HasShadows)
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
							factory.QuadMesh.Draw();
							GL.Disable(EnableCap.Blend); MyGL.Check();

						}

						GBuffer.Unbind();
					}
				}
			}

			#endregion Lights rendering
		}

		public void RenderPostProcessEffects(UniformBlock ubo, IEnumerable<IPostProcessEffect> postProcessEffects)
		{
			// POST PROCESS EFFECTs
			if (enablePostProcessEffects)
			{
				GL.Disable(EnableCap.DepthTest); MyGL.Check();
				GL.Disable(EnableCap.CullFace); MyGL.Check();
				GL.Disable(EnableCap.Blend); MyGL.Check();

				GL.Disable(EnableCap.DepthTest); MyGL.Check();
				GL.DepthMask(false); MyGL.Check();

				foreach (var pe in postProcessEffects)
				{
					if (pe.IsEnabled == false) continue;
					pe.BeforeBindCallBack();
					GBuffer.BindForPostProcessEffects(pe);
					pe.Shader.Bind();
					factory.QuadMesh.Draw();
				}
				GBuffer.Unbind();

			}
		}

		int lastTotalPossible = 0;
		public void BuildRenderList(IList<IRenderable> possibleRenderables, Camera camera)
		{
			// without Parallel.ForEach = 130 fps
			// with ConcurrentBag = 180 fps
			// with ConcurrentQueue = 200 fps
			// with lock List = 200 fps

			var frustum = camera.GetFrustum();
			var camPos = camera.Transform.Position;
			var totalPossible = possibleRenderables.Count;

			if (lastTotalPossible > totalPossible) Array.Clear(toRender, totalPossible, lastTotalPossible - totalPossible);

			toRenderCount = 0;
			lock (possibleRenderables)
			{

				Parallel.ForEach(possibleRenderables, (renderable) =>
					{
						if (renderable != null && renderable.ShouldRenderInContext(camera, RenderContext))
						{
							if (renderable.ForcePassFrustumCulling)
							{
								renderable.SetCameraRenderStatusFeedback(camera, RenderStatus.RenderedAndVisible);
								var newIndex = Interlocked.Increment(ref toRenderCount);
								toRender[newIndex - 1] = renderable;
							}
							else
							{
								var bounds = renderable.GetCameraSpaceBounds(camPos);
								if (
									frustum.VsSphere(bounds.Center, bounds.Extents.Length)
									&& frustum.VsBounds(bounds)
								)
								{
									renderable.SetCameraRenderStatusFeedback(camera, RenderStatus.RenderedAndVisible);
									var newIndex = Interlocked.Increment(ref toRenderCount);
									toRender[newIndex - 1] = renderable;
								}
								else
								{
									renderable.SetCameraRenderStatusFeedback(camera, RenderStatus.NotRendered);
								}
							}
						}
					}
				);
			}
			debug.AddValue("rendering / meshes rendered", toRenderCount + "/" + totalPossible);

			if (debug.CommonCVars.SortRenderers())
			{
				var comparer = new RenderableDistanceComparer(camera.ViewPointPosition);
				Array.Sort(toRender, 0, toRenderCount, comparer); // sorts renderables so the closest to camere are first
			}
		}

		class RenderableDistanceComparer : IComparer<IRenderable>
		{
			WorldPos viewPointPosition;

			public RenderableDistanceComparer(WorldPos viewPointPosition)
			{
				this.viewPointPosition = viewPointPosition;
			}

			//Less than zero = x is less than y.
			//Zero = x equals y.
			//Greater than zero = x is greater than y.
			public int Compare(IRenderable x, IRenderable y)
			{
				if (ReferenceEquals(y, x)) return 0;
				var distX = x.GetCameraSpaceBounds(viewPointPosition).Center.LengthFast;
				var distY = y.GetCameraSpaceBounds(viewPointPosition).Center.LengthFast;
				return distX.CompareTo(distY);
			}
		}
	}
}