using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine.SimulateGLSL
{
	public struct vec4
	{
		public float x;
		public float y;
		public float z;
		public float w;


		public static vec4 New(float x, float y, float z, float w)
		{
			return new vec4() { x = x, y = y, z = z, w = w };
		}

		public static vec4 New(double x, double y, double z, double w)
		{
			return new vec4() { x = (float)x, y = (float)y, z = (float)z, w = (float)w };
		}



		public static vec4 operator +(vec4 l, vec4 r)
		{
			return New(l.x + r.x, l.y + r.y, l.z + r.z, l.w + r.w);
		}
		public static vec4 operator -(vec4 l, vec4 r)
		{
			return New(l.x - r.x, l.y - r.y, l.z - r.z, l.w - r.w);
		}
		public static vec4 operator *(vec4 l, vec4 r)
		{
			return New(l.x * r.x, l.y * r.y, l.z * r.z, l.w * r.w);
		}
		public static vec4 operator /(vec4 l, vec4 r)
		{
			return New(l.x / r.x, l.y / r.y, l.z / r.z, l.w / r.w);
		}


		public static vec4 operator +(vec4 l, float r)
		{
			return New(l.x + r, l.y + r, l.z + r, l.w + r);
		}
		public static vec4 operator +(float l, vec4 r)
		{
			return r + l;
		}
		public static vec4 operator -(vec4 l, float r)
		{
			return New(l.x - r, l.y - r, l.z - r, l.w - r);
		}
		public static vec4 operator -(float l, vec4 r)
		{
			return New(l - r.x, l - r.y, l - r.z, l - r.w);
		}
		public static vec4 operator *(vec4 l, float r)
		{
			return New(l.x * r, l.y * r, l.z * r, l.w * r);
		}
		public static vec4 operator *(float l, vec4 r)
		{
			return r * l;
		}
		public static vec4 operator /(vec4 l, float r)
		{
			return New(l.x / r, l.y / r, l.z / r, l.w / r);
		}
		public static vec4 operator /(float l, vec4 r)
		{
			return New(l / r.x, l / r.y, l / r.z, l / r.w);
		}




		public static vec4 operator +(vec4 l, double r)
		{
			return New(l.x + r, l.y + r, l.z + r, l.w + r);
		}
		public static vec4 operator +(double l, vec4 r)
		{
			return r + l;
		}
		public static vec4 operator -(vec4 l, double r)
		{
			return New(l.x - r, l.y - r, l.z - r, l.w - r);
		}
		public static vec4 operator -(double l, vec4 r)
		{
			return New(l - r.x, l - r.y, l - r.z, l - r.w);
		}
		public static vec4 operator *(vec4 l, double r)
		{
			return New(l.x * r, l.y * r, l.z * r, l.w * r);
		}
		public static vec4 operator *(double l, vec4 r)
		{
			return r * l;
		}
		public static vec4 operator /(vec4 l, double r)
		{
			return New(l.x / r, l.y / r, l.z / r, l.w / r);
		}
		public static vec4 operator /(double l, vec4 r)
		{
			return New(l / r.x, l / r.y, l / r.z, l / r.w);
		}
	}
}
