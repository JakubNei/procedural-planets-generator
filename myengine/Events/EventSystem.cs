using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine.Events
{
    public class EventSystem
    {
        Dictionary<Type, Delegate> typeToCallbacks = new Dictionary<Type, Delegate>();
        HashSet<Delegate> allDelegates = new HashSet<Delegate>();

        public event Action<IEvent> OnAnyEventCalled;


        public void Raise(IEvent evt)
        {
            Delegate delegat;
            var type = evt.GetType();
            if (typeToCallbacks.TryGetValue(type, out delegat) == true)
            {
                delegat.DynamicInvoke(evt);
            }
            if (OnAnyEventCalled != null) OnAnyEventCalled(evt);
        }

        public void Register<T>(Action<T> callback) where T : IEvent
        {
            lock(allDelegates)
            {
                if (allDelegates.Contains(callback)) return;
                allDelegates.Add(callback);

                Delegate callbackToCombine;
                var type = typeof(T);
                if (typeToCallbacks.TryGetValue(type, out callbackToCombine) == false)
                {
                    typeToCallbacks[type] = callback;
                }
                else {
                    typeToCallbacks[type] = System.Delegate.Combine(callbackToCombine, callback);
                }
            }
        }

        public void Unregister<T>(Action<T> callback) where T : IEvent
        {
            lock(allDelegates)
            {
                if (allDelegates.Contains(callback) == false) return;
                allDelegates.Remove(callback);

                Delegate callbackToCombineTo;
                var type = typeof(T);
                if (typeToCallbacks.TryGetValue(type, out callbackToCombineTo))
                {                    
                    System.Delegate.Remove(callbackToCombineTo, callback);
                }
            }
        }

    }    
}
