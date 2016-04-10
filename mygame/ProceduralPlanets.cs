using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using MyEngine;
using MyEngine.Components;

namespace MyGame
{
    public class ProceduralPlanets
    {

        List<PlanetaryBody> planets = new List<PlanetaryBody>();
        SceneSystem scene;

        Camera cam { get { return scene.mainCamera; } }

        public ProceduralPlanets(SceneSystem scene)
        {
            this.scene = scene;
            Start();
        }

        void Start()
        {


            /*
            planetShader = Shader::Get("shaders/planet.shader");
            var planetMaterial = new Material(planetShader);
            planetMaterial.SetTexture("grass", Texture2D::Get("textures/grass.jpg"));
            planetMaterial.SetTexture("rock", Texture2D::Get("textures/rock.jpg"));
            planetMaterial.SetTexture("snow", Texture2D::Get("textures/snow.jpg"));
            planetMaterial.SetTexture("perlinNoise", Texture2D::Get("textures/perlin_noise.png"));
            */


            PlanetaryBody planet;


            planet = scene.AddEntity().AddComponent<PlanetaryBody>();
            planet.radius = 150;
            planet.radiusVariation = 7;
            planet.Transform.Position = new Vector3(-2500, 200, 0);
            //planet.planetMaterial = planetMaterial;
            planet.Start();
            planets.Add(planet);
            //terrainMaterial.SetTexture("heightMap",Texture2D::Get("textures/perlin_noise.png"));

            planet = scene.AddEntity().AddComponent<PlanetaryBody>();
            planet.radius = 100;
            planet.radiusVariation = 5;
            planet.Transform.Position = new Vector3(-2000, 50, 0);
            planet.Start();
            //planet.planetMaterial = planetMaterial;
            planets.Add(planet);

            planet = scene.AddEntity().AddComponent<PlanetaryBody>();
            planet.radius = 2000; // 6371000 earth radius
            planet.radiusVariation = 100;
            planet.chunkNumberOfVerticesOnEdge = 10; // 20
            planet.subdivisionRecurisonDepth = 10;
            planet.subdivisionSphereRadiusModifier = 0.5f;
            planet.Transform.Position = new Vector3(1000, -100, 1000);
            planet.Start();
            //planet.planetMaterial = planetMaterial;
            planets.Add(planet);

            cam.Transform.Position = new Vector3(-planet.radius, 0, 0) + planet.Transform.Position;

            scene.EventSystem.Register((MyEngine.Events.GraphicsUpdate e) => OnGraphicsUpdate());
        }




        void OnGraphicsUpdate()
        {
            var camPos = cam.Transform.Position;

            var planet = planets.OrderBy(p => p.Transform.Position.Distance(camPos) - p.radius).First();

            // make cam on top of the planet
            {
                var p = cam.Transform.Position - planet.Transform.Position;
                var camPosS = planet.CalestialToSpherical(p);
                var h = 1 + planet.GetHeight(p);
                if (camPosS.altitude < h)
                {
                    camPosS.altitude = h;
                    cam.Transform.Position = planet.SphericalToCalestial(camPosS) + planet.Transform.Position;
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