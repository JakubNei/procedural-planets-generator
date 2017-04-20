using MyEngine;
using MyEngine.Components;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

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
				var scene = engine.NewScene();
				var factory = engine.Factory;

				new DebugKeys(scene, engine.Debug);

				var proceduralPlanets = new ProceduralPlanets(scene);

				{
					var entity = scene.AddEntity();
					fpc = entity.AddComponent<FirstPersonCamera>();
					fpc.planets = proceduralPlanets;
					//var flashLight = entity.AddComponent<Light>();
					//flashLight.LighType = LightType.Point;				


					var cam = scene.MainCamera = entity.AddComponent<Camera>();
					cam.NearClipPlane = 0.5f;
					cam.FarClipPlane = 5000000;

					// post process effects
					{
						entity.AddComponent<Bloom>();

						entity.AddComponent<Tonemapping>();

						var gr = entity.AddComponent<GodRays>();
						gr.lightWorldRadius = 1000;
						entity.EventSystem.On<MyEngine.Events.PreRenderUpdate>((e) =>
						{
							gr.lightScreenPos = cam.WorldToScreenPos(sunEntity.Transform.Position);
							gr.lightWorldPos = cam.Transform.Position.Towards(sunEntity.Transform.Position).ToVector3();
						});
					}

					string skyboxName = "skybox/generated/";
					scene.skyBox = factory.GetCubeMap(new[] {
						skyboxName + "left.png",
						skyboxName + "right.png",
						skyboxName + "top.png",
						skyboxName + "bottom.png",
						skyboxName + "front.png",
						skyboxName + "back.png"
					});

					entity.Transform.Position = new WorldPos(100, 100, 100);
					entity.Transform.LookAt(new WorldPos(0, 0, 100));

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

				// SUN
				{
					var entity = sunEntity = scene.AddEntity("sun");
					entity.Transform.Scale *= 1000;

					var renderer = entity.AddComponent<MeshRenderer>();
					renderer.Mesh = factory.GetMesh("sphere_smooth.obj");
					renderer.RenderingMode = MyRenderingMode.RenderGeometryAndCastShadows;

					var mat = renderer.Material = factory.NewMaterial();
					mat.RenderShader = factory.GetShader("shaders/sun.glsl");
					mat.Uniforms.Set("param_turbulenceColorGradient", factory.GetTexture2D("textures/fire_gradient.png"));
					mat.Uniforms.Set("param_turbulenceMap", factory.GetTexture2D("textures/turbulence_map.png"));
					mat.Uniforms.Set("param_surfaceDiffuse", factory.GetTexture2D("textures/sun_surface_d.png"));
					mat.Uniforms.Set("param_perlinNoise", factory.GetTexture2D("textures/perlin_noise.png"));
				}

				{
					var entity = scene.AddEntity("scene directional light");
					var light = entity.AddComponent<Light>();
					light.LighType = LightType.Directional;
					light.Color = Vector3.One * 1f;
					light.Shadows = LightShadows.None;

					// make directional sun light look at the closest planet
					//scene.EventSystem.Register((MyEngine.Events.EventThreadUpdate e) =>
					//{
					//	entity.Transform.Position = sunEntity.Transform.Position;
					//	var p = proceduralPlanets.planets.FirstOrDefault();
					//	if (p != null)
					//	{
					//		entity.Transform.LookAt(p.Transform.Position);
					//	}
					//});
				}


				engine.Run();
			}
		}
	}
}