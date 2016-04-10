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
    public class SceneSystem : GameSystemBase
    {
        public IReadOnlyCollection<Entity> Entities
        {
            get
            {
                return entities.AsReadOnly();
            }
        }

        public IReadOnlyCollection<Light> Lights
        {
            get
            {
                return lights.AsReadOnly();
            }
        }


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



        List<Entity> entities = new List<Entity>();
        List<Light> lights = new List<Light>();

        public SceneSystem(EngineMain engine)
        {
            this.Engine = engine;
            this.EventSystem = new EventSystem();
            this.EventSystem.OnAnyEventCalled += (EventBase evt) =>
            {
                foreach(var e in entities)
                {
                    e.EventSystem.Raise(evt);
                }
            };
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
        public void Render()
        {

        }
    }
}
