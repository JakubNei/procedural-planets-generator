using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

using MyEngine;
using MyEngine.Events;
using MyEngine.Components;

namespace MyEngine.Components
{
	public class SSAO : PostProcessEffect
	{
		public override void OnAddedToEntity(Entity entity)
		{
			base.OnAddedToEntity(entity);

			//var shader = Factory.GetShader("postProcessEffects/SSAO.shader");

			//shader.Uniform.Set("testColor", new Vector3(0, 1, 0));

			//Camera.main.AddPostProcessEffect(shader);
			Entity.EventSystem.On<EventThreadUpdate>(e => Update(e.DeltaTime));
		}

		void Update(double deltaTime)
		{

		}
	}
}
