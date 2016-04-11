using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using MyEngine;
using MyEngine.Events;
using MyEngine.Components;

namespace MyGame
{
    public class DebugShowFrustumPlanes : ComponentWithShortcuts
    {
        List<Entity> gos = new List<Entity>();

        public DebugShowFrustumPlanes(Entity entity) : base(entity)
        {
            for (int i = 0; i < 6; i++)
            {
                var e = Entity.Scene.AddEntity();
                gos.Add(e);
                var r = e.AddComponent<MeshRenderer>();
                r.Mesh = Factory.GetMesh("internal/cube.obj");
                e.Transform.Scale = new Vector3(10, 10, 1);
            }

            Entity.EventSystem.Register<GraphicsUpdate>(e => Update(e.DeltaTime));
        }
        void Update(double deltaTime)
        {
            var p = Entity.GetComponent<Camera>().GetFrustumPlanes();

            for (int i = 0; i < 6; i++)
            {
                // is broken maybe, furstum culling works but this doesnt make much sense
                //gos[i].transform.position = p[i].normal * p[i].distance;
                gos[i].Transform.Rotation = QuaternionUtility.LookRotation(p[i].normal);
            }
        }
    }
}

