using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using MyEngine;
using MyEngine.Components;
using MyEngine.Events;

namespace MyEngine
{
    public class SceneSystem : GameSystemBase
    {
        public SparseList<Entity> Entities
        {
            get
            {
                return entities;
            }
        }

        public SparseList<Light> Lights
        {
            get
            {
                return lights;
            }
        }
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



        //public ISynchronizeInvoke SynchronizeInvoke { get; private set; }
        //DeferredSynchronizeInvoke.Owner deferredSynchronizeInvokeOwner;

        public EventSystem EventSystem { get; private set; }
        public EngineMain Engine { get; private set; }
        public InputSystem Input
        {
            get
            {
                return Engine.Input;
            }
        }

        public Camera mainCamera;



        SparseList<Entity> entities = new SparseList<Entity>(1000);
        SparseList<Light> lights = new SparseList<Light>(1000);
        SparseList<Renderer> geometryRenderers = new SparseList<Renderer>(1000);
        SparseList<Renderer> shadowCasters = new SparseList<Renderer>(1000);

        public SceneSystem(EngineMain engine)
        {
            this.Engine = engine;
            this.EventSystem = new EventSystem();
            this.EventSystem.OnAnyEventCalled += (IEvent evt) =>
            {
                foreach(var e in entities)
                {
                    e.EventSystem.Raise(evt);
                }
            };

            //deferredSynchronizeInvokeOwner = new DeferredSynchronizeInvoke.Owner();
            /*
            SynchronizeInvoke = new DeferredSynchronizeInvoke(deferredSynchronizeInvokeOwner);
            EventSystem.Register((Events.GraphicsUpdate evt) =>
            {
                deferredSynchronizeInvokeOwner.ProcessQueue();
            });
            */
        }

        public Entity AddEntity(string name = "unnamed entity")
        {
            var e = new Entity(this, name);
            entities.Add(e);
            return e;
        }

        public void Add(Entity entity)
        {
            lock(entities)
            {
                entities.Add(entity);
            }
        }
        public void Remove(Entity entity)
        {
            lock(entities)
            {
                entities.Remove(entity);
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
        public void AddGeometry(Renderer renderer)
        {
            lock(geometryRenderers)
            {
                geometryRenderers.Add(renderer);
            }
        }
        public void RemoveGeometry(Renderer renderer)
        {
            lock(geometryRenderers)
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


        public class RenderSettings
        {

        }


        public void Render(RenderSettings settings)
        {

        }
    }
}
