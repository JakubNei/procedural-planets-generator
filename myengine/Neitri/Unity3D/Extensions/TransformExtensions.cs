using System.Collections;
using UnityEngine;

namespace Neitri
{
	public static class TransformExtensions
	{

		public static Transform SetPosition(this Transform transform, Vector3 pos)
		{
			var rb = transform.GetComponent<Rigidbody>();
			if (rb)
			{
				rb.velocity = Vector3.zero;
				rb.MovePosition(pos);
			}
			else transform.position = pos;
			return transform;
		}
		public static Vector3 GetPosition(this Transform transform)
		{
			var rb = transform.GetComponent<Rigidbody>();
			if(rb) return rb.position;
			return transform.position;
		}

		public static Transform SetRotation(this Transform transform, Quaternion rot)
		{
			var rb = transform.GetComponent<Rigidbody>();
			if (rb)
			{
				rb.angularVelocity = Vector3.zero;
				rb.MoveRotation(rot);
			}
			else transform.rotation = rot;
			return transform;
		}
		public static Quaternion GetRotation(this Transform transform)
		{
			var rb = transform.GetComponent<Rigidbody>();
			if (rb) return rb.rotation;
			return transform.rotation;
		}

		public static Transform SetVelocity(this Transform transform, Vector3 vel)
		{
			var rb = transform.GetComponent<Rigidbody>();
			if (rb)
			{
				rb.velocity = vel;
			}
			return transform;
		}
		public static Vector3 GetVelocity(this Transform transform)
		{
			var rb = transform.GetComponent<Rigidbody>();
			if (rb) return rb.velocity;
			return Vector3.zero;
		}

		public static Transform SetAngularVelocity(this Transform transform, Vector3 vel)
		{
			var rb = transform.GetComponent<Rigidbody>();
			if (rb)
			{
				rb.angularVelocity = vel;
			}
			return transform;
		}
		public static Vector3 GetAngularVelocity(this Transform transform)
		{
			var rb = transform.GetComponent<Rigidbody>();
			if (rb) return rb.angularVelocity;
			return Vector3.zero;
		}

		/// <summary>
		/// Recursive and deep variation of FindChild. Parent first search.
		/// </summary>
		public static Transform FindChildRecursive(this Transform transform, string name)
	    {
	        foreach (Transform child in transform)
	        {
	            if (child.name == name) return child;
	        }
	        Transform ret = null;
	        foreach (Transform child in transform)
	        {
	            ret = FindChildRecursive(child, name);
	            if (ret != null) return ret;
	        }
	        return null;
	    }


	    
	    /// <summary>
	    /// Destroyes all child transforms.
	    /// </summary>
	    /// <param name="removeMe"></param>
	    public static void RemoveAllChilds(this Transform transform)
	    {
	        foreach(Transform t in transform)
	        {
                GameObject.Destroy(t.gameObject);
	        }
	    }
	    
	}
	
	
	
	
}