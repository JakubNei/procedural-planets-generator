using System;
using System.Collections.Generic;
using System.Text;

namespace MyEngine
{
    public class UnloadFactory
    {
        static internal List<IUnloadable> unloadables = new List<IUnloadable>();
        public static void Set<T>(ref T oldObj, T newObj) where T : class
        {
            if (oldObj is IUnloadable)
            {
                if (oldObj != null)
                {
                    (oldObj as IUnloadable).Unload();
                    unloadables.Remove((oldObj as IUnloadable));
                }
                if (newObj != null) unloadables.Add((newObj as IUnloadable));
            }

            oldObj = newObj;
        }
        public static void Add(object obj)
        {
            var u = obj as IUnloadable;
            if (u!=null && !unloadables.Contains(u))
            {
                unloadables.Add(u);
            }
        }
    }
}
