using System;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using MyEngine.Components;

namespace MyEngine
{

    
    public class EngineMain : GameWindow
    {


        public InputSystem Input { get; private set; }
        List<SceneSystem> scenes = new List<SceneSystem>();

        public EngineMain()
            : base(1400, 900,
            new GraphicsMode(), "MyEngine", GameWindowFlags.Default,
            DisplayDevice.Default, 3, 2,
            GraphicsContextFlags.Default)
            //GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        {

            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.High;

            VSync = VSyncMode.Off;

            Texture2D.InitTexture2D();
            UnloadFactory.Set(ref ubo, new UniformBlock());
            new PhysicsUsage.PhysicsManager();
            Input = new InputSystem(this);
        }

       


        public Cubemap skyboxCubeMap;


        internal UnloadFactory unloadFactory = new UnloadFactory();
        internal static UniformBlock ubo;
        Mesh quadMesh;
        Mesh skyboxMesh;

        public SceneSystem AddScene()
        {
            var s = new SceneSystem(this);
            AddScene(s);
            return s;
        }


        protected override void OnLoad(System.EventArgs e)
        {
            
            quadMesh = Factory.GetMesh("internal/quad.obj");
            skyboxMesh = Factory.GetMesh("internal/skybox.obj");

            foreach (StringName r in System.Enum.GetValues(typeof(StringName)))
            {
                if (r == StringName.Extensions) break;
                Debug.Info(r.ToString() + ": " + GL.GetString(r));
            }
            //Debug.Info(StringName.Extensions.ToString() + ": " + GL.GetString(StringName.Extensions));


            this.VSync = VSyncMode.Off;
            
            // Other state
            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            //GL.ClearColor(System.Drawing.Color.MidnightBlue);
            GL.ClearColor(System.Drawing.Color.Black);

        }



        protected override void OnUnload(EventArgs e)
        {
            PhysicsUsage.PhysicsManager.instance.CleanUp();
            foreach (var i in UnloadFactory.unloadables)
            {
                i.Unload();
            }
        }




        DeferredGBuffer gBuffer;


        protected override void OnResize(EventArgs e)
        {
            foreach (var scene in scenes)
            {
                scene.mainCamera.SetSize(ClientSize.Width, ClientSize.Height);
            }

            UnloadFactory.Set(ref gBuffer, new DeferredGBuffer(Width, Height));

            //screenCenter = new Point(Bounds.Left + (Bounds.Width / 2), Bounds.Top + (Bounds.Height / 2));
            //windowCenter = new Point(Width / 2, Height / 2);

            Debug.Info("Windows resized to: width:" + ClientSize.Width + " height:" + ClientSize.Height);
        }

        void DebugDrawTexture(Texture2D texture, float valueScale = 1, float valueOffset = 0)
        {
            DebugDrawTexture(texture, Vector4.One, Vector4.Zero, valueScale, valueOffset);
        }
        void DebugDrawTexture(Texture2D texture, Vector4 positionScale, Vector4 positionOffset, float valueScale = 1, float valueOffset = 0)
        {

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.Viewport(0, 0, Width, Height);
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            var shader = Factory.GetShader("internal/debugDrawTexture.shader");
            shader.Bind();

            shader.SetUniform("debugDrawTexture", texture);
            shader.SetUniform("debugDrawTexturePositionScale", positionScale);
            shader.SetUniform("debugDrawTexturePositionOffset", positionOffset);
            shader.SetUniform("debugDrawTextureScale", valueScale);
            shader.SetUniform("debugDrawTextureOffset", valueOffset);

            quadMesh.Draw();
        }


        public void AddScene(SceneSystem scene)
        {
            scenes.Add(scene);
        }

        bool drawShadowMapContents = false;
        bool drawGBufferContents = false;
        bool drawLines = false;
        bool debugBounds = true;
        bool shadowsEnabled = true;

        protected override void OnRenderFrame(FrameEventArgs e)
        {

            this.Title = string.Join("\t  ", Debug.stringValues.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Key + ":" + kvp.Value).ToArray());
            

            Debug.AddValue("FPS", (1 / e.Time).ToString("0."));

            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (this.Focused) Input.Update();


            var scene = scenes[0];

            var allLights = scene.Lights;
            var camera = scene.mainCamera;
            var allEntitys = scene.Entities;


            var countMeshesRendered = 0;


            if (Input.GetKeyDown(OpenTK.Input.Key.F10)) drawLines = !drawLines;
            if (Input.GetKeyDown(OpenTK.Input.Key.F9)) drawGBufferContents = !drawGBufferContents;
            if (Input.GetKeyDown(OpenTK.Input.Key.F8)) drawShadowMapContents = !drawShadowMapContents;
            if (Input.GetKeyDown(OpenTK.Input.Key.F7)) debugBounds = !debugBounds;
            if (Input.GetKeyDown(OpenTK.Input.Key.F6)) shadowsEnabled = !shadowsEnabled;
            if (Input.GetKeyDown(OpenTK.Input.Key.F5)) Factory.ReloadAllShaders();

            PhysicsUsage.PhysicsManager.instance.Update((float)e.Time);


            scene.EventSystem.Raise(new MyEngine.Events.GraphicsUpdate(e.Time));


            var frustrumPlanes = camera.GetFrustumPlanes();

            var allRenderers = new List<Renderer>();
            foreach (var go in allEntitys)
            {
                allRenderers.AddRange(go.GetComponents<Renderer>());
            }


