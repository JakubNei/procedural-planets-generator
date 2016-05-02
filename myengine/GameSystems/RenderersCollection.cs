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
        public SparseList<Renderer> Renderers
        {
            get
            {
                return geometryRenderers;
            }
        }
        public SparseList<Renderer> ShadowCasters
        {
            get
            {
                return shadowCasters;
            }
        }

        public SparseList<Light> Lights
        {
            get
            {
                return lights;
            }
        }

        SparseList<Renderer> geometryRenderers = new SparseList<Renderer>(1000);
        SparseList<Renderer> shadowCasters = new SparseList<Renderer>(1000);
        SparseList<Light> lights = new SparseList<Light>(1000);

        public void AddGeometry(Renderer renderer)
        {
            lock (geometryRenderers)
            {
                geometryRenderers.Add(renderer);
            }
        }
        public void RemoveGeometry(Renderer renderer)
        {
            lock (geometryRenderers)
            {
                geometryRenderers.Remove(renderer);
            }
        }
        public void AddShadowCaster(Renderer renderer)
        {
            lock (shadowCasters)
            {
                shadowCasters.Add(renderer);
            }
        }
        public void RemoveShadowCaster(Renderer renderer)
        {
            lock (shadowCasters)
            {
                shadowCasters.Remove(renderer);
            }
        }

        public void Add(Light light)
        {
            lock (lights)
            {
                lights.Add(light);
            }
        }
        public void Remove(Light light)
        {
            lock (lights)
            {
                lights.Remove(light);
            }
        }


    }
}
