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

		public DeferredGBuffer GBuffer { get; private set; }

		public Cubemap SkyboxCubeMap { get; set; }
		Shader FinalDrawShader => Factory.GetShader("internal/finalDraw.glsl");

		CVar DrawLines => Debug.GetCVar("rendering / debug / draw lines");
		CVar EnablePostProcessEffects => Debug.GetCVar("rendering / enable post process effects");
		CVar DebugBounds => Debug.GetCVar("rendering / debug / draw mesh bouding boxes");
		CVar ShadowsEnabled => Debug.GetCVar("rendering / shadows enabled");

		CVar EnableCulling => Debug.GetCVar("rendering / enable culling", true);
		CVar EnableRasterizerRasterization => Debug.GetCVar("rendering / enable rasterizer rasterization", true);
		CVar EnableRasterizerCulling => Debug.GetCVar("rendering / enable rasterizer culling", true);
		CVar ShowRasterizerContents => Debug.GetCVar("rendering / debug / show rasterizer contents");
		CVar SortRenderables => Debug.GetCVar("rendering / sort renderables", true);
		CVar DoParallelize => Debug.GetCVar("rendering / parallelize render prepare", true);

		CVar RenderOnlyFront => Debug.GetCVar("rendering / render only front of triangles", true);

		ParallerRunner paraller;

		public RenderManager()
		{
			EventSystem.On<Events.WindowResized>(evt =>
			{
				if (GBuffer != null) GBuffer.Dispose();
				GBuffer = new DeferredGBuffer(evt.NewPixelWidth, evt.NewPixelHeight);
			});

			EnableRasterizerRasterization.OnChangedAndNow((c) =>
			{
				if (c.Bool) rasterizer = new SoftwareDepthRasterizer(200, 100);
				else rasterizer = null;
			});

			ShowRasterizerContents.OnChangedAndNow((c) =>
			{
				if (c.Bool) rasterizer?.Show();
				else rasterizer?.Hide();
			});

			paraller = new ParallerRunner(Environment.ProcessorCount - 2, "render manager", ThreadPriority.Highest);
		}

		public void RenderAll(UniformBlock ubo, Camera camera, IList<ILight> allLights, IEnumerable<IPostProcessEffect> postProcessEffect)
		{
			camera.UploadCameraDataToUBO(ubo); // bind camera view params and matrices only once

			RenderGBuffer(ubo, camera);

			RenderLights(ubo, camera, allLights);

			if (DrawLines) GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill); MyGL.Check();

			RenderPostProcessEffects(ubo, postProcessEffect);


			GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0); MyGL.Check();
			GL.Viewport(0, 0, camera.PixelWidth, camera.PixelHeight); MyGL.Check();

			// FINAL DRAW TO SCREEN
			{
				//DebugDrawTexture(gBuffer.finalTextureToRead);
				GL.Disable(EnableCap.DepthTest); MyGL.Check();
				GL.Disable(EnableCap.CullFace); MyGL.Check();
				GL.Disable(EnableCap.Blend); MyGL.Check();

				FinalDrawShader.Uniforms.Set("finalDrawTexture", GBuffer.finalTextureToRead);
				if (FinalDrawShader.Bind())
				{
					Factory.QuadMesh.Draw();
				}
			}

			if (DebugBounds) RenderDebugBounds(ubo, camera);

			if (Debug.GetCVar("rendering / debug / draw normal buffer contents")) GBuffer.DebugDrawNormal();
			if (Debug.GetCVar("rendering / debug / draw gbuffer contents")) GBuffer.DebugDrawContents();
			//if (drawShadowMapContents) DebugDrawTexture(shadowMap.depthMap, new Vector4(0.5f, 0.5f, 1, 1), new Vector4(0.5f,0.5f,0,1), 1, 0);

			ErrorCode glError;
			while ((glError = GL.GetError()) != ErrorCode.NoError)
				Log.Error("GL Error: " + glError);
		}

		private void RenderGBuffer(UniformBlock ubo, Camera camera)
		{
			// G BUFFER GRAB PASS
			{
				GBuffer.BindAllFrameBuffersForDrawing();

				GL.DepthMask(true); MyGL.Check();				

				// SKYBOX PASS
				if (Debug.GetCVar("rendering / debug / render white background"))
				{
					GL.ClearColor(System.Drawing.Color.White); MyGL.Check();
					GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); MyGL.Check();
				}
				else
				{
					GL.ClearColor(System.Drawing.Color.Black); MyGL.Check();
					GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); MyGL.Check();

					if (SkyboxCubeMap != null)
					{
						//GL.DepthRange(0.999, 1); MyGL.Check();
						GL.Disable(EnableCap.DepthTest); MyGL.Check();
						GL.DepthMask(false); MyGL.Check();

						var shader = Factory.GetShader("internal/deferred.skybox.shader");
						shader.Uniforms.Set("skyboxCubeMap", SkyboxCubeMap);
						shader.Bind();

						Factory.SkyBoxMesh.Draw();
					}
				}


				if (DrawLines)
				{
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line); MyGL.Check();
				}
				else
				{
					if (RenderOnlyFront)
					{
						GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill); MyGL.Check();
					}
					else
					{
						GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill); MyGL.Check();
					}
				}

				// RENDER ALL OBJECTS
				{
					GL.DepthMask(true); MyGL.Check();
					GL.Enable(EnableCap.DepthTest); MyGL.Check();

					if (RenderOnlyFront)
					{
						GL.Enable(EnableCap.CullFace); MyGL.Check();
						GL.CullFace(CullFaceMode.Back); MyGL.Check();
					}
					else
					{
						GL.Disable(EnableCap.CullFace); MyGL.Check();
					}
					GL.Disable(EnableCap.Blend); MyGL.Check();
					for (int i = 0; i < toRenderRenderablesCount; i++)
					{
						var renderable = toRenderRenderables[i];
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

		private void RenderLights(UniformBlock ubo, Camera camera, IList<ILight> allLights)
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

					camera.UploadCameraDataToUBO(ubo); // bind camera view params

					// G BUFFER LIGHT PASS

					{
						GL.Disable(EnableCap.CullFace); MyGL.Check();
						//GL.CullFace(CullFaceMode.Back);

						GL.Disable(EnableCap.DepthTest); MyGL.Check();
						GL.DepthMask(false); MyGL.Check();

						light.UploadUBOdata(camera, ubo, lightIndex);

						var shader = Factory.GetShader("internal/deferred.oneLight.shader");
						GBuffer.BindForLightPass(shader);

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

						GBuffer.Unbind();
					}
				}
			}

			#endregion Lights rendering
		}

		private void RenderPostProcessEffects(UniformBlock ubo, IEnumerable<IPostProcessEffect> postProcessEffects)
		{
			// POST PROCESS EFFECTs
			if (EnablePostProcessEffects)
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
					Factory.QuadMesh.Draw();
				}
				GBuffer.Unbind();

			}
		}

		private void RenderDebugBounds(UniformBlock ubo, Camera camera)
		{
			if (Factory.GetShader("internal/debugDrawBounds.shader").Bind())
			{
				GL.DepthMask(false); MyGL.Check();
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line); MyGL.Check();
				GL.Disable(EnableCap.DepthTest); MyGL.Check();
				GL.Disable(EnableCap.CullFace); MyGL.Check();
				GL.Disable(EnableCap.Blend); MyGL.Check();
				var camPos = camera.ViewPointPosition;
				for (int i = 0; i < toRenderRenderablesCount; i++)
				{
					var renderable = toRenderRenderables[i];
					var bounds = renderable.GetFloatingOriginSpaceBounds(camPos);

					var modelMat = Matrix4.CreateScale(bounds.Extents) * Matrix4.CreateTranslation(bounds.Center);
					var modelViewMat = modelMat * camera.GetRotationMatrix();

					ubo.model.modelMatrix = modelMat;
					ubo.model.modelViewMatrix = modelViewMat;
					ubo.model.modelViewProjectionMatrix = modelViewMat * camera.GetProjectionMatrix();
					ubo.modelUBO.UploadToGPU();
					Factory.SkyBoxMesh.Draw(false);
				}
				GL.DepthMask(true); MyGL.Check();
				GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill); MyGL.Check();
				GL.Enable(EnableCap.DepthTest); MyGL.Check();
				GL.Enable(EnableCap.CullFace); MyGL.Check();
				GL.Disable(EnableCap.Blend); MyGL.Check();
			}
		}

		public class ParallerRunner
		{
			Thread[] threads;
			ManualResetEventSlim[] workerThreadsRun;
			ManualResetEventSlim[] workerThreadFinished;
			public ParallerRunner(int threadCount = 0, string threadPrefixName = "", ThreadPriority threadPriority = ThreadPriority.Normal)
			{
				if (threadCount < 1) threadCount = Environment.ProcessorCount;
				if (threadCount < 1) threadCount = 1;

				threads = new Thread[threadCount];
				workerThreadFinished = new ManualResetEventSlim[threadCount];
				workerThreadsRun = new ManualResetEventSlim[threadCount];

				for (int i = 0; i < threads.Length; i++)
				{
					workerThreadsRun[i] = new ManualResetEventSlim(false);
					workerThreadFinished[i] = new ManualResetEventSlim(false);

					int workerThreadIndex = i;
					var t = new Thread(() =>
					{
						while (true)
						{
							workerThreadsRun[workerThreadIndex].Wait();
							ExecuteNext(workerThreadIndex);
						}
					});
					threads[i] = t;

					t.Name = threadPrefixName + " worker #" + i;
					t.IsBackground = true;
					t.Priority = threadPriority;
					t.Start();

				}
			}




			int fromInclusive;
			int toExclusive;
			Action<int> body;
			int currentIndex;

			SemaphoreSlim s;
			private void ExecuteNext(int workerThreadIndex)
			{
				var myCurrentIndex = Interlocked.Increment(ref currentIndex) - 1;
				if (myCurrentIndex >= toExclusive)
				{
					workerThreadFinished[workerThreadIndex].Set();
					return;
				}
				body(myCurrentIndex);
			}
			private bool ExecuteNextAndReturnStatus(int workerThreadIndex)
			{
				var myCurrentIndex = Interlocked.Increment(ref currentIndex) - 1;
				if (myCurrentIndex >= toExclusive)
				{
					workerThreadFinished[workerThreadIndex].Set();
					return false;
				}
				body(myCurrentIndex);
				return true;
			}


			public void For(int fromInclusive, int toExclusive, Action<int> body)
			{
				this.fromInclusive = fromInclusive;
				this.toExclusive = toExclusive;
				this.currentIndex = fromInclusive;
				this.body = body;

				var useThreads = threads.Length;
				var maxUseThreads = toExclusive - fromInclusive;
				if (useThreads > maxUseThreads) useThreads = maxUseThreads;

				for (int i = 0; i < useThreads; i++)
				{
					workerThreadFinished[i].Reset();
					workerThreadsRun[i].Set();
				}

				for (int i = 0; i < useThreads; i++)
				{
					workerThreadFinished[i].Wait();
					workerThreadsRun[i].Reset();
				}
			}


			public void ForUseThisThreadToo(int fromInclusive, int toExclusive, Action<int> body)
			{
				this.fromInclusive = fromInclusive;
				this.toExclusive = toExclusive;
				this.currentIndex = fromInclusive;
				this.body = body;

				var useThreads = threads.Length;
				var maxUseThreads = toExclusive - fromInclusive;
				if (useThreads > maxUseThreads) useThreads = maxUseThreads;

				for (int i = 1; i < useThreads; i++)
				{
					workerThreadFinished[i].Reset();
					workerThreadsRun[i].Set();
				}

				while (ExecuteNextAndReturnStatus(0)) ;

				for (int i = 1; i < useThreads; i++)
				{
					workerThreadFinished[i].Wait();
					workerThreadsRun[i].Reset();
				}
			}

		}

		SoftwareDepthRasterizer rasterizer;


		const int maxToRenderAtOnce = 10000;

		IRenderable[] passedFrustumCulling = new IRenderable[maxToRenderAtOnce];
		IRenderable[] passedRasterizationCulling = new IRenderable[maxToRenderAtOnce];
		IRenderable[] toRenderRenderables;
		int toRenderRenderablesCount = 0;

		float[] distancesToCamera = new float[maxToRenderAtOnce];
		int lastTotalPossible = 0;
		public void PrepareRender(RenderableData data, Camera camera)
		{
			// without Parallel.ForEach = 130 fps
			// with ConcurrentBag = 180 fps
			// with ConcurrentQueue = 200 fps
			// with lock List = 200 fps

			var frustum = camera.GetFrustum();
			var camPos = camera.Transform.Position;

			var possibleRenderables = data.Renderers;
			var possibleRenderablesCount = possibleRenderables.Count;


			Debug.AddValue("rendering / meshes / total possible", possibleRenderablesCount);

			// clear references to IRenderables in part of the array that will not be used, there is a chance that it might hang onto references thus stopping GC from collecting IRenderables
			if (lastTotalPossible > possibleRenderablesCount)
			{
				Array.Clear(passedFrustumCulling, possibleRenderablesCount, lastTotalPossible - possibleRenderablesCount);
				Array.Clear(passedRasterizationCulling, possibleRenderablesCount, lastTotalPossible - possibleRenderablesCount);
			}
			lastTotalPossible = possibleRenderablesCount;

			rasterizer?.Clear();


			if (EnableCulling)
			{

				int passedFrustumCullingIndex = 0;
				{
                    Action<IRenderable> work = renderable =>
                    {
                        if (renderable.ShouldRenderInContext(camera, RenderContext))
                        {
                            if (renderable.ForcePassCulling)
                            {
                                var newIndex = Interlocked.Increment(ref passedFrustumCullingIndex) - 1;
                                passedFrustumCulling[newIndex] = renderable;
                                rasterizer?.AddTriangles(renderable.GetCameraSpaceOccluderTriangles(camera));
                            }
                            else
                            {
                                var bounds = renderable.GetFloatingOriginSpaceBounds(camPos);
                                if (
                                    frustum.VsSphere(bounds.Center, bounds.Extents.LengthSquared)
                                    && frustum.VsBounds(bounds)
                                )
                                {
                                    var newIndex = Interlocked.Increment(ref passedFrustumCullingIndex) - 1;
                                    passedFrustumCulling[newIndex] = renderable;
                                    rasterizer?.AddTriangles(renderable.GetCameraSpaceOccluderTriangles(camera));
                                }
                                else
                                {
                                    renderable.SetCameraRenderStatusFeedback(camera, RenderStatus.NotRendered);
                                }
                            }
                        }
                    };

					if (DoParallelize)
					{
						paraller.ForUseThisThreadToo(0, possibleRenderables.Count, i =>
						{
							IRenderable renderable;
							if (possibleRenderables.TryGetAtIndex(i, out renderable))
								work(renderable);
						});
					}
					else
					{
						for (int i = 0; i < possibleRenderables.Count; i++)
						{
							IRenderable renderable;
							if (possibleRenderables.TryGetAtIndex(i, out renderable))
								work(renderable);
						}
					}

				}


				Debug.AddValue("rendering / meshes / passed frustum culling", passedFrustumCullingIndex);

				if (rasterizer != null && EnableRasterizerCulling)
				{

					int passedRasterizationCullingIndex = 0;

					Action<IRenderable> work = renderable =>
					{
						if (renderable.ForcePassCulling)
						{
							renderable.SetCameraRenderStatusFeedback(camera, RenderStatus.RenderedAndVisible);
							var newIndex = Interlocked.Increment(ref passedRasterizationCullingIndex) - 1;
							passedRasterizationCulling[newIndex] = renderable;
							distancesToCamera[newIndex] = 0;
						}
						else
						{
							var bounds = renderable.GetCameraSpaceBounds(camera);
							if (rasterizer.AreBoundsVisible(bounds))
							{
								renderable.SetCameraRenderStatusFeedback(camera, RenderStatus.RenderedAndVisible);
								var newIndex = Interlocked.Increment(ref passedRasterizationCullingIndex) - 1;
								passedRasterizationCulling[newIndex] = renderable;
								distancesToCamera[newIndex] = bounds.depthClosest;
							}
							else
							{
								renderable.SetCameraRenderStatusFeedback(camera, RenderStatus.NotRendered);
							}
						}
					};


					if (DoParallelize)
					{
						paraller.ForUseThisThreadToo(0, passedFrustumCullingIndex, (i) =>
						{
							work(passedFrustumCulling[i]);
						});
					}
					else
					{
						for (int i = 0; i < passedFrustumCullingIndex; i++)
						{
							work(passedFrustumCulling[i]);
						}
					}

					Debug.AddValue("rendering / meshes / passed rasterization culling", passedRasterizationCullingIndex);

					if (SortRenderables)
					{
						// sort renderables so closest to camera are first
						Array.Sort(distancesToCamera, passedRasterizationCulling, 0, passedRasterizationCullingIndex, Comparer<float>.Default);
					}

					toRenderRenderables = passedRasterizationCulling;
					toRenderRenderablesCount = passedRasterizationCullingIndex;
				}
				else
				{
					var a = passedFrustumCulling.Where(renderable => renderable != null && renderable.ShouldRenderInContext(camera, RenderContext)).ToArray();
					toRenderRenderables = a;
					toRenderRenderablesCount = a.Length;
				}

			}
			else
			{
				var a = possibleRenderables.Where(renderable => renderable != null && renderable.ShouldRenderInContext(camera, RenderContext)).ToArray();
				toRenderRenderables = a;
				toRenderRenderablesCount = a.Length;
			}


		}

	}
}