            camera.UploadDataToUBO(ubo); // bind camera view params
            //GL.BeginQuery(QueryTarget.)

            // G BUFFER GRAB PASS
            {
                gBuffer.BindAllFrameBuffersForDrawing();


                GL.Enable(EnableCap.DepthTest);
                GL.DepthMask(true);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

                // SKYBOX PASS
                {
                    GL.DepthRange(0.999, 1);
                    GL.DepthMask(false);

                    var shader = Factory.GetShader("internal/deferred.skybox.shader");
                    shader.SetUniform("skyboxCubeMap", skyboxCubeMap);
                    shader.Bind();

                    skyboxMesh.Draw();
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

                    foreach (var renderer in allRenderers)
                    {
                        if (renderer.ShouldRenderGeometry)
                        {
                            if(renderer.CanBeFrustumCulled == false || GeometryUtility.TestPlanesAABB(frustrumPlanes, renderer.bounds)) {
                                renderer.material.BindUniforms();
                                renderer.material.gBufferShader.Bind();
                                renderer.UploadUBOandDraw(camera, ubo);
                                countMeshesRendered++;
                            }
                        }
                    }

                }

                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            }

         
            #region Lights rendering

            int lightIndex = 0;


            foreach (var light in allLights)
            {
                
                var shadowMap = light.shadowMap;

                // SHADOW MAAPING
                if (shadowsEnabled && light.hasShadows)
                {

                    //GL.Enable(EnableCap.CullFace);
                    //GL.CullFace(CullFaceMode.Back);

                    shadowMap.FrameBufferForWriting();

                    GL.Enable(EnableCap.DepthTest);
                    GL.DepthMask(true);

                    shadowMap.Clear();



                    shadowMap.shadowViewCamera.UploadDataToUBO(ubo);


                    foreach (var renderer in allRenderers)
                    {
                        if (renderer.ShouldCastShadows)
                        {
                            //if (renderer.CanBeFrustumCulled == false || GeometryUtility.TestPlanesAABB(frustrumPlanes, renderer.bounds)) 
                            {
                                renderer.material.BindUniforms();
                                renderer.material.depthGrabShader.Bind();
                                renderer.UploadUBOandDraw(shadowMap.shadowViewCamera, ubo);
                            }
                        }
                    }

                }


                camera.UploadDataToUBO(ubo); // bind camera view params

                // G BUFFER LIGHT PASS

                {
                    GL.Disable(EnableCap.CullFace);
                    //GL.CullFace(CullFaceMode.Back);

                    GL.Disable(EnableCap.DepthTest);
                    GL.DepthMask(false);


                    light.UploadUBOdata(ubo, lightIndex);

                    var shader = Factory.GetShader("internal/deferred.oneLight.shader");
                    gBuffer.BindGBufferTexturesTo(shader);
                    if (shadowsEnabled && light.hasShadows)
                    {
                        shadowMap.BindUniforms(shader);
                    }

                    shader.Bind();

                    GL.Enable(EnableCap.Blend);
                    //GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
                    //GL.BlendFunc(BlendingFactorSrc.SrcColor, BlendingFactorDest.SrcColor);                    
                    GL.BlendEquation(BlendEquationMode.FuncAdd);
                    GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
                    quadMesh.Draw();
                    GL.Disable(EnableCap.Blend);

                }

                lightIndex++;
            }

            #endregion
            
            
            // POST PROCESS EFFECTs
            /*{
                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.CullFace);
                GL.Disable(EnableCap.Blend);

                GL.Disable(EnableCap.DepthTest);
                GL.DepthMask(false);

                foreach (var shader in camera.postProcessEffects)
                {
                    gBuffer.BindForLightingPass(shader);
                    shader.Bind();
                    quad.Draw();
                    gBuffer.SwapFinalTextureTarget();
                }
            }*/

            // FINAL DRAW TO SCREEN
            {
                DebugDrawTexture(gBuffer.finalTextureToWriteTo);
            }

            /*if(debugBounds)
            {
                var allColiders = new List<BoxCollider>();
                foreach (var go in Factory.allEntitys)
                {
                    allColiders.AddRange(go.GetComponents<BoxCollider>());
                }
                GL.DepthMask(false);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.CullFace);
                GL.Disable(EnableCap.Blend);
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
            if (drawGBufferContents) gBuffer.DebugDrawContents();
            //if (drawShadowMapContents) DebugDrawTexture(shadowMap.depthMap, new Vector4(0.5f, 0.5f, 1, 1), new Vector4(0.5f,0.5f,0,1), 1, 0);


            /*
            {
                var shader = Factory.GetShader("internal/forward.allLights.shader");
                gBuffer.BindForWriting();

                // transparent pass, must enable blending
                GL.Enable(EnableCap.DepthTest);
                GL.DepthMask(false);
                GL.Disable(EnableCap.CullFace);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);


                shader.Bind();
                //shader.SetParam(Shader.screenSizeLocationName, new Vector2(Width, Height));
                camera.BindUniforms(shader);
                //Light.BindAll(shader);

                foreach (var go in Factory.allEntitys)
                {
                    foreach (var renderer in go.GetComponents<Renderer>())
                    {
                        renderer.Draw(shader, camera);
                    }
                }
            }*/


            SwapBuffers();

            Debug.AddValue("countMeshesRendered", countMeshesRendered.ToString());

        }


    }
}