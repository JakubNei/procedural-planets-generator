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
    public class RenderData
    {
        public SparseList<IRenderable> Renderers
        {
            get
            {
                return geometryRenderers;
            }
        }
        public SparseList<IRenderable> ShadowCasters
        {
            get
            {
                return shadowCasters;
            }
        }

        public SparseList<ILight> Lights
        {
            get
            {
                return lights;
            }
        }

        SparseList<IRenderable> geometryRenderers = new SparseList<IRenderable>(1000);
        SparseList<IRenderable> shadowCasters = new SparseList<IRenderable>(1000);
        SparseList<ILight> lights = new SparseList<ILight>(1000);

        public void AddGeometry(IRenderable renderer)
        {
            lock (geometryRenderers)
            {
                geometryRenderers.Add(renderer);
            }
        }
        public void RemoveGeometry(IRenderable renderer)
        {
            lock (geometryRenderers)
            {
                geometryRenderers.Remove(renderer);
            }
        }
        public void AddShadowCaster(IRenderable renderer)
        {
            lock (shadowCasters)
            {
                shadowCasters.Add(renderer);
            }
        }
        public void RemoveShadowCaster(IRenderable renderer)
        {
            lock (shadowCasters)
            {
                shadowCasters.Remove(renderer);
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
