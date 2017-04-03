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
		public List<PlanetaryBody.Planet> planets = new List<PlanetaryBody.Planet>();
		SceneSystem scene;
		Factory Factory => scene.Factory;
		MyDebug Debug => scene.Debug;

		Camera Cam { get { return scene.MainCamera; } }

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
					}
				});
				t.Name = "Planet logic";
				t.Priority = ThreadPriority.Lowest;
				t.IsBackground = true;
				t.Start();
			}

			scene.EventSystem.On<FrameEnded>(GPUThreadUpdate);
		}



		PlanetaryBody.Planet planet;

		public PlanetaryBody.Planet AddPlanet()
		{
			var planet = scene.AddEntity("procedural planet #" + planets.Count + 1).AddComponent<PlanetaryBody.Planet>();
			planets.Add(planet);
			return planet;
		}

		void Initialize()
		{

			/*{
				// procedural stars or space dust
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
			cfg.chunkNumberOfVerticesOnEdge = Debug.GetCVar("segment number of vertices on edge", 50);
			cfg.sizeOnScreenNeededToSubdivide = Debug.GetCVar("segment subdivide if size on screen is bigger than", 0.3f);
			cfg.stopSegmentRecursionAtWorldSize = Debug.GetCVar("segment stop recursion at world size", 100);

			cfg.radiusMin = 1000000;
			cfg.baseHeightMap = Factory.GetTexture2D("textures/earth_elevation_map.*");
			cfg.baseHeightMapMultiplier = 20000; //20 km
			cfg.noiseMultiplier = 500;
			cfg.AddControlSplatMap(0, Factory.GetTexture2D("textures/biomes_splat_map_channels0.*"));
			cfg.AddControlSplatMap(1, Factory.GetTexture2D("textures/biomes_splat_map_channels1.*"));
			cfg.AddBiome(0, Factory.GetTexture2D("biomes/snow_d.*"), Factory.DefaultNormalMap); // white, 1,1,1
			cfg.AddBiome(6, Factory.GetTexture2D("biomes/sand2_d.*"), Factory.DefaultNormalMap); // yellow, 1,1,0
			cfg.AddBiome(4, Factory.GetTexture2D("biomes/sand_d.*"), Factory.GetTexture2D("biomes/sand_n.*")); // sand,  1,1,0.5
			cfg.AddBiome(3, Factory.GetTexture2D("biomes/tundra_d.*"), Factory.GetTexture2D("biomes/tundra_n.*")); // grey,  0.5,0.5,0.5
			cfg.AddBiome(5, Factory.GetTexture2D("biomes/forest_d.*"), Factory.GetTexture2D("biomes/forest_n.*")); // green, 0,0.5,0
			cfg.AddBiome(2, Factory.GetTexture2D("biomes/tundra2_d.*"), Factory.GetTexture2D("biomes/tundra2_n.*")); // green-vomit,  0.25,0.5,0.25
			cfg.AddBiome(1, Factory.GetTexture2D("biomes/water_d.*"), Factory.DefaultNormalMap); // blue, 0,0,1


			var planet = AddPlanet();
			planet.SetConfig(cfg);
			planet.Transform.Position = new WorldPos(planet.RadiusMin * 3, 0, 0);

			var planetShader = Factory.GetShader("shaders/planet.shader");
			var planetMaterial = new Material();
			planetMaterial.GBufferShader = planetShader;
			planetMaterial.Uniforms.Set("param_perlinNoise", Factory.GetTexture2D("textures/perlin_noise.*"));
			planetMaterial.Uniforms.Set("param_baseHeightMap", cfg.baseHeightMap);
			planet.PlanetMaterial = planetMaterial;

			planet.Initialize();
		}

		public PlanetaryBody.Planet GetClosestPlanet(WorldPos pos)
		{
			return planets.OrderBy(p => p.Transform.Position.DistanceSqr(pos) - p.RadiusMin * p.RadiusMin).FirstOrDefault();
		}


		bool FreezeUpdate => scene.Debug.GetCVar("planet segments pause subdivision and visibility updates");
		void PlanetLogicUpdate()
		{
			if (FreezeUpdate) return;

			if (runPlanetLogicInOwnThread) canRunNextLogicUpdate.Wait();

			Debug.Tick("generation / planet logic");

			var camPos = Cam.Transform.Position;

			foreach (var p in planets)
			{
				p.TrySubdivideOver(camPos);
			}

			if (runPlanetLogicInOwnThread) canRunNextLogicUpdate.Reset();
		}


		ManualResetEventSlim canRunNextLogicUpdate = new ManualResetEventSlim();


		void GPUThreadUpdate(FrameTimeEvent r)
		{
			if (!runPlanetLogicInOwnThread) PlanetLogicUpdate();

			foreach (var p in planets)
			{
				p.GPUThreadTick(r.FrameTime);
			}

			if (runPlanetLogicInOwnThread) canRunNextLogicUpdate.Set();
		}
	}
}