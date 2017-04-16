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
using MyGame.PlanetaryBody;
using System.Drawing;

namespace MyGame
{
	public class ProceduralPlanets : SingletonsPropertyAccesor
	{
		public List<PlanetaryBody.Planet> planets = new List<PlanetaryBody.Planet>();
		readonly SceneSystem scene;

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

			scene.EventSystem.On<RenderPrepareEnded>(e =>
			{				
				if (runPlanetLogicInOwnThread) canRunNextLogicUpdate.Set();
				else PlanetLogicUpdate();
			});

			scene.EventSystem.On<FrameEnded>(GPUThreadUpdate);
		}


		public PlanetaryBody.Planet AddPlanet()
		{
			var planet = scene.AddEntity("procedural planet #" + planets.Count + 1).AddComponent<PlanetaryBody.Planet>();
			planets.Add(planet);
			return planet;
		}



		void Initialize()
		{

			var biomesAtlas = new BiomesAtlas();

			biomesAtlas.AddBiome(Color.FromArgb(255, 255, 255), Factory.GetTexture2D("planet/biomes/snow_d.*"), Factory.DefaultNormalMap); // white, 1,1,1
			biomesAtlas.AddBiome(Color.FromArgb(128, 128, 128), Factory.GetTexture2D("planet/biomes/tundra_d.*"), Factory.GetTexture2D("planet/biomes/tundra_n.*")); // grey,  0.5,0.5,0.5
			biomesAtlas.AddBiome(Color.FromArgb(64, 128, 64), Factory.GetTexture2D("planet/biomes/tundra2_d.*"), Factory.GetTexture2D("planet/biomes/tundra2_n.*")); // green-vomit,  0.25,0.5,0.25
			biomesAtlas.AddBiome(Color.FromArgb(255, 255, 128), Factory.GetTexture2D("planet/biomes/sand_d.*"), Factory.GetTexture2D("planet/biomes/sand_n.*")); // sand,  1,1,0.5
			biomesAtlas.AddBiome(Color.FromArgb(255, 255, 0), Factory.GetTexture2D("planet/biomes/sand2_d.*"), Factory.DefaultNormalMap); // yellow, 1,1,0
			biomesAtlas.AddBiome(Color.FromArgb(0, 128, 0), Factory.GetTexture2D("planet/biomes/forest_d.*"), Factory.GetTexture2D("planet/biomes/forest_n.*")); // green, 0,0.5,0
			biomesAtlas.AddBiome(Color.FromArgb(0, 0, 255), Factory.GetTexture2D("planet/biomes/water_d.*"), Factory.DefaultNormalMap); // blue, 0,0,1

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


			{
				var cfg = new PlanetaryBody.Config();
				cfg.chunkNumberOfVerticesOnEdge = Debug.GetCVar("generation / segment number of vertices on edge", 50);
				cfg.weightNeededToSubdivide = Debug.GetCVar("generation / segment subdivide if weight is bigger than", 0.2f);
				cfg.stopSegmentRecursionAtWorldSize = Debug.GetCVar("generation / segment stop recursion at world size", 100);

				// 6371000m is earth radius
				cfg.radiusMin = 1000000;
				cfg.baseHeightMap = Factory.GetTexture2D("planet/data/earth/height_map.*");
				cfg.baseHeightMapMultiplier = 20000; //20 km
				cfg.noiseMultiplier = 200;
				cfg.AddControlSplatMap(0, Factory.GetTexture2D("planet/data/earth/biomes_splat_map_0.*"));
				cfg.AddControlSplatMap(1, Factory.GetTexture2D("planet/data/earth/biomes_splat_map_1.*"));
				cfg.LoadConfig(FileSystem.FindFile("planet/data/earth/biomes_splat_maps_metadata.xml"), biomesAtlas);

				var planetShader = Factory.GetShader("shaders/planet.shader");
				var planetMaterial = new Material();
				planetMaterial.GBufferShader = planetShader;

				var planet = AddPlanet();
				planet.Transform.Position = new WorldPos(cfg.radiusMin * 3, 0, 0);
				planet.PlanetMaterial = planetMaterial;
				planet.Initialize(cfg);
			}



			{
				var cfg = new PlanetaryBody.Config();
				cfg.chunkNumberOfVerticesOnEdge = Debug.GetCVar("generation / segment number of vertices on edge", 50);
				cfg.weightNeededToSubdivide = Debug.GetCVar("generation / segment subdivide if weight is bigger than", 0.2f);
				cfg.stopSegmentRecursionAtWorldSize = Debug.GetCVar("generation / segment stop recursion at world size", 100);

				cfg.radiusMin = 100000;
				cfg.baseHeightMap = Factory.GetTexture2D("planet/data/myplanet1/height_map.*");
				cfg.baseHeightMapMultiplier = 2000;
				cfg.noiseMultiplier = 200;
				cfg.AddControlSplatMap(0, Factory.GetTexture2D("planet/data/myplanet1/biomes_splat_map_0.*"));
				cfg.AddControlSplatMap(1, Factory.GetTexture2D("planet/data/myplanet1/biomes_splat_map_1.*"));
				cfg.LoadConfig(FileSystem.FindFile("planet/data/myplanet1/biomes_splat_maps_metadata.xml"), biomesAtlas);

				var planetShader = Factory.GetShader("shaders/planet.shader");
				var planetMaterial = new Material();
				planetMaterial.GBufferShader = planetShader;

				var planet = AddPlanet();
				planet.Transform.Position = new WorldPos(1000000 * 5, 0, 0);
				planet.PlanetMaterial = planetMaterial;
				planet.Initialize(cfg);
			}

		}

		public PlanetaryBody.Planet GetClosestPlanet(WorldPos pos)
		{
			return planets.OrderBy(p => p.Transform.Position.DistanceSqr(pos) - p.RadiusMin * p.RadiusMin).FirstOrDefault();
		}


		bool FreezeUpdate => scene.Debug.GetCVar("generation / planet segments pause subdivision and visibility updates");
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
			foreach (var p in planets)
			{
				p.GPUThreadTick(r.FrameTime);
			}
		}
	}
}