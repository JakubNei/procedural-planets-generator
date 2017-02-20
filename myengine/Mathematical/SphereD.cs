using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{
	public struct SphereD
	{
		public Vector3d center;
		public double radius;

		public SphereD(Vector3d center, double radius)
		{
			this.center = center;
			this.radius = radius;
		}
	}
}