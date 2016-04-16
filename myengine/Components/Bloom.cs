using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace MyEngine.Components
{
    public class Bloom : PostProcessEffect
    {
        public Bloom(Entity entity) : base(entity)
        {
            Shader = Factory.GetShader("postProcessEffects/bloom.glsl");
        }
    }
}
