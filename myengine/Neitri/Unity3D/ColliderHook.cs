using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Neitri
{
	public static partial class GameObjectExtensions
	{
		public static ColliderHook ColliderHook(this GameObject go)
		{
			return go.GetOrAddComponent<ColliderHook>();
		}
	}
	public class CollisionInfo
	{
		[NotNull] public Collider Collider { get; set; }
	}
	public class ColliderHook : MonoBehaviour
	{
		public event Action<CollisionInfo> onCollisionEnter;
		public event Action onCollisionExit;
		public event Action onCollisionStay;
		public event Action onTriggerEnter;
		public event Action onTriggerExit;
		public event Action onTriggerStay;

		void OnCollisionEnter(Collision collisionInfo)
		{
			if (onCollisionEnter != null) onCollisionEnter(new CollisionInfo()
			{
				Collider = collisionInfo.collider
			});
		}
		void OnCollisionExit(Collision collisionInfo)
		{
			if (onCollisionExit != null) onCollisionExit();
		}
		void OnCollisionStay(Collision collisionInfo)
		{
			if (onCollisionStay != null) onCollisionStay();
		}
		void OnTriggerEnter(Collider other)
		{
			if (onTriggerEnter != null) onTriggerEnter();
		}
		void OnTriggerExit(Collider other)
		{
			if (onTriggerExit != null) onTriggerExit();
		}
		void OnTriggerStay(Collider other)
		{
			if (onTriggerStay != null) onTriggerStay();
		}

		bool hasLastPosition;
		Vector3 lastPosition;
		void FixedUpdate()
		{
			var rb = this.GetComponent<Rigidbody>();
			if (rb)
			{
				if (hasLastPosition)
				{
					RaycastHit hitInfo;
					if (rb.SweepTest(rb.position.Towards(lastPosition), out hitInfo, rb.position.Distance(lastPosition)))
					{
						if (onCollisionEnter != null) onCollisionEnter(new CollisionInfo()
						{
							Collider = hitInfo.collider
						});
					}
				}
				lastPosition = rb.position;
				hasLastPosition = true;
			}
		}
	}
}
