using MyEngine;
using MyEngine.Components;
using MyEngine.Events;
using Neitri;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{
	public class SceneSystem : GameSystemBase
	{
		public Debug Debug => Engine.Debug;
		public Factory Factory => Engine.Factory;
		public IDependencyManager Dependency => Engine.Dependency;

		public SparseList<Entity> Entities
		{
			get
			{
				return entities;
			}
		}

		public Cubemap skyBox;
		public RenderableData DataToRender { get; private set; }

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

		public SceneSystem(EngineMain engine)
		{
			this.Engine = engine;
			this.EventSystem = new EventSystem();
			this.DataToRender = new RenderableData();
			this.EventSystem.OnAnyEventCalled += (IEvent evt) =>
			{
				foreach (var e in entities)
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
			lock (entities)
			{
				entities.Add(entity);
			}
		}

		public void Remove(Entity entity)
		{
			lock (entities)
			{
				entities.Remove(entity);
			}
		}
	}
}