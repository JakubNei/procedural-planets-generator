using System;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Threading;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using MyEngine.Components;

namespace MyEngine
{


    public class EngineMain : GameWindow
    {


        public InputSystem Input { get; private set; }
        public AssetSystem Asset { get; private set; }
        List<SceneSystem> scenes = new List<SceneSystem>();

        public EngineMain() : base(
            1400,
            900,
            new GraphicsMode(),
            "MyEngine",
            GameWindowFlags.Default,
            DisplayDevice.Default,
            4,
            0,
            GraphicsContextFlags.ForwardCompatible
        )
        {

            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.AboveNormal;

            VSync = VSyncMode.Off;
            TargetRenderFrequency = 0;

            //Texture2D.InitTexture2D();
            ubo = new UniformBlock();
            //new PhysicsUsage.PhysicsManager();
            Asset = new AssetSystem();
            Input = new InputSystem(this);

            stopwatchSinceStart.Restart();

            StartSec();


            //{
            //    var winForm = new Panels.DebugValuesTable();
            //    winForm.Show();
            //}
            renderManager = new RenderManager(eventSystem);
        }
        RenderManager renderManager;
        Events.EventSystem eventSystem = new Events.EventSystem();

        System.Diagnostics.Stopwatch stopwatchSinceStart = new System.Diagnostics.Stopwatch();


        internal static UniformBlock ubo;
        Shader finalDrawShader;

        public SceneSystem AddScene()
        {
            var s = new SceneSystem(this);
            AddScene(s);
            return s;
        }


        protected override void OnLoad(System.EventArgs e)
        {
            finalDrawShader = Factory.GetShader("internal/finalDraw.glsl");

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
            GL.Enable(EnableCap.Multisample);


            //GL.ClearColor(System.Drawing.Color.MidnightBlue);
            GL.ClearColor(System.Drawing.Color.Black);

        }



        protected override void OnUnload(EventArgs e)
        {

        }

        protected override void OnResize(EventArgs e)
        {
            var resizeEvent = new Events.WindowResized(Width, Height);
            eventSystem.Raise(resizeEvent);
            Debug.Info("Window resized to: width:" + resizeEvent.NewPixelWidth + " height:" + resizeEvent.NewPixelHeight);
        }

        public void AddScene(SceneSystem scene)
        {
            eventSystem.PassEventsTo(scene.EventSystem);
            scenes.Add(scene);
        }

        bool drawNormalGBuffer = false;
        bool drawGBufferContents = false;

        public override void Exit()
        {
            if (IsDisposed) return;
            if (IsExiting) return;
            base.Exit();
        }

        bool autoBuildRenderList = true;

        void StartSec()
        {
            eventThreadTime.Restart();

            {
                var t = new Thread(() =>
                {
                    while (this.IsDisposed == false && this.IsExiting == false)
                    {
                        if (Input.GetKeyDown(OpenTK.Input.Key.F4)) autoBuildRenderList = !autoBuildRenderList;
                        if (autoBuildRenderList)
                        {
                            if (scenes.Count > 0)
                            {
                                Debug.Tick("BuildRenderList");
                                foreach (var scene in scenes)
                                {
                                    var camera = scene.mainCamera;
                                    var dataToRender = scene.DataToRender;
                                    if (camera != null && dataToRender != null)
                                    {
                                        renderManager.BuildRenderList(dataToRender.Renderers, camera);
                                    }
                                }
                            }
                        }
                    }
                });

                t.Priority = ThreadPriority.Highest;
                t.IsBackground = true;
                t.Start();
            }

            {
                var t = new Thread(() =>
                {
                    while (this.IsDisposed == false && this.IsExiting == false)
                    {
                        EventThreadMain();
                    }
                });

                t.Priority = ThreadPriority.Highest;
                t.IsBackground = true;
                t.Start();
            }
            
        }

        System.Diagnostics.Stopwatch eventThreadTime = new System.Diagnostics.Stopwatch();
        ManualResetEvent onRenderGameWaitHandle = new ManualResetEvent(false);


        Queue<DateTime> frameTimes1sec = new Queue<DateTime>();
        void EventThreadMain()
        {
            Debug.Tick("eventThread");

            var now = DateTime.Now;
            frameTimes1sec.Enqueue(now);
            while ((now - frameTimes1sec.Peek()).TotalSeconds > 1) frameTimes1sec.Dequeue();

            var deltaTime = eventThreadTime.ElapsedMilliseconds / 1000.0;
            deltaTime = 1.0 / frameTimes1sec.Count;

            eventThreadTime.Restart();

            this.Title = string.Join("\t  ", Debug.stringValues.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Key + ":" + kvp.Value).ToArray());

            if (this.Focused) Input.Update();

            eventSystem.Raise(new MyEngine.Events.InputUpdate(deltaTime));

        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //Debug.AddValue("fps", (1 / e.Time).ToString());
            Debug.Tick("renderThread");

            
            
            ubo.engine.totalElapsedSecondsSinceEngineStart = (float)stopwatchSinceStart.Elapsed.TotalSeconds;
            ubo.engine.gammaCorrectionTextureRead = 2.2f;
            ubo.engine.gammaCorrectionFinalColor = 1 / 2.2f;


            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var countMeshesRendered = 0;



            {
                var scene = scenes[0];
                var camera = scene.mainCamera;
                var dataToRender = scene.DataToRender;


                if (Input.GetKeyDown(OpenTK.Input.Key.F5)) Factory.ReloadAllShaders();
                if (Input.GetKeyDown(OpenTK.Input.Key.F6)) renderManager.shadowsEnabled = !renderManager.shadowsEnabled;
                if (Input.GetKeyDown(OpenTK.Input.Key.F7)) renderManager.debugBounds = !renderManager.debugBounds;
                if (Input.GetKeyDown(OpenTK.Input.Key.F8)) renderManager.enablePostProcessEffects = !renderManager.enablePostProcessEffects;
                if (Input.GetKeyDown(OpenTK.Input.Key.F9)) drawGBufferContents = !drawGBufferContents;
                if (Input.GetKeyDown(OpenTK.Input.Key.F10)) drawNormalGBuffer = !drawNormalGBuffer;
                if (Input.GetKeyDown(OpenTK.Input.Key.F11)) renderManager.drawLines = !renderManager.drawLines;



                renderManager.SkyboxCubeMap = scene.skyBox;
                renderManager.RenderAll(ubo, camera, dataToRender.Lights, camera.postProcessEffects);
            }


            var gBuffer = renderManager.GBuffer;

            // FINAL DRAW TO SCREEN
            {
                //DebugDrawTexture(gBuffer.finalTextureToRead);

                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.CullFace);
                GL.Disable(EnableCap.Blend);

                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
                GL.Viewport(0, 0, Width, Height);
                //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

                finalDrawShader.Uniforms.Set("finalDrawTexture", gBuffer.finalTextureToRead);
                finalDrawShader.Bind();

                Mesh.Quad.Draw();
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
            if (drawNormalGBuffer) gBuffer.DebugDrawNormal();
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

            Debug.AddValue("countMeshesRendered", countMeshesRendered + "/" + renderManager.CountRenderablesRendered);

        }

    }
}