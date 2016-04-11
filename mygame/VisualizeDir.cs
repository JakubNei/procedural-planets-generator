using System;
using System.Collections.Generic;
using System.Text;

using MyEngine;
using MyEngine.Events;
using MyEngine.Components;

using OpenTK;

namespace MyGame
{
    public class VisualizeDir : ComponentWithShortcuts
    {

        public Vector3 offset = new Vector3(0, 10, 0);

        Entity dirVisualize;

        public VisualizeDir(Entity entity) : base(entity)
        {
            Start();
            Entity.EventSystem.Register<GraphicsUpdate>(e => Update(e.DeltaTime));
        }

        void Start()
        {
            var go = Entity.Scene.AddEntity();
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.Mesh = Factory.GetMesh("sphere.obj");
            renderer.Material.albedo = new Vector4(0, 0, 1, 1);
            go.Transform.Scale *= 0.5f;

            dirVisualize = go;
        }

        void Update(double deltaTime)
        {
            dirVisualize.Transform.Position = this.Entity.Transform.Position + this.Entity.Transform.Forward*2;
        }
    }
}
