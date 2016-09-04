using UnityEngine;
using System.Collections.Generic;

namespace Neitri
{
	public static class Vector3Extensions
	{

		public static Vector3 Closest(this IEnumerable<Vector3> vecEnum, Vector3 closestTo)
		{
			var closestDist = float.MaxValue;
			var closestElem = closestTo;
			foreach (var currentElem in vecEnum)
			{
				var currentDistance = currentElem.DistanceSqr(closestTo);
				if (currentDistance < closestDist)
				{
					closestDist = currentDistance;
					closestElem = currentElem;
				}
			}
			return closestElem;
		}

		public static Vector3 LerpTo(this Vector3 me, Vector3 towards, float t)
		{
			return Vector3.Lerp(me, towards, t);
		}
		public static Vector3 MoveTo(this Vector3 me, Vector3 towards, float byAmount)
		{
			me.x = me.x.MoveTo(towards.x, byAmount);
			me.y = me.y.MoveTo(towards.y, byAmount);
			me.z = me.z.MoveTo(towards.z, byAmount);
			return me;
		}
		public static Vector3 MoveTo(this Vector3 me, Vector3 towards, Vector3 byAmount)
		{
			me.x = me.x.MoveTo(towards.x, byAmount.x);
			me.y = me.y.MoveTo(towards.y, byAmount.y);
			me.z = me.z.MoveTo(towards.z, byAmount.z);
			return me;
		}
		public static Vector3 Reflect(this Vector3 me, Vector3 byNormal)
		{
			return Vector3.Reflect(me, byNormal);
		}
		public static Vector3 Round(this Vector3 me)
		{
			return new Vector3(me.x.Round(), me.y.Round(), me.z.Round());
		}
		public static Vector3 Ceil(this Vector3 me)
		{
			return new Vector3(me.x.Ceil(), me.y.Ceil(), me.z.Ceil());
		}
		public static Vector3 Floor(this Vector3 me)
		{
			return new Vector3(me.x.Floor(), me.y.Floor(), me.z.Floor());
		}

		public static float DistanceSqr(this Vector3 me, Vector3 other)
		{
			return (me - other).sqrMagnitude;
		}
		public static float Distance(this Vector3 me, Vector3 other)
		{
			return Vector3.Distance(me, other);
		}

		public static float Dot(this Vector3 me, Vector3 other)
		{
			return Vector3.Dot(me, other);
		}

		public static float Angle(this Vector3 me, Vector3 other)
		{
			return Vector3.Angle(me, other);
		}

		public static Quaternion LookRot(this Vector3 me)
		{
			if (me.sqrMagnitude > 0) return Quaternion.LookRotation(me);
			return Quaternion.LookRotation(Vector3.forward);
		}
		public static Quaternion LookRot(this Vector3 me, Vector3 vectorUp)
		{
			if (me.sqrMagnitude > 0) return Quaternion.LookRotation(me, vectorUp);
			return Quaternion.LookRotation(Vector3.forward, vectorUp);
		}

		public static Quaternion Euler(this Vector3 me)
		{
			return Quaternion.Euler(me);
		}

		/// <summary>
		/// Divides each number of the vector. result_i = me_i / other_i
		/// </summary>
		/// <param name="me"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		public static Vector3 Divide(this Vector3 me, Vector3 other)
		{
			me.Scale(new Vector3(1 / other.x, 1 / other.y, 1 / other.z));
			return me;
		}

		/// <summary>
		/// Multiplies each number of the vector. result_i = me_i * other_i
		/// </summary>
		/// <param name="me"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		public static Vector3 Multiply(this Vector3 me, Vector3 other)
		{
			me.Scale(other);
			return me;
		}


		public static Vector3 Normalized(this Vector3 me)
		{
			return me.normalized;
		}

		public static float SqrMagnitude(this Vector3 me)
		{
			return me.sqrMagnitude;
		}

		public static float Magnitude(this Vector3 me)
		{
			return me.magnitude;
		}

		public static Vector3 Cross(this Vector3 me, Vector3 other)
		{
			return Vector3.Cross(me, other);
		}

		public static Vector3 Towards(this Vector3 from, Vector3 to)
		{
			return to - from;
		}


		/*
            javascript code to generate all the To Vector2 combinations, open Chrome web browser press f12, paste the code, press enter, done, copy paste here

            var names =   ["X","Y","Z","0","1"];
            var returns = ["me.x","me.y","me.z","0","1"];
            var ret="\n\n";
            names.forEach(function(value1, index1){
                names.forEach(function(value2, index2){
                    ret += "public static Vector2 To"+value1+value2+"(this Vector3 me) { return new Vector2("+returns[index1]+", "+returns[index2]+"); }\n";
                });
            });
            ret+"\n\n"
        */
		public static Vector2 ToXX(this Vector3 me) { return new Vector2(me.x, me.x); }
		public static Vector2 ToXY(this Vector3 me) { return new Vector2(me.x, me.y); }
		public static Vector2 ToXZ(this Vector3 me) { return new Vector2(me.x, me.z); }
		public static Vector2 ToX0(this Vector3 me) { return new Vector2(me.x, 0); }
		public static Vector2 ToX1(this Vector3 me) { return new Vector2(me.x, 1); }
		public static Vector2 ToYX(this Vector3 me) { return new Vector2(me.y, me.x); }
		public static Vector2 ToYY(this Vector3 me) { return new Vector2(me.y, me.y); }
		public static Vector2 ToYZ(this Vector3 me) { return new Vector2(me.y, me.z); }
		public static Vector2 ToY0(this Vector3 me) { return new Vector2(me.y, 0); }
		public static Vector2 ToY1(this Vector3 me) { return new Vector2(me.y, 1); }
		public static Vector2 ToZX(this Vector3 me) { return new Vector2(me.z, me.x); }
		public static Vector2 ToZY(this Vector3 me) { return new Vector2(me.z, me.y); }
		public static Vector2 ToZZ(this Vector3 me) { return new Vector2(me.z, me.z); }
		public static Vector2 ToZ0(this Vector3 me) { return new Vector2(me.z, 0); }
		public static Vector2 ToZ1(this Vector3 me) { return new Vector2(me.z, 1); }
		public static Vector2 To0X(this Vector3 me) { return new Vector2(0, me.x); }
		public static Vector2 To0Y(this Vector3 me) { return new Vector2(0, me.y); }
		public static Vector2 To0Z(this Vector3 me) { return new Vector2(0, me.z); }
		public static Vector2 To00(this Vector3 me) { return new Vector2(0, 0); }
		public static Vector2 To01(this Vector3 me) { return new Vector2(0, 1); }
		public static Vector2 To1X(this Vector3 me) { return new Vector2(1, me.x); }
		public static Vector2 To1Y(this Vector3 me) { return new Vector2(1, me.y); }
		public static Vector2 To1Z(this Vector3 me) { return new Vector2(1, me.z); }
		public static Vector2 To10(this Vector3 me) { return new Vector2(1, 0); }
		public static Vector2 To11(this Vector3 me) { return new Vector2(1, 1); }



	}


}