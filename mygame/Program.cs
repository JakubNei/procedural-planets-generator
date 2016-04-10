using System;
using System.Collections.Generic;
using System.Text;

using OpenTK;

using MyEngine;
using MyEngine.Components;

namespace MyGame
{
    class Program
    {
        [STAThread]
        public static void Main()
        {
            //string[] args = System.Environment.GetCommandLineArgs();
            using (var engine = new EngineMain())
            {

                var scene = engine.AddScene();

                {

                    var entity = scene.AddEntity();
                    entity.AddComponent<FirstPersonCamera>();

                    scene.mainCamera = entity.AddComponent<Camera>();
                    //var c=go.AddComponent<BoxCollider>(); c.size = Vector3.One*5;
                    string skyboxName = "skybox/purple_nebula/";
                    engine.skyboxCubeMap = Factory.GetCubeMap(new ResourcePath[] {
                        skyboxName +"left.png",
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


                new ProceduralPlanets(scene);
                


                {
                    var entity = scene.AddEntity();
                    //go.AddComponent<Rigidbody>();
                    var renderer = entity.AddComponent<MeshRenderer>();
                    renderer.Mesh = Factory.GetMesh("internal/cube_bigUVs.obj");

                    renderer.Material.albedoTexture = Factory.GetTexture2D("textures/grass_d.jpg");
                    renderer.Material.normalMap = Factory.GetTexture2D("textures/grass_n.jpg");
                    renderer.Material.depthMap = Factory.GetTexture2D("textures/grass_d.jpg");

                    //renderer.material.albedoTexture = Factory.GetTexture2D("textures/cobblestonesDiffuse.bmp");
                    //renderer.material.normalMap = Factory.GetTexture2D("textures/cobblestonesNormal.bmp");
                    //renderer.material.depthMap = Factory.GetTexture2D("textures/cobblestonesDepth.bmp");

                    //renderer.material.albedoTexture = Factory.GetTexture2D("textures/stonewallDiffuse.bmp");
                    //renderer.material.normalMap = Factory.GetTexture2D("textures/stonewallNormal.bmp");
                    //renderer.material.depthMap = Factory.GetTexture2D("textures/stonewallDepth.bmp");


                    entity.Transform.Translate(new Vector3(0, -10, 0));
                    entity.Transform.Scale = new Vector3(1000, 1, 1000);
                    entity.AddComponent<BoxCollider>();

                }
                


                /*{
                    var go = new Entity();
                    //go.AddComponent<Rigidbody>();
                    var renderer = go.AddComponent<MeshRenderer>();
                    renderer.mesh = Factory.GetMesh("internal/cube.obj");
                    go.transform.Translate(new Vector3(0, 0, 2));
                    go.AddComponent<BoxCollider>();
                    renderer.material.albedoTexture = Factory.GetTexture2D("textures/cobblestonesDiffuse.bmp");
                    renderer.material.albedo = Vector4.One;
                    renderer.material.normalMap = Factory.GetTexture2D("textures/cobblestonesNormal.bmp");
                    renderer.material.depthMap = Factory.GetTexture2D("textures/cobblestonesDepth.bmp");
                }*/

                {
                    var entity = scene.AddEntity();
                    var renderer = entity.AddComponent<MeshRenderer>();
                    renderer.Mesh = Factory.GetMesh("dragon.obj");
                    entity.Transform.Position += new Vector3(-30,0,0);
                    entity.GetComponent<MeshRenderer>().Material.smoothness = 0.8f;
                    entity.AddComponent<Rigidbody>();
                    entity.AddComponent<BoxCollider>();

                }


                {
                    var entity = scene.AddEntity();
                    entity.Transform.Position = new Vector3(-10, 10, -10);
                    entity.Transform.LookAt(Vector3.Zero);
                    var light = entity.AddComponent<Light>();
                    light.type = LightType.Directional;
                    light.color = Vector3.One * 0.7f;
                    light.shadows = LightShadows.Soft;

                    var renderer = entity.AddComponent<MeshRenderer>();
                    renderer.Mesh = Factory.GetMesh("internal/cube.obj");
                    entity.AddComponent<BoxCollider>();
                    entity.AddComponent<MoveWithArrowKeys>();
                    entity.AddComponent<VisualizeDir>();
                    //go.AddComponent<LightMovement>();

                }

                {
                    var entity = scene.AddEntity();
                    entity.Transform.Position = new Vector3(10, 10, 10);
                    entity.Transform.LookAt(Vector3.Zero);
                    var light = entity.AddComponent<Light>();
                    light.type = LightType.Directional;
                    light.color = Vector3.One * 0.7f;
                    light.shadows = LightShadows.Soft;

                    var renderer = entity.AddComponent<MeshRenderer>();
                    renderer.Mesh = Factory.GetMesh("internal/cube.obj");
                    entity.AddComponent<VisualizeDir>();
                    //go.AddComponent<LightMovement>();

                }

                {
                    var entity = scene.AddEntity();
                    entity.Transform.Position = new Vector3(0, 10, 0);
                    entity.Transform.LookAt(Vector3.Zero);
                    var light = entity.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = Vector3.One * 0.3f;
                    light.shadows = LightShadows.Soft;

                    var renderer = entity.AddComponent<MeshRenderer>();
                    renderer.Mesh = Factory.GetMesh("internal/cube.obj");
                    entity.AddComponent<VisualizeDir>();

                    scene.Add(entity);
                }

                engine.Run();

            }
        }
    }   
}
