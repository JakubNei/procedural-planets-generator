using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine.SimulateGLSL
{
	public class vec3
	{
		public float x;
		public float y;
		public float z;


		public static vec3 New(float x, float y, float z)
		{
			return new vec3() { x = x, y = y, z = z };
		}

		public static vec3 New(double x, double y, double z)
		{
			return new vec3() { x = (float)x, y = (float)y, z = (float)z };
		}



		public static vec3 operator +(vec3 l, vec3 r)
		{
			return New(l.x + r.x, l.y + r.y, l.z + r.z);
		}
		public static vec3 operator -(vec3 l, vec3 r)
		{
			return New(l.x - r.x, l.y - r.y, l.z - r.z);
		}
		public static vec3 operator *(vec3 l, vec3 r)
		{
			return New(l.x * r.x, l.y * r.y, l.z * r.z);
		}
		public static vec3 operator /(vec3 l, vec3 r)
		{
			return New(l.x / r.x, l.y / r.y, l.z / r.z);
		}


		public static vec3 operator +(vec3 l, float r)
		{
			return New(l.x + r, l.y + r, l.z + r);
		}
		public static vec3 operator +(float l, vec3 r)
		{
			return r + l;
		}
		public static vec3 operator -(vec3 l, float r)
		{
			return New(l.x - r, l.y - r, l.z - r);
		}
		public static vec3 operator -(float l, vec3 r)
		{
			return New(l - r.x, l - r.y, l - r.z);
		}
		public static vec3 operator *(vec3 l, float r)
		{
			return New(l.x * r, l.y * r, l.z * r);
		}
		public static vec3 operator *(float l, vec3 r)
		{
			return r * l;
		}
		public static vec3 operator /(vec3 l, float r)
		{
			return New(l.x / r, l.y / r, l.z / r);
		}
		public static vec3 operator /(float l, vec3 r)
		{
			return New(l / r.x, l / r.y, l / r.z);
		}




		public static vec3 operator +(vec3 l, double r)
		{
			return New(l.x + r, l.y + r, l.z + r);
		}
		public static vec3 operator +(double l, vec3 r)
		{
			return r + l;
		}
		public static vec3 operator -(vec3 l, double r)
		{
			return New(l.x - r, l.y - r, l.z - r);
		}
		public static vec3 operator -(double l, vec3 r)
		{
			return New(l - r.x, l - r.y, l - r.z);
		}
		public static vec3 operator *(vec3 l, double r)
		{
			return New(l.x * r, l.y * r, l.z * r);
		}
		public static vec3 operator *(double l, vec3 r)
		{
			return r * l;
		}
		public static vec3 operator /(vec3 l, double r)
		{
			return New(l.x / r, l.y / r, l.z / r);
		}
		public static vec3 operator /(double l, vec3 r)
		{
			return New(l / r.x, l / r.y, l / r.z);
		}
	}
}
