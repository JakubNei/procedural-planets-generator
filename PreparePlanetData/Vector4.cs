using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PreparePlanetData
{
	public struct Vector4
	{

		public float x;
		public float y;
		public float z;
		public float w;


		public Vector4(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}


		public float this[int index]
		{
			get
			{
				if (index == 0) return x;
				else if (index == 1) return y;
				else if (index == 2) return y;
				return w;
			}
		}


		public Vector3 rgb()
		{
			return new Vector3(x, y, z);
		}

	}
}