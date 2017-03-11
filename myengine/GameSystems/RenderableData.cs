using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyEngine;
using MyEngine.Components;
using MyEngine.Events;

namespace MyEngine
{
    public class RenderableData
    {
        public IList<IRenderable> Renderers =>  renderers;
    
        public IList<ILight> Lights => lights;

		IList<IRenderable> renderers = new UnorderedList<IRenderable>(1000);
		IList<ILight> lights = new UnorderedList<ILight>(1000);

        public void Add(IRenderable renderer)
        {
            lock (renderers)
            {
                renderers.Add(renderer);
            }
        }
        public void Remove(IRenderable renderer)
        {
            lock (renderers)
            {
                renderers.Remove(renderer);
            }
        }
        public void Add(ILight light)
        {
            lock (lights)
            {
                lights.Add(light);
            }
        }
        public void Remove(ILight light)
        {
            lock (lights)
            {
                lights.Remove(light);
            }
        }


    }
}
