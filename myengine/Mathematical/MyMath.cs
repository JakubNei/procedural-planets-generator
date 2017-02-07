using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

using OpenTK;

namespace MyEngine
{
	public static partial class MyMath
	{

		public static float ToRadians(float degrees)
		{
			return (float)(degrees / 180 * Math.PI);
		}
		public static float ToDegrees(float radians)
		{
			return (float)(radians * 180 / Math.PI);
		}

		public static float Cot(float x)
		{
			//#define cot(V) 1.0/tan(V)
			return (float)(1.0 / Math.Tan(x));
		}

		public static float SmoothStep(float edge0, float edge1, float x)
		{
			var tmp = (x - edge0) / (edge1 - edge0);
			Clamp01(ref tmp);
			return tmp * tmp * (3.0f - 2.0f * tmp);
		}


		public static float Lerp(float edge0, float edge1, float x)
		{
			return edge0 * x + edge1 * (1 - x);
		}


		public static Vector3 Slerp(Vector3 start, Vector3 end, float percent)
		{
			Vector3 ret;
			Slerp(ref start, ref end, percent, out ret);
			return ret;
		}

		// https://keithmaggio.wordpress.com/2011/02/15/math-magician-lerp-slerp-and-nlerp/
		public static void Slerp(ref Vector3 start, ref Vector3 end, float percent, out Vector3 ret)
		{
			var startMagnitude = start.Length;
			var endMagnitude = end.Length;
			var startNormalized = start / startMagnitude;
			var endNormalized = end / endMagnitude;


			// Dot product - the cosine of the angle between 2 vectors.
			var dot = startNormalized.Dot(endNormalized);
			// Clamp it to be in the range of Acos()
			// This may be unnecessary, but floating point
			// precision can be a fickle mistress.
			dot = Clamp(dot, -1, 1);
			// Acos(dot) returns the angle between start and end,
			// And multiplying that by percent returns the angle between
			// start and the final result.
			var theta = Acos(dot) * percent;
			var RelativeVec = endNormalized - startNormalized * dot;
			RelativeVec.Normalize();     // Orthonormal basis
										 // The final result.
			ret = ((startNormalized * Cos(theta)) + (RelativeVec * Sin(theta))) * Lerp(startMagnitude, endMagnitude, percent);


			/*
            Vector3d up;

            up = Vector3d.UnitY;
            var fromMag = from.Length;
            if (up.Dot(from)>0.8f) up = Vector3d.UnitX;
            var fromRot = Matrix4d.LookAt(Vector3d.Zero, from, up).ExtractRotation();

            up = Vector3d.UnitY;
            var toMag = to.Length;
            if (up.Dot(to)>0.8f) up = Vector3d.UnitX;
            var toRot = Matrix4d.LookAt(Vector3d.Zero, to, up).ExtractRotation();

            var mag = Lerp(fromMag, toMag, x);
            var rot = Quaterniond.Slerp(fromRot, toRot, x);

            Vector3 newDirection;
            Vector3 vector = Vector3.UnitX;
            Matrix4 rotMat = Matrix4.CreateFromQuaternion(rot);
            Vector3.TransformVector(ref vector, ref rotMat, out newDirection);

            return newDirection * mag
                */
		}


		// https://www.opengl.org/sdk/docs/man/html/smoothstep.xhtml

		public static float Clamp(float x, float min, float max)
		{
			if (x > max) return max;
			if (x < min) return min;
			return x;
		}


		public static void Clamp01(ref float x)
		{
			if (x > 1) x = 1;
			if (x < 0) x = 0;
		}

		public static float Clamp01(float x)
		{
			if (x > 1) return 1;
			if (x < 0) return 0;
			return x;
		}



		public static void Clamp01(ref int x)
		{
			if (x > 1) x = 1;
			if (x < 0) x = 0;
		}

		public static int Clamp(int x, int min, int max)
		{
			if (x > max) return max;
			if (x < min) return min;
			return x;
		}

		public static float Sqrt(float x)
		{
			return (float)Math.Sqrt(x);
		}





		public static float Cos(float x)
		{
			return (float)System.Math.Cos(x);
		}

		public static float Acos(float x)
		{
			return (float)System.Math.Acos(x);
		}

		public static float Sin(float x)
		{
			return (float)System.Math.Sin(x);
		}

		public static float Asin(float x)
		{
			return (float)System.Math.Asin(x);
		}


		const float float__isqrt2 = (float)0.70710676908493042;

		// http://stackoverflow.com/questions/2656899/mapping-a-sphere-to-a-cube        

		static Vector3 Cubify(Vector3 s)
		{
			float xx2 = s.X * s.X * 2.0f;
			float yy2 = s.Y * s.Y * 2.0f;

			Vector2 v = new Vector2(xx2 - yy2, yy2 - xx2);

			float ii = v.Y - 3.0f;
			ii *= ii;

			float isqrt = -Sqrt(ii - 12.0f * xx2) + 3.0f;

			v.X = Sqrt(v.X + isqrt);
			v.Y = Sqrt(v.Y + isqrt);
			v *= float__isqrt2;

			return s.Sign().CompomentWiseMult(new Vector3(v.X, v.Y, 1.0f));
		}
		/// <summary>
		/// Transforms spherical direction into cube coordinates.
		/// </summary>
		/// <param name="sphere"></param>
		/// <returns></returns>

		public static Vector3 Sphere2Cube(Vector3 sphere)
		{

			Vector3 f = sphere.Abs();

			bool a = f.Y >= f.X && f.Y >= f.Z;
			bool b = f.X >= f.Z;

			return a ? Cubify(sphere.Xzy).Xzy : b ? Cubify(sphere.Yzx).Zxy : Cubify(sphere);
		}

	}
}
