using MyEngine;
using MyEngine.Components;
using MyEngine.Events;
using Neitri;
using OpenTK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{
	public class SceneSystem : GameSystemBase
	{
		public Debug Debug => Engine.Debug;
		public Factory Factory => Engine.Factory;
		public FileSystem FileSystem => Engine.FileSystem;
		public IDependencyManager Dependency => Engine.Dependency;

		public IList<Entity> Entities => entities;


		public Cubemap skyBox;
		public RenderableData DataToRender { get; private set; }

		//public ISynchronizeInvoke SynchronizeInvoke { get; private set; }
		//DeferredSynchronizeInvoke.Owner deferredSynchronizeInvokeOwner;

		public EventSystem EventSystem { get; private set; }
		public EngineMain Engine { get; private set; }

		public InputSystem Input => Engine.Input;

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


		List<Entity> debugEntitites = new List<Entity>();
		ConcurrentQueue<Tuple<WorldPos, float, Vector4>> debugEntitiesToCreate = new ConcurrentQueue<Tuple<WorldPos, float, Vector4>>();
		public void DebugShere(WorldPos position, float size, Vector4 color)
		{
			debugEntitiesToCreate.Enqueue(Tuple.Create(position, size, color));
			InitializeDebug();
		}

		bool debugInitialized;
		void InitializeDebug()
		{
			if (debugInitialized) return;
			debugInitialized = true;
			EventSystem.Register<Events.PreRenderUpdate>((evt) =>
			{
				if (debugEntitiesToCreate.Count > 0)
				{
					Tuple<WorldPos, float, Vector4> tuple;
					while (debugEntitiesToCreate.Count > 0 && debugEntitiesToCreate.TryDequeue(out tuple))
					{
						var position = tuple.Item1;
						var size = tuple.Item2;
						var color = tuple.Item3;
						var e = AddEntity("debug sphere");
						e.Transform.Position = position;
						e.Transform.Scale = new Vector3(size, size, size);
						var r = e.AddComponent<MeshRenderer>();
						r.Mesh = Factory.GetMesh("sphere.obj");
						var m = new MaterialPBR(Factory);
						r.Material = m;
						m.GBufferShader = Factory.GetShader("internal/deferred.gBuffer.PBR.shader");
						m.albedo = color;
						debugEntitites.Add(e);
					}
				}
			});
			EventSystem.Register<Events.FrameEnded>((evt) =>
			{
				if (debugEntitites.Count > 0)
				{
					debugEntitites.ForEach((entity) => entity.Destroy());
					debugEntitites.Clear();
				}
			});
		}

		//List<Entity> gos = new List<Entity>();
		//public DebugShowFrustumPlanes(Camera cam)
		//{
		//	for (int i = 0; i < 6; i++)
		//	{
		//		var e = Entity.Scene.AddEntity();
		//		gos.Add(e);
		//		var r = e.AddComponent<MeshRenderer>();
		//		r.Mesh = Factory.GetMesh("internal/cube.obj");
		//		e.Transform.Scale = new Vector3(10, 10, 1);
		//	}

		//	//Entity.EventSystem.Register<EventThreadUpdate>(e => Update(e.DeltaTimeNow));
		//}
		//void Update(double deltaTime)
		//{
		//	var p = Entity.GetComponent<Camera>().GetFrustum();

		//	for (int i = 0; i < 6; i++)
		//	{

		//		 is broken maybe, furstum culling works but this doesnt make much sense

		//		gos[i].transform.position = p[i].normal * p[i].distance;
		//		gos[i].Transform.Rotation = QuaternionUtility.LookRotation(p[i].normal);
		//	}
		//}


	}
}