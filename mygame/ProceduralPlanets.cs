using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Input;

using MyEngine;
using MyEngine.Components;

namespace MyGame
{
    public class ProceduralPlanets
    {

        public List<PlanetaryBody> planets = new List<PlanetaryBody>();
        SceneSystem scene;

        bool clampCameraToSurface = false;
        bool moveCameraToSurfaceOnStart = false;

        Camera cam { get { return scene.mainCamera; } }

        public ProceduralPlanets(SceneSystem scene)
        {
            this.scene = scene;
            Start();
            scene.EventSystem.Register((MyEngine.Events.GraphicsUpdate e) => OnGraphicsUpdate(e.DeltaTime));
        }

        void Start()
        {

            Material planetMaterial = null;


            var planetShader = Factory.GetShader("shaders/planet.shader");
            planetMaterial = new Material();
            planetMaterial.GBufferShader = planetShader;
            planetMaterial.Uniforms.Set("param_grass", Factory.GetTexture2D("textures/grass.jpg"));
            planetMaterial.Uniforms.Set("param_rock", Factory.GetTexture2D("textures/rock.jpg"));
            planetMaterial.Uniforms.Set("param_snow", Factory.GetTexture2D("textures/snow.jpg"));
            planetMaterial.Uniforms.Set("param_perlinNoise", Factory.GetTexture2D("textures/perlin_noise.png"));
            





            PlanetaryBody planet;

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

            planet = scene.AddEntity().AddComponent<PlanetaryBody>();
            planets.Add(planet);
            //planet.radius = 2000; // 6371000 earth radius
            //planet.radiusVariation = 100;
            planet.radius = 100; // 6371000 earth radius
            planet.radiusVariation = 5;
            planet.chunkNumberOfVerticesOnEdge = 20; // 20
            planet.subdivisionMaxRecurisonDepth = 11;
            planet.startingRadiusSubdivisionModifier = 2f;
            planet.subdivisionSphereRadiusModifier = 0.5f;
            planet.Transform.Position = new Vector3(1000, -100, 1000);
            planet.Start();
            planet.planetMaterial = planetMaterial;
            planets.Add(planet);

            if (moveCameraToSurfaceOnStart)
            {
                cam.Transform.Position = new Vector3((float)-planet.radius, 0, 0) + planet.Transform.Position;
            }

        }

        bool freezeUpdate = false;
        void OnGraphicsUpdate(double dt)
        {
            if (scene.Input.GetKeyDown(Key.P)) freezeUpdate = !freezeUpdate;
            if (freezeUpdate) return;

            var camPos = cam.Transform.Position;

            var planet = planets.OrderBy(p => p.Transform.Position.Distance(camPos) - p.radius).First();

            // make cam on top of the planet
            if(clampCameraToSurface) {
                var p = (cam.Transform.Position - planet.Transform.Position).ToVector3d();
                var camPosS = planet.CalestialToSpherical(p);
                var h = 1 + planet.GetHeight(p);
                if (camPosS.altitude < h)
                {
                    camPosS.altitude = h;
                    cam.Transform.Position = planet.SphericalToCalestial(camPosS).ToVector3() + planet.Transform.Position;
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