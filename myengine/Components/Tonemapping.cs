using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine.Components
{
	public class Tonemapping : PostProcessEffect
	{
		public Tonemapping(Entity entity) : base(entity)
		{
			Shader = Factory.GetShader("postProcessEffects/tonemapping.glsl");
		}
	}
}