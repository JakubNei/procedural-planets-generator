using System;
using OpenTK;

namespace MyEngine
{
    /** Two Dimensional Integer Coordinate Pair */

    public struct Int2
    {
        public int x;
        public int y;

        private static Int2 _zero = new Int2(0, 0);

        public static Int2 zero
        {
            get { return _zero; }
        }


        public Int2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Int2 normalized
        {
            get
            {
                float d = magnitude;
                if (d == 0) return this;
                return this / d;
            }
        }

        public int magnitude
        {
            get { return (int)Math.Sqrt(x * x + y * y); }
        }


        public int sqrMagnitude
        {
            get { return x * x + y * y; }
        }

        public long sqrMagnitudeLong
        {
            get { return (long)x * (long)x + (long)y * (long)y; }
        }


        public static Int2 operator *(Int2 a, int b)
        {
            return new Int2(a.x * b, a.y * b);
        }

        public static Int2 operator /(Int2 a, int b)
        {
            return new Int2(a.x / b, a.y / b);
        }

        public static Int2 operator /(Int2 a, float b)
        {
            return new Int2((int)(a.x / b), (int)(a.y / b));
        }


        public static Int2 operator +(Int2 a, Int2 b)
        {
            return new Int2(a.x + b.x, a.y + b.y);
        }

        public static Int2 operator -(Int2 a, Int2 b)
        {
            return new Int2(a.x - b.x, a.y - b.y);
        }

        public static bool operator ==(Int2 a, Int2 b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(Int2 a, Int2 b)
        {
            return a.x != b.x || a.y != b.y;
        }

        public static int Dot(Int2 a, Int2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

        public static long DotLong(Int2 a, Int2 b)
        {
            return (long)a.x * (long)b.x + (long)a.y * (long)b.y;
        }

        public override bool Equals(System.Object o)
        {
            if (o == null) return false;
            Int2 rhs = (Int2)o;

            return x == rhs.x && y == rhs.y;
        }

        public override int GetHashCode()
        {
            return x * 49157 + y * 98317;
        }

        /** Matrices for rotation.
	        * Each group of 4 elements is a 2x2 matrix.
	        * The XZ position is multiplied by this.
	        * So
	        * \code
	        * //A rotation by 90 degrees clockwise, second matrix in the array
	        * (5,2) * ((0, 1), (-1, 0)) = (2,-5)
	        * \endcode
	        */

        private static readonly int[] Rotations =
        {
            1, 0, //Identity matrix
            0, 1,

            0, 1,
            -1, 0,

            -1, 0,
            0, -1,

            0, -1,
            1, 0
        };

        /** Returns a new Int2 rotated 90*r degrees around the origin. */

        public static Int2 Rotate(Int2 v, int r)
        {
            r = r % 4;
            return new Int2(v.x * Rotations[r * 4 + 0] + v.y * Rotations[r * 4 + 1],
                v.x * Rotations[r * 4 + 2] + v.y * Rotations[r * 4 + 3]);
        }

        public static Int2 Min(Int2 a, Int2 b)
        {
            return new Int2(System.Math.Min(a.x, b.x), System.Math.Min(a.y, b.y));
        }

        public static Int2 Max(Int2 a, Int2 b)
        {
            return new Int2(System.Math.Max(a.x, b.x), System.Math.Max(a.y, b.y));
        }

        public static Int2 FromInt3XZ(Int3 o)
        {
            return new Int2(o.x, o.z);
        }

        public static Int3 ToInt3XZ(Int2 o)
        {
            return new Int3(o.x, 0, o.y);
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }
    }


}