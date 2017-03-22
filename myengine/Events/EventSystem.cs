using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine.Events
{
	public enum EventHandling
	{
		StopPropagation,
		ContinuePropagation,
	}
	public interface IEvent
	{
	}

	// TODO: add WeakReference (weak event pattern) probably WeakEventManager https://msdn.microsoft.com/en-us/library/system.windows.weakeventmanager(v=vs.100).aspx
	public class EventSystem
	{
		Dictionary<Type, Delegate> typeToCallbacksAlways = new Dictionary<Type, Delegate>();
		HashSet<Delegate> allDelegatesAlways = new HashSet<Delegate>();

		Dictionary<Type, Delegate> typeToCallbacksOnce = new Dictionary<Type, Delegate>();

		List<EventSystem> passEventsTo = new List<EventSystem>();
		public event Action<IEvent> OnAnyEventCalled;


		public void Raise(IEvent evt)
		{
			Delegate delegat;
			var type = evt.GetType();
			if (typeToCallbacksOnce.TryGetValue(type, out delegat) == true)
			{
				delegat.DynamicInvoke(evt);
				typeToCallbacksOnce.Remove(type);
			}
			if (typeToCallbacksAlways.TryGetValue(type, out delegat) == true)
			{
				delegat.DynamicInvoke(evt);
			}

			OnAnyEventCalled?.Invoke(evt);
			foreach (var e in passEventsTo) e.Raise(evt);
		}

		/*
		public Task Raise(IEvent evt)
		{
			var tasks = new List<Task>();

			Delegate delegat;
			var type = evt.GetType();
			if (typeToCallbacks.TryGetValue(type, out delegat) == true)
			{
				tasks.Add(Task.Run(() =>
				{
					delegat.DynamicInvoke(evt);
				}));
			}
			tasks.Add(Task.Run(() =>
			{
				OnAnyEventCalled?.Invoke(evt);
			}));
			foreach (var e in passEventsTo)
			{
				tasks.Add(Task.Run(() =>
				{
					e.Raise(evt);
				}));
			}
			return Task.Run(() =>
			{
				Task.WaitAll(tasks.ToArray());
			});
		}*/
		public void PassEventsTo(EventSystem eventSystem)
		{
			passEventsTo.Add(eventSystem);
		}
		/*
        public void Register<T>(Func<T, EventHandling> callback) where T : IEvent
        {

        }
		*/

		/// <summary>
		/// Will be called always if event occurs.
		/// Register to event with implicit EventHandling.ContinuePropagation.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="callback"></param>
		public void On<T>(Action<T> callback) where T : IEvent
		{
			lock (allDelegatesAlways)
			{
				if (allDelegatesAlways.Contains(callback)) return;
				allDelegatesAlways.Add(callback);

				Delegate callbackToCombine;
				var type = typeof(T);
				if (typeToCallbacksAlways.TryGetValue(type, out callbackToCombine) == false)
				{
					typeToCallbacksAlways[type] = callback;
				}
				else
				{
					typeToCallbacksAlways[type] = System.Delegate.Combine(callbackToCombine, callback);
				}
			}
		}
		public void Once<T>(Action<T> callback) where T : IEvent
		{
			lock (typeToCallbacksOnce)
			{
				Delegate callbackToCombine;
				var type = typeof(T);
				if (typeToCallbacksOnce.TryGetValue(type, out callbackToCombine) == false)
				{
					typeToCallbacksOnce[type] = callback;
				}
				else
				{
					typeToCallbacksOnce[type] = System.Delegate.Combine(callbackToCombine, callback);
				}
			}
		}

		public void Off<T>(Action<T> callback) where T : IEvent
		{
			lock (allDelegatesAlways)
			{
				if (allDelegatesAlways.Contains(callback) == false) return;
				allDelegatesAlways.Remove(callback);

				Delegate callbackToCombineTo;
				var type = typeof(T);
				if (typeToCallbacksAlways.TryGetValue(type, out callbackToCombineTo))
				{
					System.Delegate.Remove(callbackToCombineTo, callback);
				}
			}
		}

	}
}



/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine.Events
{
	public enum EventHandling
	{
		StopPropagation,
		ContinuePropagation,
	}
	public interface IEvent
	{
		bool AllowMultiThreading { get; }
	}
	// TODO: add WeakReference (weak event pattern) probably WeakEventManager https://msdn.microsoft.com/en-us/library/system.windows.weakeventmanager(v=vs.100).aspx
	public class EventSystem
	{
		Dictionary<Type, List<Delegate>> typeToCallbacks = new Dictionary<Type, List<Delegate>>();
		HashSet<Delegate> allDelegates = new HashSet<Delegate>();
		List<EventSystem> passEventsTo = new List<EventSystem>();
		public event Action<IEvent> OnAnyEventCalled;

		DataAccessibleList<Task>.Data data = new DataAccessibleList<Task>.Data();
		DataAccessibleList<Task> t;


		List<Task> raiseWaitTasks = new List<Task>();

		public bool AllowMultiThreading = false;

		public void Raise<T>(T evt) where T : IEvent
		{
			var callbacks = typeToCallbacks.GetOrAdd(typeof(T));
			if (AllowMultiThreading && evt.AllowMultiThreading)
			{
				raiseWaitTasks.Clear();
				foreach (var c in callbacks)
				{
					var task = Task.Factory.StartNew(() => c.DynamicInvoke(evt));
					raiseWaitTasks.Add(task);
				}
				Task.WaitAll(raiseWaitTasks.ToArray());
			}
			else
			{
				foreach (var c in callbacks)
				{
					(c as Action<T>)?.Invoke(evt);
				}
			}

			if (OnAnyEventCalled != null) OnAnyEventCalled(evt);
			foreach (var e in passEventsTo) e?.Invoke(evt);
		}
		public void PassEventsTo(EventSystem eventSystem)
		{
			passEventsTo.Add(eventSystem);
		}
		
		/// <summary>
		/// Will be called always if event occurs.
		/// Register to event with implicit EventHandling.ContinuePropagation.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="callback"></param>
		public void Register<T>(Action<T> callback) where T : IEvent
		{
			lock (allDelegates)
			{
				if (allDelegates.Contains(callback)) return;
				var callbacks = typeToCallbacks.GetOrAdd(typeof(T));
				callbacks.Add(callback);
				allDelegates.Add(callback);
			}
		}

		public void Unregister<T>(Action<T> callback) where T : IEvent
		{
			lock (allDelegates)
			{
				if (allDelegates.Contains(callback) == false) return;
				allDelegates.Remove(callback);

				var callbacks = typeToCallbacks.GetOrAdd(typeof(T));
				callbacks.Remove(callback);
			}
		}

	}
}

*/
