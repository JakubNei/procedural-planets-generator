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
		const double double__isqrt2 = (double)0.70710676908493042;

		#region Convenience extensions

		public static double ToRadians(double degrees)
		{
			return (double)(degrees / 180 * Math.PI);
		}
		public static double ToDegrees(double radians)
		{
			return (double)(radians * 180 / Math.PI);
		}

		public static double Cot(double x)
		{
			//#define cot(V) 1.0/tan(V)
			return (double)(1.0 / Math.Tan(x));
		}

		public static double Clamp(double x, double min, double max)
		{
			if (x > max) return max;
			if (x < min) return min;
			return x;
		}


		public static void Clamp01(ref double x)
		{
			if (x > 1) x = 1;
			if (x < 0) x = 0;
		}

		public static double Clamp01(double x)
		{
			if (x > 1) return 1;
			if (x < 0) return 0;
			return x;
		}



		public static void Clamp01(ref long x)
		{
			if (x > 1) x = 1;
			if (x < 0) x = 0;
		}

		public static long Clamp(long x, long min, long max)
		{
			if (x > max) return max;
			if (x < min) return min;
			return x;
		}

		public static double Sqrt(double x)
		{
			return (double)Math.Sqrt(x);
		}





		public static double Cos(double x)
		{
			return (double)System.Math.Cos(x);
		}

		public static double Acos(double x)
		{
			return (double)System.Math.Acos(x);
		}

		public static double Sin(double x)
		{
			return (double)System.Math.Sin(x);
		}

		public static double Asin(double x)
		{
			return (double)System.Math.Asin(x);
		}
		#endregion

		#region GLSL like operations
		public static double SmoothStep(double edge0, double edge1, double x)
		{
			var tmp = (x - edge0) / (edge1 - edge0);
			Clamp01(ref tmp);
			return tmp * tmp * (3.0f - 2.0f * tmp);
		}

		public static Vector3d Lerp(Vector3d start, Vector3d end, double t)
		{
			return start * t + end * (1 - t);
		}
		public static Vector3d Slerp(Vector3d start, Vector3d end, double t)
		{
			var startMagnitude = start.Length;
			var endMagnitude = end.Length;
			var startNormalized = start / startMagnitude;
			var endNormalized = end / endMagnitude;
			var dot = startNormalized.Dot(endNormalized);
			dot = Clamp(dot, -1, 1);
			var theta = Acos(dot) * t;
			var RelativeVec = endNormalized - startNormalized * dot;
			RelativeVec.Normalize();
			return
				(startNormalized * Cos(theta)) + (RelativeVec * Sin(theta))
				* Lerp(startMagnitude, endMagnitude, t);
		}

		public static double Lerp(double edge0, double edge1, double x)
		{
			return edge0 * x + edge1 * (1 - x);
		}

		// https://keithmaggio.wordpress.com/2011/02/15/math-magician-lerp-slerp-and-nlerp/
		public static void Slerp(ref Vector3d start, ref Vector3d end, double percent, out Vector3d ret)
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
		}



		#endregion


		#region Intersections

		public static RayDHitInfo CastRay(this RayD ray, SphereD sphere)
		{
			var u = ray.direction;
			var A = ray.origin;
			var S = sphere.center;
			var r = sphere.radius;

			double _a = 1;
			// if u is not normalized then:
			//_a = u.x * u.x + u.y * u.y + u.z * u.z;

			double _b =
					(
						(A.X * u.X) - (u.X * S.X) +
						(A.Y * u.Y) - (u.Y * S.Y) +
						(A.Z * u.Z) - (u.Z * S.Z)
					) * 2;

			double _c =
					((A.X - S.X) * (A.X - S.X)) +
					((A.Y - S.Y) * (A.Y - S.Y)) +
					((A.Z - S.Z) * (A.Z - S.Z)) +
					-(r * r);

			// a*t*t + b*t + c

			double determinant = _b * _b - 4 * _a * _c;

			if (determinant < 0) return RayDHitInfo.NoHit; // no hit

			double _2a = 2 * _a;

			if (determinant > 0) // two hits
			{
				double dSqrt = (double)Math.Sqrt(determinant);
				double t1 = (-_b + dSqrt) / _2a;
				double t2 = (-_b - dSqrt) / _2a;

				if (t1 < t2) return RayDHitInfo.HitAtRayDistance(t1);
				else return RayDHitInfo.HitAtRayDistance(t2);
			}

			if (determinant == 0) // one hit
			{
				double t1 = -_b / _2a;
				return RayDHitInfo.HitAtRayDistance(t1);
			}

			return RayDHitInfo.NoHit;
		}


		// Thomas Moller ray triangle intersection
		// or http://www.lighthouse3d.com/tutorials/maths/ray-triangle-intersection/
		// release 0.054317 us
		// debug   0.602176 us
		public static RayDHitInfo CastRay(this RayD ray, TriangleD triangle)
		{
			// TODO
			/*
			Zde doplòte kód pro testování existence prùseèíku parsku s trojúhelníkem.
			Vypoètený parametr t spoleènì s trojúhelníkem triangle vložte ma konci této metody
			do metody ray.closest_hit, napø. takto

			return ray.closest_hit( t, triangle );

			Tato metoda vrátí true v pøípadì, že zadaný parametr t je menší než pøedchozí a zapíše
			do paprsku i ukazatel na zasažený trojúhelník, pøes který (metoda target) je možno následnì zjistit
			normálu (target.normal()) nebo materiál v bodì zásahu.
			*/

			const double ____EPSILON = 0.000001f;

			Vector3d V1 = triangle.a;
			Vector3d V2 = triangle.b;
			Vector3d V3 = triangle.c;
			Vector3d O = ray.origin;  //RayD origin
			Vector3d D = ray.direction;  //RayD direction


			Vector3d e1, e2;  //Edge1, Edge2
			Vector3d P, Q, T;
			double det, inv_det, u, v;
			double t;

			//Find vectors for two edges sharing V1
			e1 = V2 - V1;
			e2 = V3 - V1;
			//Begin calculating determinant - also used to calculate u parameter
			P = D.Cross(e2);
			//if determinant is near zero, ray lies in plane of triangle
			det = e1.Dot(P);
			//NOT CULLING
			if (det > -____EPSILON && det < ____EPSILON)
			{
				return RayDHitInfo.NoHit;
			}
			inv_det = 1.0f / det;

			//calculate distance from V1 to ray origin
			T = O - V1;

			//Calculate u parameter and test bound
			u = T.Dot(P) * inv_det;
			//The intersection lies outside of the triangle
			if (u < 0.0f || u > 1.0f)
			{
				return RayDHitInfo.NoHit;
			}

			//Prepare to test v parameter
			Q = T.Cross(e1);

			//Calculate V parameter and test bound
			v = D.Dot(Q) * inv_det;
			//The intersection lies outside of the triangle
			if (v < 0.0f || u + v > 1.0f)
			{
				return RayDHitInfo.NoHit;
			}

			t = e2.Dot(Q) * inv_det;

			if (t > ____EPSILON)
			{ //ray intersection

				return RayDHitInfo.HitAtRayDistance(t);
			}


			return RayDHitInfo.NoHit; // trojúhelník nenalezen
		}





		// http://gamedev.stackexchange.com/questions/18436/most-efficient-aabb-vs-ray-collision-algorithms
		// release 0.050010 us
		// debug   0.069480 us
		public static RayDHitInfo CastRay(this RayD ray, Bounds bounds)
		{

			// lb is the corner of AABB with minimal coordinates - left bottom, rt is maximal corner
			// r.org is origin of ray
			double tt1 = (bounds.Min.X - ray.origin.X) / ray.direction.X;
			double t2 = (bounds.Max.X - ray.origin.X) / ray.direction.X;
			double t3 = (bounds.Min.Y - ray.origin.Y) / ray.direction.Y;
			double t4 = (bounds.Max.Y - ray.origin.Y) / ray.direction.Y;
			double t5 = (bounds.Min.Z - ray.origin.Z) / ray.direction.Z;
			double t6 = (bounds.Max.Z - ray.origin.Z) / ray.direction.Z;

			double tmin = Math.Max(Math.Max(Math.Min(tt1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
			double tmax = Math.Min(Math.Min(Math.Max(tt1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

			// if tmax < 0, ray (line) is intersecting AABB, but whole AABB is behing us
			if (tmax < 0)
			{
				//t0 = tmax;				
				return RayDHitInfo.NoHit;
			}

			//if (tmin > t1) return false;

			// if tmin > tmax, ray doesn't intersect AABB
			if (tmin > tmax)
			{
				//t0 = tmax;				
				return RayDHitInfo.NoHit;
			}

			return RayDHitInfo.HitAtRayDistance(tmin);
			//t1 = tmin;
		}


		public static bool Intersects(this SphereD sphere, TriangleD triangle) => Intersects(triangle, sphere);
		public static bool Intersects(this TriangleD triangle, SphereD sphere)
		{
			// from http://realtimecollisiondetection.net/blog/?p=103
			var A = triangle.a - sphere.center;
			var B = triangle.b - sphere.center;
			var C = triangle.c - sphere.center;
			var rr = sphere.radius * sphere.radius;

			var V = (B - A).Cross(C - A);
			var d = A.Dot(V);
			var e = V.Dot(V);
			var sep1 = (d * d > rr * e);
			var aa = A.Dot(A);
			var ab = A.Dot(B);
			var ac = A.Dot(C);
			var bb = B.Dot(B);
			var bc = B.Dot(C);
			var cc = C.Dot(C);
			var sep2 = (aa > rr) & (ab > aa) & (ac > aa);
			var sep3 = (bb > rr) & (ab > bb) & (bc > bb);
			var sep4 = (cc > rr) & (ac > cc) & (bc > cc);
			var AB = B - A;
			var BC = C - B;
			var CA = A - C;
			var d1 = ab - aa;
			var d2 = bc - bb;
			var d3 = ac - cc;
			var e1 = AB.Dot(AB);
			var e2 = BC.Dot(BC);
			var e3 = CA.Dot(CA);
			var Q1 = A * e1 - d1 * AB;
			var Q2 = B * e2 - d2 * BC;
			var Q3 = C * e3 - d3 * CA;
			var QC = C * e1 - Q1;
			var QA = A * e2 - Q2;
			var QB = B * e3 - Q3;
			var sep5 = (Q1.Dot(Q1) > rr * e1 * e1) & (Q1.Dot(QC) > 0);
			var sep6 = (Q2.Dot(Q2) > rr * e2 * e2) & (Q2.Dot(QA) > 0);
			var sep7 = (Q3.Dot(Q3) > rr * e3 * e3) & (Q3.Dot(QB) > 0);
			return !(sep1 | sep2 | sep3 | sep4 | sep5 | sep6 | sep7);
			// or this, but i failed to convert it into shere x triangle http://www.phatcode.net/articles.php?id=459
		}


		#endregion
	}
}
