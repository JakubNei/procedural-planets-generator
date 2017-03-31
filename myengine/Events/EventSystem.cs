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
	public class EventSystem : SingletonsPropertyAccesor
	{
		Dictionary<Type, Delegate> typeToCallbacksAlways = new Dictionary<Type, Delegate>();
		HashSet<Delegate> allDelegatesAlways = new HashSet<Delegate>();

		Dictionary<Type, Delegate> typeToCallbacksOnce = new Dictionary<Type, Delegate>();

		//List<EventSystem> passEventsTo = new List<EventSystem>();
		public event Action<IEvent> OnAnyEventCalled;


		public void Raise(IEvent evt)
		{
			try
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
				//foreach (var e in passEventsTo) e.Raise(evt);
			}
			catch(Exception e)
			{
				Log.Exception(e);
			}
		}

		//public void PassEventsTo(EventSystem eventSystem)
		//{
		//	passEventsTo.Add(eventSystem);
		//}


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



