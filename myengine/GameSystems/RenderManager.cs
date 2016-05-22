using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using MyEngine;
using MyEngine.Components;

namespace MyEngine
{
    public class RenderManager
    {
        public int CountRenderablesRendered
        {
            get
            {
                return toRender.Count;
            }
        }

        public object RenderContext { get; set; } = Components.RenderContext.Geometry;

        public DeferredGBuffer GBuffer { get; private set; }

        public Cubemap SkyboxCubeMap { get; set; }

        List<IRenderable> toRender = new List<IRenderable>();

        public bool drawLines { get { return Debug.Value("debugRenderWithLines").Bool; } }
        public bool enablePostProcessEffects { get { return Debug.Value("enablePostProcessEffects").Bool; } }
        public bool debugBounds { get { return Debug.Value("disablePostProcessEffects").Bool; } }
        public bool shadowsEnabled { get { return Debug.Value("shadowsDisabled").Bool == false; } }

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

                    var shader = Factory.GetShader("internal/deferred.skybox.shader");
                    shader.Uniforms.Set("skyboxCubeMap", SkyboxCubeMap);
                    shader.Bind();

                    Mesh.SkyBox.Draw();
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
                    lock (this)
                    {
                        for (int i = 0; i < toRender.Count; i++)
                        {
                            var renderable = toRender[i];
                            renderable.Material.BeforeBindCallback();
                            renderable.Material.Uniforms.SendAllUniformsTo(renderable.Material.GBufferShader.Uniforms);
                            renderable.Material.GBufferShader.Bind();
                            renderable.UploadUBOandDraw(camera, ubo);
                        }
                    }
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
                    #endregion


                    camera.UploadDataToUBO(ubo); // bind camera view params

                    // G BUFFER LIGHT PASS

                    {
                        GL.Disable(EnableCap.CullFace);
                        //GL.CullFace(CullFaceMode.Back);

                        GL.Disable(EnableCap.DepthTest);
                        GL.DepthMask(false);


                        light.UploadUBOdata(camera, ubo, lightIndex);

                        var shader = Factory.GetShader("internal/deferred.oneLight.shader");
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
                        Mesh.Quad.Draw();
                        GL.Disable(EnableCap.Blend);

                    }

                }

            }

            #endregion
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
                    Mesh.Quad.Draw();
                }
            }
        }
        public void BuildRenderList(IEnumerable<IRenderable> possibleRenderables, Camera camera)
        {
            var newToRender = new List<IRenderable>();
            var frustrumPlanes = camera.GetFrustumPlanes();
            lock (possibleRenderables)
            {
                foreach (var renderable in possibleRenderables)
                {
                    if (renderable.ShouldRender(RenderContext))
                    {
                        if (renderable.ForcePassFrustumCulling || GeometryUtility.TestPlanesAABB(frustrumPlanes, renderable.GetBounds(camera.Transform.Position)))
                        {
                            newToRender.Add(renderable);
                            if (renderable.ForcePassFrustumCulling) renderable.SetCameraRenderStatus(camera, RenderStatus.RenderedForced);
                            else renderable.SetCameraRenderStatus(camera, RenderStatus.RenderedAndVisible);
                        }
                        else
                        {
                            renderable.SetCameraRenderStatus(camera, RenderStatus.NotRendered);
                        }
                    }
                }
            }
            var comparer = new RenderableDistanceComparer(camera.ViewPointPosition);
            newToRender.Sort(comparer);
            this.toRender = newToRender;
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
                var distX = x.GetBounds(viewPointPosition).Center.DistanceSqr(viewPointPosition_vec3);
                var distY = y.GetBounds(viewPointPosition).Center.DistanceSqr(viewPointPosition_vec3);
                if (distX == distY) return 0;
                if (distX > distY) return 1;
                else return -1;
            }
        }
    }
}