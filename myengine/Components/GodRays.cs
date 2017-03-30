using Neitri;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine.Components
{
	public class GodRays : PostProcessEffect
	{
		public Vector3 lightScreenPos;
		public Vector3 lightWorldPos;
		public float lightWorldRadius;

		public override void OnAddedToEntity(Entity entity)
		{
			base.OnAddedToEntity(entity);

			Shader = Factory.GetShader("postProcessEffects/godRays.glsl");
		}

		public override void BeforeBindCallBack()
		{
			Shader.Uniforms.Set("param_lightScreenPos", lightScreenPos);
			Shader.Uniforms.Set("param_lightWorldPos", lightWorldPos);
			Shader.Uniforms.Set("param_lightWorldRadius", lightWorldRadius);
		}
	}
}