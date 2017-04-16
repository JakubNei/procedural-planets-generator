using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreparePlanetData
{
	public struct Vector2
	{
		public float x;
		public float y;

		public static readonly Vector2 Zero = new Vector2(0, 0);

		public Vector2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		public float this[int index]
		{
			get
			{
				if (index == 0) return x;
				return y;
			}
		}

		public static Vector2 operator +(Vector2 l, Vector2 r)
		{
			return new Vector2(l.x + r.x, l.y + r.y);
		}
		public static Vector2 operator -(Vector2 l, Vector2 r)
		{
			return new Vector2(l.x - r.x, l.y - r.y);
		}
		public static Vector2 operator /(Vector2 l, float r)
		{
			return new Vector2(l.x / r, l.y / r);
		}
		public static Vector2 operator *(Vector2 l, float r)
		{
			return new Vector2(l.x * r, l.y * r);
		}
		public static Vector2 operator *(float r, Vector2 l)
		{
			return new Vector2(l.x * r, l.y * r);
		}


		public override string ToString()
		{
			return $"x:{x},y:{y}";
		}

		public void Print()
		{
			Console.WriteLine(ToString());
		}

	}

}