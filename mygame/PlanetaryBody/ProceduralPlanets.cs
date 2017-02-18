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

        public ProceduralPlanets(SceneSystem scene)
        {
            this.scene = scene;
            Start();

            var t = new Thread(() =>
            {
                while (true)
                {
                    PlanetLogicUpdate();
                    Thread.Sleep(10);
                }
            });
            t.Priority = ThreadPriority.Highest;
            t.IsBackground = true;
            t.Start();

            scene.EventSystem.Register<RenderUpdate>(OnRender);

        }

 

        PlanetaryBody.Root planet;


        void OnRender(RenderUpdate r)
        {
            foreach (var p in planets)
                p.OnRender(r);
        }










        void Start()
        {
            Material planetMaterial = null;

            var planetShader = Factory.GetShader("shaders/planet.shader");
            planetMaterial = new Material(Factory);
            planetMaterial.GBufferShader = planetShader;
            planetMaterial.Uniforms.Set("param_rock", Factory.GetTexture2D("textures/rock.jpg"));
            planetMaterial.Uniforms.Set("param_snow", Factory.GetTexture2D("textures/snow.jpg"));
            planetMaterial.Uniforms.Set("param_biomesSplatMap", Factory.GetTexture2D("textures/biomesSplatMap.png"));
            planetMaterial.Uniforms.Set("param_perlinNoise", Factory.GetTexture2D("textures/perlin_noise.png"));

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

            planet = scene.AddEntity().AddComponent<PlanetaryBody.Root>();
            planets.Add(planet);
            // 6371000 earth radius
            planet.Configure(10000, 500);
            planet.Transform.Position = new WorldPos(planet.RadiusMax * 3, 0, 0);
            planet.Start();
            planet.planetMaterial = planetMaterial;
            planet.planetMaterial.Uniforms.Set("param_planetRadius", (float)planet.RadiusMax);
            planets.Add(planet);

            Cam.Transform.LookAt(planet.Transform.Position);
            if (moveCameraToSurfaceOnStart)
            {
                Cam.Transform.Position = new WorldPos((float)-planet.RadiusMax, 0, 0) + planet.Transform.Position;
            }
        }

        bool freezeUpdate = false;

        void PlanetLogicUpdate()
        {
            if (scene.Input.GetKeyDown(Key.P)) freezeUpdate = !freezeUpdate;
            if (freezeUpdate) return;

            Debug.Tick("generation / planet logic");

            var camPos = Cam.Transform.Position;

            var closestPlanet = planets.OrderBy(p => p.Transform.Position.DistanceSqr(camPos) - p.RadiusMax * p.RadiusMax).First();

            foreach (var p in planets)
            {
                p.TrySubdivideOver(camPos);
            }
        }
    }
}