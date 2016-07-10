using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Input;

using MyEngine;
using MyEngine.Components;

namespace MyGame
{
    public class ProceduralPlanets
    {

        public List<PlanetaryBody.Root> planets = new List<PlanetaryBody.Root>();
        SceneSystem scene;

        bool clampCameraToSurface = true;
        bool moveCameraToSurfaceOnStart = true;

        Camera cam { get { return scene.mainCamera; } }

        public ProceduralPlanets(SceneSystem scene)
        {
            this.scene = scene;
            Start();

            var t = new Thread(() =>
            {
                while (scene.Engine.IsExiting == false)
                {
                    Debug.Tick("planets");
                    OnGraphicsUpdate();
                }
            });
            t.Priority = ThreadPriority.Highest;
            t.IsBackground = true;
            t.Start();
        }

        void Start()
        {

            Material planetMaterial = null;


            var planetShader = Factory.GetShader("shaders/planet.shader");
            planetMaterial = new Material();
            planetMaterial.GBufferShader = planetShader;
            planetMaterial.Uniforms.Set("param_moon", Factory.GetTexture2D("textures/moon1.jpg"));
            planetMaterial.Uniforms.Set("param_rock", Factory.GetTexture2D("textures/rock.jpg"));
            planetMaterial.Uniforms.Set("param_snow", Factory.GetTexture2D("textures/snow.jpg"));
            planetMaterial.Uniforms.Set("param_perlinNoise", Factory.GetTexture2D("textures/perlin_noise.png"));



            
            {
                var random = new Random();
                for (int i = 0; i < 1000; i++)
                {
                    var e = scene.AddEntity("start dust #" + i);
                    var vec = new Vector3d(random.Next(-1.0, 1.0), random.Next(-1.0, 1.0), random.Next(-1.0, 1.0));
                    e.Transform.Position = new WorldPos(vec.Normalized() * (2000.0 + random.Next(0, 2000)));
                    e.Transform.Scale *= 1f;
                    var r = e.AddComponent<MeshRenderer>();
                    r.Mesh = Factory.GetMesh("sphere.obj");
                    var m = new MaterialPBR();
                    r.Material = m;
                    m.GBufferShader = Factory.GetShader("internal/deferred.gBuffer.PBR.shader");
                    m.albedo = new Vector4(10);
                }
            }



			PlanetaryBody.Root planet;

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
            planet.radius = 2000; // 6371000 earth radius
            planet.radius = 300; // 6371000 earth radius
            planet.radiusVariation = 100;
            planet.radiusVariation = 15;
            planet.Transform.Position = new WorldPos(1000, -100, 1000);
            planet.Start();
            planet.planetMaterial = planetMaterial;
            planets.Add(planet);

            if (moveCameraToSurfaceOnStart)
            {
                cam.Transform.Position = new WorldPos((float)-planet.radius, 0, 0) + planet.Transform.Position;
            }

        }

        bool freezeUpdate = false;
        void OnGraphicsUpdate()
        {
            if (scene.Input.GetKeyDown(Key.P)) freezeUpdate = !freezeUpdate;
            if (freezeUpdate) return;

            var camPos = cam.Transform.Position;

            var planet = planets.OrderBy(p => p.Transform.Position.Distance(camPos) - p.radius).First();

            // make cam on top of the planet
            if (clampCameraToSurface)
            {
                var p = (cam.Transform.Position - planet.Transform.Position).ToVector3d();
                var camPosS = planet.CalestialToSpherical(p);
                var h = 1 + planet.GetHeight(p);
                if (camPosS.altitude < h)
                {
                    camPosS.altitude = h;
                    cam.Transform.Position = planet.Transform.Position + planet.SphericalToCalestial(camPosS).ToVector3();
                }
            }

            foreach (var p in planets)
            {
                p.TrySubdivideOver(camPos);
            }



            /*var normal = normalize(pos - planet.position);
            var tangent = cross(normal, vec3(0, 1, 0));
            CameraMovement::instance.cameraUpDirection = normal;*/



        }
    }
}