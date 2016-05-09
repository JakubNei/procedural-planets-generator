using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using OpenTK;

using MyEngine;
using MyEngine.Components;

namespace MyGame
{
    class Program
    {

        public static Entity sunEntity = null;
        public static Entity sunTarget = null;
        public static FirstPersonCamera fpc;

        [STAThread]
        public static void Main()
        {
            //string[] args = System.Environment.GetCommandLineArgs();
            using (var engine = new EngineMain())
            {

                var scene = engine.AddScene();

                new DebugKeys(scene);




                {

                    var entity = scene.AddEntity();
                    fpc = entity.AddComponent<FirstPersonCamera>();

                    var cam = scene.mainCamera = entity.AddComponent<Camera>();

                    // post process effects
                    {
                        entity.AddComponent<Bloom>();

                        entity.AddComponent<Tonemapping>();

                        var gr = entity.AddComponent<GodRays>();
                        gr.lightWorldRadius = 1000;
                        entity.EventSystem.Register((MyEngine.Events.GraphicsUpdate e) =>
                        {
                            var mp = cam.GetViewMat() * cam.GetProjectionMat();
                            var p = Vector4.Transform(new Vector4(sunEntity.Transform.Position, 1), mp);
                            gr.lightScreenPos = (p.Xyz / p.W) / 2 + Vector3.One / 2;
                            gr.lightWorldPos = sunEntity.Transform.Position;
                        });

                    }

                    string skyboxName = "skybox/generated/";
                    engine.skyboxCubeMap = Factory.GetCubeMap(new[] {
                        skyboxName + "left.png",
                        skyboxName + "right.png",
                        skyboxName + "top.png",
                        skyboxName + "bottom.png",
                        skyboxName + "front.png",
                        skyboxName + "back.png"
                    });


                    entity.Transform.Position = (new Vector3(1, 1, 1)) * 100;
                    entity.Transform.LookAt(new Vector3(0, 0, 100));

                    //engine.camera.entity.AddComponent<SSAO>();

                }



                /*
                {
                    var entity = scene.AddEntity();
                    var mr = entity.AddComponent<MeshRenderer>();
                    mr.Mesh = Factory.GetMesh("cube.obj");
                    entity.Transform.Scale = new Vector3(100, 10, 10);
                }
                */


                
                var proceduralPlanets = new ProceduralPlanets(scene);


                {
                    var entity = sunEntity = scene.AddEntity();
                    entity.Transform.Scale *= 1000;
                    entity.Transform.Position = new Vector3(-2000, -2000, 100);

                    var renderer = entity.AddComponent<MeshRenderer>();
                    renderer.Mesh = Factory.GetMesh("sphere_smooth.obj");

                    var mat = renderer.Material = new Material();
                    mat.GBufferShader = Factory.GetShader("shaders/sun.glsl");
                    mat.Uniforms.Set("param_turbulenceColorGradient", Factory.GetTexture2D("textures/fire_gradient.png"));
                    mat.Uniforms.Set("param_turbulenceMap", Factory.GetTexture2D("textures/turbulence_map.png"));
                    mat.Uniforms.Set("param_surfaceDiffuse", Factory.GetTexture2D("textures/sun_surface_d.png"));

                }

                {
                    var entity = scene.AddEntity();
                    var light = entity.AddComponent<Light>();
                    light.LighType = LightType.Directional;
                    light.color = Vector3.One * 1f;
                    light.Shadows = LightShadows.None;

                    scene.EventSystem.Register((MyEngine.Events.GraphicsUpdate e) =>
                    {
                        entity.Transform.Position = sunEntity.Transform.Position;
                        var p = proceduralPlanets.planets.FirstOrDefault();
                        if (p != null)
                        {
                            entity.Transform.LookAt(p.Transform.Position);
                        }
                    });

                    
                }
                
 
                engine.Run();

            }
        }
    }
}
