using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreparePlanetData
{
	public struct Vector3 : IEquatable<Vector3>
	{

		public float x;
		public float y;
		public float z;

		public static Vector3 Zero { get { return new Vector3(0, 0, 0); } }
		public static Vector3 One { get { return new Vector3(1, 1, 1); } }

		public Vector3(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}


		public float this[int index]
		{
			get
			{
				if (index == 0) return x;
				else if (index == 1) return y;
				return z;
			}
		}

		public Vector3 Normalized
		{
			get
			{
				var v = this;
				v.Normalize();
				return v;
			}
		}

		public static Vector3 operator +(Vector3 l, Vector3 r)
		{
			return new Vector3(l.x + r.x, l.y + r.y, l.z + r.z);
		}
		public static Vector3 operator -(Vector3 l, Vector3 r)
		{
			return new Vector3(l.x - r.x, l.y - r.y, l.z - r.z);
		}
		public static Vector3 operator -(Vector3 l)
		{
			return new Vector3(-l.x, -l.y, -l.z);
		}
		public static Vector3 operator /(Vector3 l, float r)
		{
			return new Vector3(l.x / r, l.y / r, l.z / r);
		}
		public static Vector3 operator *(Vector3 l, Vector3 r)
		{
			return new Vector3(l.x * r.x, l.y * r.y, l.z * r.z);
		}
		public static Vector3 operator *(Vector3 l, float r)
		{
			return new Vector3(l.x * r, l.y * r, l.z * r);
		}
		public static Vector3 operator *(float l, Vector3 r)
		{
			return new Vector3(l * r.x, l * r.y, l * r.z);
		}


		public float Magnitude => L2Norm();

		// vec3 sqr(x) sqr(y) sqr(z)
		public Vector3 Sqrt()
		{
			return new Vector3((float)Math.Sqrt(x), (float)Math.Sqrt(y), (float)Math.Sqrt(z));
		}


		public float L2Norm()
		{
			return (float)Math.Sqrt(SqrL2Norm());
		}

		//! Druhá mocnina L2-normy vektoru.
		/*!
		\return Hodnotu \f$\mathbf{||v||^2}=x^2+y^2+z^2\f$.
		*/
		public float SqrL2Norm()
		{
			return x * x + y * y + z * z;
		}
		//! Normalizace vektoru.
		/*!
		Po provedení operace bude mít vektor jednotkovou délku.
		*/
		public void Normalize()
		{
			float norm = SqrL2Norm();

			if (norm != 0)
			{
				float rn = 1 / (float)Math.Sqrt(norm);

				x *= rn;
				y *= rn;
				z *= rn;
			}
		}

		//! Vektorový součin.
		/*!
		\param v vektor \f$\mathbf{v}\f$.

		\return Vektor \f$(\mathbf{u}_x \mathbf{v}_z - \mathbf{u}_z \mathbf{v}_y,
		\mathbf{u}_z \mathbf{v}_x - \mathbf{u}_x \mathbf{v}_z,
		\mathbf{u}_x \mathbf{v}_y - \mathbf{u}_y \mathbf{v}_x)\f$.
		*/
		public Vector3 CrossProduct(Vector3 v)
		{
			return new Vector3(
				y * v.z - z * v.y,
				z * v.x - x * v.z,
				x * v.y - y * v.x);
		}

		public Vector3 Reflect(Vector3 reflectOnNormal)
		{
			// r = d−2(d⋅n)n
			var dot = this.DotProduct(ref reflectOnNormal);
			return this - (reflectOnNormal * 2 * dot);
		}
		public Vector3 Reflect(ref Vector3 reflectOnNormal)
		{
			// r = d−2(d⋅n)n
			var dot = this.DotProduct(ref reflectOnNormal);
			return this - (reflectOnNormal * 2 * dot);
		}


		//! Skalární součin.
		/*!		
		\return Hodnotu \f$\mathbf{u}_x \mathbf{v}_x + \mathbf{u}_y \mathbf{v}_y + \mathbf{u}_z \mathbf{v}_z)\f$.
		*/
		public float DotProduct(Vector3 v)
		{
			return (x * v.x) + (y * v.y) + (z * v.z);
		}
		public float DotProduct(ref Vector3 v)
		{
			return (x * v.x) + (y * v.y) + (z * v.z);
		}



		public float Distance(Vector3 v)
		{
			return (float)Math.Sqrt(DotProduct(v));
		}

		public Vector3 Abs()
		{
			return new Vector3((float)Math.Abs(x), (float)Math.Abs(y), (float)Math.Abs(z));
		}


		public byte LargestComponent(bool absolute_value)
		{
			Vector3 d = absolute_value ? this.Abs() : this;

			if (d.x > d.y)
			{
				if (d.x > d.z)
				{
					return 0;
				}
				else
				{
					return 2;
				}
			}
			else
			{
				if (d.y > d.z)
				{
					return 1;
				}
				else
				{
					return 2;
				}
			}
		}

		public Vector3 Towards(ref Vector3 other)
		{
			return other - this;
		}

		public override string ToString()
		{
			return $"x:{x},y:{y},z:{z}";
		}

		public void Print()
		{
			Console.WriteLine(ToString());
		}

		public override bool Equals(object obj)
		{
			if (obj is Vector3) return this.Equals((Vector3)obj);
			return false;
		}
		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}

		public bool Equals(Vector3 other)
		{
			return this.x == other.x && this.y == other.y && this.z == other.z;
		}
	}

}