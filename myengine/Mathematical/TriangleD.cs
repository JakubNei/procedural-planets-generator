using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace MyEngine
{
	public struct TriangleD
	{
		public Vector3d a;
		public Vector3d b;
		public Vector3d c;



		public Vector3d CenterPos
		{
			get
			{
				return (a + b + c) / 3.0f;
			}
		}

		public Vector3d Normal
		{
			get
			{
				return Vector3d.Normalize(Vector3d.Cross(
					b - a,
					c - a
				));
			}
		}

		public TriangleD(Vector3d a, Vector3d b, Vector3d c)
		{
			this.a = a;
			this.b = b;
			this.c = c;
		}

		public SphereD ToBoundingSphere()
		{
			var c = CenterPos;
			var radius = (float)Math.Sqrt(
				Math.Max(
					Math.Max(
						a.DistanceSqr(b),
						a.DistanceSqr(c)
					),
					b.DistanceSqr(c)
				)
			);
			return new SphereD(c, radius);
		}

		// http://gamedev.stackexchange.com/questions/23743/whats-the-most-efficient-way-to-find-barycentric-coordinates
		public Vector3d CalculateBarycentric(Vector3d p)
		{
			var v0 = b - a;
			var v1 = c - a;
			var v2 = p - a;
			var d00 = v0.Dot(v0);
			var d01 = v0.Dot(v1);
			var d11 = v1.Dot(v1);
			var d20 = v2.Dot(v0);
			var d21 = v2.Dot(v1);
			var denom = d00 * d11 - d01 * d01;
			var result = new Vector3d();
			result.Y = (d11 * d20 - d01 * d21) / denom;
			result.Z = (d00 * d21 - d01 * d20) / denom;
			result.X = 1.0f - result.Y - result.Z;
			return result;
		}
	}
}
