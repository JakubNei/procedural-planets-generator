using MyEngine;
using MyEngine.Components;
using MyEngine.Events;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyGame
{
	public class VisualizePosition : ComponentWithShortcuts
	{
		Entity target;
		Vector3 targetsLocalPosition;

		public VisualizePosition(Entity entity) : base(entity)
		{
			Entity.EventSystem.Register<EventThreadUpdate>(e => Update(e.DeltaTimeNow));
		}

		/*
        public static void Create(Entity entity, Vector3 targetsLocalPosition)
        {
            var go = entity.Scene.AddEntity();
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.Mesh = Factory.GetMesh("sphere.obj");
            go.Transform.Scale *= 0.5f;

            var vp=go.AddComponent<VisualizePosition>();
            vp.target = entity;
            vp.targetsLocalPosition = targetsLocalPosition;
        }
		*/

		void Update(double deltaTime)
		{
			Entity.Transform.Position = target.Transform.Position + targetsLocalPosition.RotateBy(target.Transform.Rotation);
		}
	}
}