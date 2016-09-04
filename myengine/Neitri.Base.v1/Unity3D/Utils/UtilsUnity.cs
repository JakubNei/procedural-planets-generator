using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Neitri
{
    public class UtilsUnity
    {
        static Dictionary<Type, object> find_cache = new Dictionary<Type, object>();
        public static T FindBothActiveAndInactive<T>() where T : UnityEngine.Object
        {
            T retTyped = null;
            object ret = null;
            if (find_cache.TryGetValue(typeof(T), out ret))
            {
                retTyped = (T)ret;
            }
            else
            {
                retTyped = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
                ret = retTyped;
                find_cache[typeof(T)] = ret;
            }
            return retTyped;
        }

		static Dictionary<Type, Dictionary<int, UnityEngine.Object>> typeToIdToUnityObject = new Dictionary<Type, Dictionary<int, UnityEngine.Object>>();
		public static T GetResourceObjectByInstanceId<T>(int id) where T : UnityEngine.Object
		{
			Dictionary<int, UnityEngine.Object> idToUnityObject;
			if (typeToIdToUnityObject.TryGetValue(typeof(T), out idToUnityObject) == false)
			{
				foreach (var r in Resources.FindObjectsOfTypeAll<T>())
				{
					idToUnityObject[r.GetInstanceID()] = r;
				}
			}
			UnityEngine.Object unityObject = null;
			idToUnityObject.TryGetValue(id, out unityObject);
			return unityObject as T;
		}


		public class UnityObjectSurrogate : ISerializationSurrogate
		{
			const string key = "unityObjectId";
			public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
			{
				var o = (UnityEngine.Object)obj;
				info.AddValue(key, o.GetInstanceID());
			}

			public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
			{
				return null;
			}
		}

		public static void LogThisMethod(params object[] p)
        {
            Debug.Log(TraceUtil.ThisMethod(1, p));
        }
    }
}
