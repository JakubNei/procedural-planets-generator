using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;


namespace MyEngine
{

	public struct Ray
	{
		public Vector3 origin { get; set; }
		public Vector3 direction { get; set; }
		public Ray(Vector3 origin, Vector3 direction)
		{
			this.origin = origin;
			this.direction = direction.Normalized();
		}
		public Vector3 GetPoint(float distance)
		{
			return this.origin + this.direction * distance;
		}
		public override string ToString()
		{
			return string.Format("origin: {0}, direction: {1}", origin, direction);
		}
	}

	public struct RayHitInfo
	{
		public float HitDistance { get; private set; }
		public bool DidHit { get; private set; }

		public static RayHitInfo NoHit => new RayHitInfo();
		public static RayHitInfo HitAtRayDistance(float t) => new RayHitInfo() { DidHit = true, HitDistance = t };

	}
}
