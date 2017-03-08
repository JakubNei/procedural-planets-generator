using MyEngine;
using MyEngine.Components;
using MyEngine.Events;
using Neitri;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyGame
{
	public class ProceduralPlanets
	{
		public List<PlanetaryBody.Root> planets = new List<PlanetaryBody.Root>();
		SceneSystem scene;
		Factory Factory => scene.Factory;
		Debug Debug => scene.Debug;

		bool moveCameraToSurfaceOnStart = false;

		Camera Cam { get { return scene.mainCamera; } }

		public bool runPlanetLogicInOwnThread = true;

		public ProceduralPlanets(SceneSystem scene)
		{
			this.scene = scene;

			Initialize();

			if (runPlanetLogicInOwnThread)
			{
				var t = new Thread(() =>
				{
					while (true)
					{
						PlanetLogicUpdate();
						Thread.Sleep(10);
					}
				});
				t.Name = "Planet logic";
				t.Priority = ThreadPriority.Highest;
				t.IsBackground = true;
				t.Start();
			}

			scene.EventSystem.Register<RenderUpdate>((r) => GPUThreadUpdate());

			scene.Debug.CVar("generation / planet logic update pause").ToogledByKey(Key.P).OnChanged += (v) => freezeUpdate = v.Bool;
		}



		PlanetaryBody.Root planet;

		public PlanetaryBody.Root AddPlanet()
		{
			var planet = scene.AddEntity().AddComponent<PlanetaryBody.Root>();
			planets.Add(planet);
			return planet;
		}

		void Initialize()
		{

			/*{
				// procedural stars or star dust
				var random = new Random();
				for (int i = 0; i < 1000; i++)
				{
					var e = scene.AddEntity("start dust #" + i);
					var vec = new Vector3d(random.Next(-1.0, 1.0), random.Next(-1.0, 1.0), random.Next(-1.0, 1.0));
					e.Transform.Position = new WorldPos(vec.Normalized() * (600000.0 + random.Next(0, 200000)));
					e.Transform.Scale *= random.Next(0.5f, 1f) * 1000f;
					var r = e.AddComponent<MeshRenderer>();
					r.Mesh = Factory.GetMesh("sphere.obj");
					var m = new MaterialPBR(Factory);
					r.Material = m;
					m.GBufferShader = Factory.GetShader("internal/deferred.gBuffer.PBR.shader");
					m.albedo = new Vector4(10);
				}
			}*/

			/*
            planet = scene.AddEntity().AddComponent<PlanetaryBody>();
            planet.radius = 150;
            planet.radiusVariation = 7;
            planet.Transform.Position = new Vector3(-2500, 200, 0);
            planet.planetMaterial = planetMaterial;
            planet.Start();
            planets.Add(planet);

            planet = scene.AddEntity().AddComponent<PlanetaryBody>();
            planet.radius = 100;
            planet.radiusVariation = 5;
            planet.Transform.Position = new Vector3(-2000, 50, 0);
            planet.Start();
            planet.planetMaterial = planetMaterial;
            planets.Add(planet);
            */


			// 6371000 earth radius
			var cfg = new PlanetaryBody.Config();
			cfg.radiusMax = 1000000;
			cfg.radiusVariation = 20000;
			cfg.baseHeightMap = Factory.GetTexture2D("textures/earth_elevation_map.png");
			var planet = AddPlanet();
			planet.SetConfig(cfg);
			planet.Transform.Position = new WorldPos(planet.RadiusMax * 3, 0, 0);

			var planetShader = Factory.GetShader("shaders/planet.shader");
			var planetMaterial = new Material(Factory);
			planetMaterial.GBufferShader = planetShader;
			planetMaterial.Uniforms.Set("param_rock", Factory.GetTexture2D("textures/rock.jpg"));
			planetMaterial.Uniforms.Set("param_snow", Factory.GetTexture2D("textures/snow.jpg"));
			planetMaterial.Uniforms.Set("param_biomesSplatMap", Factory.GetTexture2D("textures/biomesSplatMap.png"));
			planetMaterial.Uniforms.Set("param_perlinNoise", Factory.GetTexture2D("textures/perlin_noise.png"));
			planetMaterial.Uniforms.Set("param_baseHeightMap", cfg.baseHeightMap);
			planet.planetMaterial = planetMaterial;

			planet.Initialize();


			Cam.Transform.LookAt(planet.Transform.Position);
			if (moveCameraToSurfaceOnStart)
			{
				Cam.Transform.Position = new WorldPos((float)-planet.RadiusMax, 0, 0) + planet.Transform.Position;
			}
		}

		bool freezeUpdate = false;
		void PlanetLogicUpdate()
		{
			if (freezeUpdate) return;

			if(runPlanetLogicInOwnThread) canRunNextLogicUpdate.Wait();

			Debug.Tick("generation / planet logic");

			var camPos = Cam.Transform.Position;

			var closestPlanet = planets.OrderBy(p => p.Transform.Position.DistanceSqr(camPos) - p.RadiusMax * p.RadiusMax).First();

			foreach (var p in planets)
			{
				p.TrySubdivideOver(camPos);
			}

			if (runPlanetLogicInOwnThread) canRunNextLogicUpdate.Reset();
		}


		ManualResetEventSlim canRunNextLogicUpdate = new ManualResetEventSlim();


		void GPUThreadUpdate()
		{
			if (!runPlanetLogicInOwnThread) PlanetLogicUpdate();

			foreach (var p in planets)
			{
				p.GPUThreadUpdate();
			}

			if (runPlanetLogicInOwnThread) canRunNextLogicUpdate.Set();
		}
	}
}