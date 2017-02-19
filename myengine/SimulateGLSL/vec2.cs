using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine.SimulateGLSL
{
	public struct vec2
	{
		public float x;
		public float y;

		public static vec2 New(float x, float y)
		{
			return new vec2() { x = x, y = y };
		}

		public static vec2 New(double x, double y)
		{
			return new vec2() { x = (float)x, y = (float)y };
		}



		public static vec2 operator +(vec2 l, vec2 r)
		{
			return New(l.x + r.x, l.y + r.y);
		}
		public static vec2 operator -(vec2 l, vec2 r)
		{
			return New(l.x - r.x, l.y - r.y);
		}
		public static vec2 operator *(vec2 l, vec2 r)
		{
			return New(l.x * r.x, l.y * r.y);
		}
		public static vec2 operator /(vec2 l, vec2 r)
		{
			return New(l.x / r.x, l.y / r.y);
		}


		public static vec2 operator +(vec2 l, float r)
		{
			return New(l.x + r, l.y + r);
		}
		public static vec2 operator +(float l, vec2 r)
		{
			return r + l;
		}
		public static vec2 operator -(vec2 l, float r)
		{
			return New(l.x - r, l.y - r);
		}
		public static vec2 operator -(float l, vec2 r)
		{
			return New(l - r.x, l - r.y);
		}
		public static vec2 operator *(vec2 l, float r)
		{
			return New(l.x * r, l.y * r);
		}
		public static vec2 operator *(float l, vec2 r)
		{
			return r * l;
		}
		public static vec2 operator /(vec2 l, float r)
		{
			return New(l.x / r, l.y / r);
		}
		public static vec2 operator /(float l, vec2 r)
		{
			return New(l / r.x, l / r.y);
		}




		public static vec2 operator +(vec2 l, double r)
		{
			return New(l.x + r, l.y + r);
		}
		public static vec2 operator +(double l, vec2 r)
		{
			return r + l;
		}
		public static vec2 operator -(vec2 l, double r)
		{
			return New(l.x - r, l.y - r);
		}
		public static vec2 operator -(double l, vec2 r)
		{
			return New(l - r.x, l - r.y);
		}
		public static vec2 operator *(vec2 l, double r)
		{
			return New(l.x * r, l.y * r);
		}
		public static vec2 operator *(double l, vec2 r)
		{
			return r * l;
		}
		public static vec2 operator /(vec2 l, double r)
		{
			return New(l.x / r, l.y / r);
		}
		public static vec2 operator /(double l, vec2 r)
		{
			return New(l / r.x, l / r.y);
		}
	}
}
