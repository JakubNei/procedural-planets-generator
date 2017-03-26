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
		Shader FinalDrawShader => Factory.GetShader("internal/finalDraw.glsl");

		public bool DrawLines { get { return debug.CommonCVars.DebugRenderWithLines().Bool; } }
		public bool EnablePostProcessEffects { get { return debug.CommonCVars.EnablePostProcessEffects().Bool; } }
		public bool DebugBounds { get { return debug.CommonCVars.DrawDebugBounds().Bool; } }
		public bool ShadowsEnabled { get { return debug.CommonCVars.ShadowsDisabled().Bool == false; } }

		Factory Factory => Factory.Instance;
		readonly MyDebug debug;

		public RenderManager(Events.EventSystem eventSystem, MyDebug debug)
		{
			this.debug = debug;

			eventSystem.On<Events.WindowResized>(evt =>
			{
				if (GBuffer != null) GBuffer.Dispose();
				GBuffer = new DeferredGBuffer(evt.NewPixelWidth, evt.NewPixelHeight);
			});
			rasterizer = new SoftwareDepthRasterizer(200, 100);



			debug.GetCVar("show rasterizer contents").ToogledByKey(OpenTK.Input.Key.M).OnChanged += (c) =>
			{
				if (c.Bool) rasterizer?.Show();
				else rasterizer?.Hide();
			};
		}

		public void RenderAll(UniformBlock ubo, Camera camera, IList<ILight> allLights, IEnumerable<IPostProcessEffect> postProcessEffect)
		{
			camera.UploadCameraDataToUBO(ubo); // bind camera view params and matrices only once

			RenderGBuffer(ubo, camera);

			RenderLights(ubo, camera, allLights);

			if (DrawLines) GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill); MyGL.Check();

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

			if (debug.CommonCVars.DebugDrawNormalBufferContents().Bool) GBuffer.DebugDrawNormal();
			if (debug.CommonCVars.DebugDrawGBufferContents().Bool) GBuffer.DebugDrawContents();
			//if (drawShadowMapContents) DebugDrawTexture(shadowMap.depthMap, new Vector4(0.5f, 0.5f, 1, 1), new Vector4(0.5f,0.5f,0,1), 1, 0);

			ErrorCode glError;
			while ((glError = GL.GetError()) != ErrorCode.NoError)
				debug.Error("GL Error: " + glError, false);
		}

		private void RenderGBuffer(UniformBlock ubo, Camera camera)
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

					var shader = Factory.GetShader("internal/deferred.skybox.shader");
					shader.Uniforms.Set("skyboxCubeMap", SkyboxCubeMap);
					shader.Bind();

					Factory.SkyBoxMesh.Draw();
					GL.DepthRange(0, 1); MyGL.Check();
				}


				if (DrawLines)
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
					for (int i = 0; i < toRenderRenderablesCount; i++)
					{
						var renderable = ToRenderRenderables[i];
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
					var renderable = ToRenderRenderables[i];
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

		SoftwareDepthRasterizer rasterizer;


		const int maxToRenderAtOnce = 10000;

		IRenderable[] passedFrustumCulling = new IRenderable[maxToRenderAtOnce];
		IRenderable[] passedRasterizationCulling = new IRenderable[maxToRenderAtOnce];
		IRenderable[] ToRenderRenderables => passedRasterizationCulling;
		int toRenderRenderablesCount = 0;

		float[] distancesToCamera = new float[maxToRenderAtOnce];
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

			// clear references to IRenderables in part of the array that will not be used, there is a chance that it might hang onto references thus stopping GC from collecting IRenderables
			if (lastTotalPossible > totalPossible)
			{
				Array.Clear(passedFrustumCulling, totalPossible, lastTotalPossible - totalPossible);
				Array.Clear(passedRasterizationCulling, totalPossible, lastTotalPossible - totalPossible);
			}
			lastTotalPossible = totalPossible;

			rasterizer.Clear();

			int passedFrustumCullingIndex = 0;

			lock (possibleRenderables)
			{
				Parallel.ForEach(possibleRenderables, (renderable) =>
				{
					if (renderable != null && renderable.ShouldRenderInContext(camera, RenderContext))
					{
						if (renderable.ForcePassCulling)
						{
							renderable.SetCameraRenderStatusFeedback(camera, RenderStatus.RenderedAndVisible);
							var newIndex = Interlocked.Increment(ref passedFrustumCullingIndex);
							passedFrustumCulling[newIndex - 1] = renderable;
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
								renderable.SetCameraRenderStatusFeedback(camera, RenderStatus.RenderedAndVisible);
								var newIndex = Interlocked.Increment(ref passedFrustumCullingIndex);
								passedFrustumCulling[newIndex - 1] = renderable;
								rasterizer?.AddTriangles(renderable.GetCameraSpaceOccluderTriangles(camera));
							}
							else
							{
								renderable.SetCameraRenderStatusFeedback(camera, RenderStatus.NotRendered);
							}
						}
					}
				});
			}



			debug.AddValue("rendering / meshes / total possible", totalPossible);
			debug.AddValue("rendering / meshes / passed frustum culling", passedFrustumCullingIndex);

			int passedRasterizationCullingIndex = 0;
			Parallel.For(0, passedFrustumCullingIndex, (i) =>
			  {
				  var renderable = passedFrustumCulling[i];
				  var bounds = renderable.GetCameraSpaceBounds(camera);
				  if (rasterizer.BoundsVisible(bounds))
				  {
					  var newIndex = Interlocked.Increment(ref passedRasterizationCullingIndex);
					  passedRasterizationCulling[newIndex - 1] = renderable;
					  distancesToCamera[newIndex - 1] = bounds.depth;
				  }
			  });

			debug.AddValue("rendering / meshes / passed rasterization culling", passedRasterizationCullingIndex);


			if (debug.CommonCVars.SortRenderers())
			{
				Array.Sort(distancesToCamera, passedRasterizationCulling, 0, passedRasterizationCullingIndex, Comparer<float>.Default); // sort renderables so closest to camera are first
			}

			toRenderRenderablesCount = passedRasterizationCullingIndex;
		}

		//IComparer<IRenderable> renderableDistanceComparer = new RenderableDistanceComparer();
		//class RenderableDistanceComparer : IComparer<IRenderable>
		//{
		//	//Less than zero = x is less than y.
		//	//Zero = x equals y.
		//	//Greater than zero = x is greater than y.
		//	public int Compare(IRenderable x, IRenderable y)
		//	{
		//		if (ReferenceEquals(y, x)) return 0;
		//		var distX = x.GetCameraSpaceBounds(viewPointPosition).Center.LengthFast;
		//		var distY = y.GetCameraSpaceBounds(viewPointPosition).Center.LengthFast;
		//		return distX.CompareTo(distY);
		//	}
		//}
	}
}