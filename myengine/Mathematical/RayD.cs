using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;


namespace MyEngine
{

	public struct RayD
	{
		public Vector3d origin { get; set; }
		public Vector3d direction { get; set; }
		public RayD(Vector3d origin, Vector3d direction)
		{
			this.origin = origin;
			this.direction = direction.Normalized();
		}
		public Vector3d GetPoint(float distance)
		{
			return this.origin + this.direction * distance;
		}
		public override string ToString()
		{
			return string.Format("origin: {0}, direction: {1}", origin, direction);
		}
	}

	public struct RayDHitInfo
	{
		public double HitDistance { get; private set; }
		public bool DidHit { get; private set; }

		public static RayDHitInfo NoHit => new RayDHitInfo();
		public static RayDHitInfo HitAtRayDistance(double t) => new RayDHitInfo() { DidHit = true, HitDistance = t };

	}
}
