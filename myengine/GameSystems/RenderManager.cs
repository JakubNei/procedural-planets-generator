using MyEngine;
using MyEngine.Components;
using Neitri;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
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
		IRenderable[] toRender = new IRenderable[1000];

		[Dependency]
		Debug debug;

		[Dependency]
		Factory factory;

		public bool drawLines { get { return debug.CVar("debugRenderWithLines").Bool; } }
		public bool enablePostProcessEffects { get { return debug.CVar("enablePostProcessEffects").Bool; } }
		public bool debugBounds { get { return debug.CVar("debugBounds").Bool; } }
		public bool shadowsEnabled { get { return debug.CVar("shadowsDisabled").Bool == false; } }

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
			if (drawLines) GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			RenderPostProcessEffects(ubo, postProcessEffect);
		}

		public void RenderGBuffer(UniformBlock ubo, Camera camera)
		{
			// G BUFFER GRAB PASS
			{
				GBuffer.BindAllFrameBuffersForDrawing();

				GL.Enable(EnableCap.DepthTest);
				GL.DepthMask(true);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

				// SKYBOX PASS
				if (SkyboxCubeMap != null)
				{
					GL.DepthRange(0.999, 1);
					GL.DepthMask(false);

					var shader = factory.GetShader("internal/deferred.skybox.shader");
					shader.Uniforms.Set("skyboxCubeMap", SkyboxCubeMap);
					shader.Bind();

					factory.SkyBoxMesh.Draw();
					GL.DepthRange(0, 1);
				}

				if (drawLines) GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
				else GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

				// RENDER ALL OBJECTS
				{
					GL.DepthMask(true);

					GL.Enable(EnableCap.CullFace);
					GL.Disable(EnableCap.Blend);
					GL.CullFace(CullFaceMode.Back);
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

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
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

						GL.Enable(EnableCap.DepthTest);
						GL.DepthMask(true);

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
						GL.Disable(EnableCap.CullFace);
						//GL.CullFace(CullFaceMode.Back);

						GL.Disable(EnableCap.DepthTest);
						GL.DepthMask(false);

						light.UploadUBOdata(camera, ubo, lightIndex);

						var shader = factory.GetShader("internal/deferred.oneLight.shader");
						GBuffer.BindForLightPass(shader);
						if (shadowsEnabled && light.HasShadows)
						{
							shadowMap.BindUniforms(shader);
						}

						shader.Bind();

						GL.Enable(EnableCap.Blend);
						//GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
						//GL.BlendFunc(BlendingFactorSrc.SrcColor, BlendingFactorDest.SrcColor);
						GL.BlendEquation(BlendEquationMode.FuncAdd);
						GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
						factory.QuadMesh.Draw();
						GL.Disable(EnableCap.Blend);
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
				GL.Disable(EnableCap.DepthTest);
				GL.Disable(EnableCap.CullFace);
				GL.Disable(EnableCap.Blend);

				GL.Disable(EnableCap.DepthTest);
				GL.DepthMask(false);

				foreach (var pe in postProcessEffects)
				{
					if (pe.IsEnabled == false) continue;
					pe.BeforeBindCallBack();
					GBuffer.BindForPostProcessEffects(pe);
					pe.Shader.Bind();
					factory.QuadMesh.Draw();
				}
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
			//lock (possibleRenderables)
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
			debug.AddValue("countMeshesRendered", toRenderCount + "/" + totalPossible);
			/*
			if (Debug.CVar("sortRenderers").Bool)
			{
				var comparer = new RenderableDistanceComparer(camera.ViewPointPosition);
				toRender.Sort(comparer);
			}
			*/
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