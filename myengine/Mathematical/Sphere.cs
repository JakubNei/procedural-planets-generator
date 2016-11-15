using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{
	public struct Sphere
	{
		public Vector3d center;
		public double radius;

		public Sphere(Vector3d center, double radius)
		{
			this.center = center;
			this.radius = radius;
		}
	}
}