using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;


namespace Neitri
{
	public static partial class GameObjectExtensions
	{

		/// <summary>
		/// Gets all components in the component's transfrom hiearchy
		/// </summary>
		/// <param name="component"></param>
		/// <param name="type">Can be any interface or UnityEngine.Component</param>
		/// <returns></returns>
		public static TYPE[] GetAllComponents<TYPE>(this GameObject gameObject) where TYPE : class
		{
			gameObject = gameObject.transform.root.gameObject;
			if (typeof(TYPE).IsInterface)
			{
				List<TYPE> ret = new List<TYPE>();
				TYPE add;
				foreach (var c in gameObject.GetComponentsInChildren<Component>())
				{
					add = c as TYPE;
					if (add != null) ret.Add(add);
				}
				return ret.ToArray();
			}
			else return gameObject.GetComponentsInChildren<TYPE>();
		}

		/// <summary>
		/// Gets first component in the component's transfrom hiearchy
		/// </summary>
		/// <param name="gameObject"></param>
		/// <param name="type">Can be any interface or UnityEngine.Component</param>
		/// <returns></returns>
		public static TYPE GetFirstComponent<TYPE>(this GameObject gameObject) where TYPE : class
		{
			gameObject = gameObject.transform.root.gameObject;
			if (typeof(TYPE).IsInterface)
			{
				foreach (var c in gameObject.GetComponentsInChildren<Component>())
				{
					if (c is TYPE) return c as TYPE;
				}
				return null;
			}
			else return gameObject.GetComponentInChildren<TYPE>();
		}


		public static TYPE GetOrAddComponent<TYPE>(this GameObject gameObject) where TYPE : Component
		{
			TYPE component = gameObject.GetComponent<TYPE>();
			if (component == null) component = gameObject.AddComponent<TYPE>();
			return component;
		}




		/*public static T[] GetComponentsInChildren<T>(this Entity removeMe, bool includeInactiveComponents, bool includeInactiveGameObjects) where T : Component
	    {
	        List<T> ret=new List<T>();
	        foreach(var t in removeMe.GetComponentsInChildren<Entity>(true)) {
	            ret.AddRange(t.GetComponents<T>());
	        }
	        return ret.ToArray();
	    }*/
		/*
			public static void IgnoreRaycast(this GameObject go, bool doIgnore)
			{
				if (doIgnore)
					go.SetLayerRecursive(Layer.ignoreRaycastLayerIndex);
				else
					go.SetLayerRecursive(0);
			}

			public static void IgnoreRaycast(this GameObject go)
			{
				go.IgnoreRaycast(true);
			}

			public static void SetLayerRecursive(this GameObject go, int layer)
			{
				foreach (var child in go.GetComponentsInChildren<UnityEngine.Transform>())
				{
					child.gameObject.layer = layer;
				}
			}
		*/




	}
}