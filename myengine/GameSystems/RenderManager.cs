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
		public object RenderContext { get; set; } = Components.RenderContext.Geometry;

		public DeferredGBuffer GBuffer { get; private set; }

		public Cubemap SkyboxCubeMap { get; set; }

		int toRenderCount = 0;
		IRenderable[] toRender = new IRenderable[10000];

		[Dependency]
		Debug debug;

		[Dependency]
		Factory factory;

		Shader FinalDrawShader => factory.GetShader("internal/finalDraw.glsl");

		public bool drawLines { get { return debug.CommonCVars.DebugRenderWithLines().Bool; } }
		public bool enablePostProcessEffects { get { return debug.CommonCVars.EnablePostProcessEffects().Bool; } }
		public bool debugBounds { get { return debug.CommonCVars.DrawDebugBounds().Bool; } }
		public bool shadowsEnabled { get { return debug.CommonCVars.ShadowsDisabled().Bool == false; } }

		public RenderManager(Events.EventSystem eventSystem)
		{
			eventSystem.Register<Events.WindowResized>(evt =>
			{
				if (GBuffer != null) GBuffer.Dispose();
				GBuffer = new DeferredGBuffer(evt.NewPixelWidth, evt.NewPixelHeight);
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
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill); My.Check();
			}
			RenderPostProcessEffects(ubo, postProcessEffect);

			// FINAL DRAW TO SCREEN
			{
				//DebugDrawTexture(gBuffer.finalTextureToRead);
				GL.Disable(EnableCap.DepthTest); My.Check();
				GL.Disable(EnableCap.CullFace); My.Check();
				GL.Disable(EnableCap.Blend); My.Check();

				GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0); My.Check();
				GL.Viewport(0, 0, camera.pixelWidth, camera.pixelHeight); My.Check();

				FinalDrawShader.Uniforms.Set("finalDrawTexture", GBuffer.finalTextureToRead);
				if (FinalDrawShader.Bind())
				{
					factory.QuadMesh.Draw();
				}
			}

			/*if(debugBounds)
            {
                var allColiders = new List<BoxCollider>();
                foreach (var go in Factory.allEntitys)
                {
                    allColiders.AddRange(go.GetComponents<BoxCollider>());
                }
                GL.DepthMask(false); My.Check();
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line); My.Check();
                GL.Disable(EnableCap.DepthTest); My.Check();
                GL.Disable(EnableCap.CullFace); My.Check();
                GL.Disable(EnableCap.Blend); My.Check();
                foreach (var c in allColiders)
                {
                    var modelMat = c.entity.transform.GetScalePosRotMatrix();
                    var modelViewMat = modelMat * camera.GetViewMat();
                    ubo.model.modelMatrix = modelMat;
                    ubo.model.modelViewMatrix = modelViewMat;
                    ubo.model.modelViewProjectionMatrix = modelViewMat * camera.GetProjectionMat();
                    ubo.modelUBO.UploadData();
                    skyboxMesh.Draw();
                }
            }*/

			if (debug.CommonCVars.DebugDrawNormalBufferContents().Bool) GBuffer.DebugDrawNormal();
			if (debug.CommonCVars.DebugDrawGBufferContents().Bool) GBuffer.DebugDrawContents();
			//if (drawShadowMapContents) DebugDrawTexture(shadowMap.depthMap, new Vector4(0.5f, 0.5f, 1, 1), new Vector4(0.5f,0.5f,0,1), 1, 0);
		}

		public void RenderGBuffer(UniformBlock ubo, Camera camera)
		{
			// G BUFFER GRAB PASS
			{
				GBuffer.BindAllFrameBuffersForDrawing();

				GL.Enable(EnableCap.DepthTest); My.Check();
				GL.DepthMask(true); My.Check();
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit); My.Check();

				// SKYBOX PASS

				if (SkyboxCubeMap != null)
				{
					GL.DepthRange(0.999, 1); My.Check();
					GL.DepthMask(false); My.Check();

					var shader = factory.GetShader("internal/deferred.skybox.shader");
					shader.Uniforms.Set("skyboxCubeMap", SkyboxCubeMap);
					shader.Bind();

					factory.SkyBoxMesh.Draw();
					GL.DepthRange(0, 1); My.Check();
				}


				if (drawLines)
				{
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line); My.Check();
				}
				else
				{
					GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill); My.Check();
				}

				// RENDER ALL OBJECTS
				{
					GL.DepthMask(true); My.Check();

					GL.Enable(EnableCap.CullFace); My.Check();
					GL.Disable(EnableCap.Blend); My.Check();
					GL.CullFace(CullFaceMode.Back); My.Check();
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

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill); My.Check();
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
						GL.Disable(EnableCap.CullFace); My.Check();
						//GL.CullFace(CullFaceMode.Back);

						GL.Disable(EnableCap.DepthTest); My.Check();
						GL.DepthMask(false); My.Check();

						light.UploadUBOdata(camera, ubo, lightIndex);

						var shader = factory.GetShader("internal/deferred.oneLight.shader");
						GBuffer.BindForLightPass(shader);

						if (lightIndex == 0)
						{
							GL.Clear(ClearBufferMask.ColorBufferBit); My.Check();
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
							GL.BlendEquation(BlendEquationMode.FuncAdd); My.Check();
							GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One); My.Check();
							factory.QuadMesh.Draw();
							GL.Disable(EnableCap.Blend); My.Check();

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
				GL.Disable(EnableCap.DepthTest); My.Check();
				GL.Disable(EnableCap.CullFace); My.Check();
				GL.Disable(EnableCap.Blend); My.Check();

				GL.Disable(EnableCap.DepthTest); My.Check();
				GL.DepthMask(false); My.Check();

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

		public void BuildRenderList(IList<IRenderable> possibleRenderables, Camera camera)
		{
			// without Parallel.ForEach = 130 fps
			// with ConcurrentBag = 180 fps
			// with ConcurrentQueue = 200 fps
			// with lock List = 200 fps

			var frustum = camera.GetFrustum();
			int totalPossible = possibleRenderables.Count;

			toRenderCount = 0;
			lock (possibleRenderables)
			{
				Parallel.ForEach(possibleRenderables, (renderable) =>
					{
						if (renderable.ShouldRenderInContext(RenderContext))
						{
							if (renderable.ForcePassFrustumCulling)
							{
								renderable.CameraRenderStatusFeedback(camera, RenderStatus.RenderedForced);
								var newIndex = Interlocked.Increment(ref toRenderCount);
								toRender[newIndex - 1] = renderable;
							}
							else
							{
								var bounds = renderable.GetCameraSpaceBounds(camera.Transform.Position);
								if (
									frustum.SphereVsFrustum(bounds.Center, bounds.Extents.Length)
									&& frustum.VolumeVsFrustum(bounds)
								)
								{
									renderable.CameraRenderStatusFeedback(camera, RenderStatus.RenderedAndVisible);
									var newIndex = Interlocked.Increment(ref toRenderCount);
									toRender[newIndex - 1] = renderable;
								}
								else
								{
									renderable.CameraRenderStatusFeedback(camera, RenderStatus.NotRendered);
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
				Array.Sort(toRender, 0, toRenderCount, comparer);
			}
		}

		class RenderableDistanceComparer : IComparer<IRenderable>
		{
			WorldPos viewPointPosition;
			Vector3 viewPointPosition_vec3;

			public RenderableDistanceComparer(WorldPos viewPointPosition)
			{
				this.viewPointPosition = viewPointPosition;
				this.viewPointPosition_vec3 = viewPointPosition.ToVector3();
			}

			public int Compare(IRenderable x, IRenderable y)
			{
				if (ReferenceEquals(y, x)) return 0;
				var distX = x.GetCameraSpaceBounds(viewPointPosition).Center.DistanceSqr(viewPointPosition_vec3);
				var distY = y.GetCameraSpaceBounds(viewPointPosition).Center.DistanceSqr(viewPointPosition_vec3);
				if (distX == distY) return 0;
				if (distX > distY) return -1;
				return +1;
			}
		}
	}
}