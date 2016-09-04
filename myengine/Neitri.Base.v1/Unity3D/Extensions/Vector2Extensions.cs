using UnityEngine;
using System.Collections.Generic;

namespace Neitri
{
	public static class Vector2Extensions
	{

		public static Vector2 Closest(this IEnumerable<Vector2> vecEnum, Vector2 closestTo)
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

		public static Vector2 LerpTo(this Vector2 me, Vector2 towards, float t)
		{
			return Vector2.Lerp(me, towards, t);
		}
		public static Vector2 Round(this Vector2 me)
		{
			return new Vector2(me.x.Round(), me.y.Round());
		}
		public static Vector2 Ceil(this Vector2 me)
		{
			return new Vector2(me.x.Ceil(), me.y.Ceil());
		}
		public static Vector2 Floor(this Vector2 me)
		{
			return new Vector2(me.x.Floor(), me.y.Floor());
		}

		public static float DistanceSqr(this UnityEngine.Vector2 me, UnityEngine.Vector2 other)
		{
			return (me - other).sqrMagnitude;
		}
		public static float Distance(this UnityEngine.Vector2 me, UnityEngine.Vector2 other)
		{
			return UnityEngine.Vector2.Distance(me, other);
		}

		public static float Dot(this UnityEngine.Vector2 me, UnityEngine.Vector2 other)
		{
			return UnityEngine.Vector2.Dot(me, other);
		}

		public static float Angle(this UnityEngine.Vector2 me, UnityEngine.Vector2 other)
		{
			return UnityEngine.Vector2.Angle(me, other);
		}

		public static Vector2 Divide(this UnityEngine.Vector2 me, UnityEngine.Vector2 other)
		{
			me.Scale(new UnityEngine.Vector2(1 / other.x, 1 / other.y));
			return me;
		}

		public static Vector2 Multiply(this UnityEngine.Vector2 me, UnityEngine.Vector2 other)
		{
			me.Scale(other);
			return me;
		}

		public static Vector2 Normalized(this UnityEngine.Vector2 me)
		{
			return me.normalized;
		}

		public static float SqrMagnitude(this UnityEngine.Vector2 me)
		{
			return me.sqrMagnitude;
		}

		public static float Magnitude(this UnityEngine.Vector2 me)
		{
			return me.magnitude;
		}

		public static Vector2 Towards(this UnityEngine.Vector2 from, UnityEngine.Vector2 to)
		{
			return to - from;
		}




		/*
            javascript code to generate all the To Vector3 combinations, open Chrome web browser press f12, paste the code, press enter, done, copy paste here

            var names =   ["X","Y","0","1"];
            var returns = ["me.x","me.y","0","1"];
            var ret="\n\n";
            names.forEach(function(value1, index1){
                names.forEach(function(value2, index2){
                    names.forEach(function(value3, index3){
                        ret += "public static Vector3 To"+value1+value2+value3+"(this Vector2 me) { return new Vector3("+returns[index1]+", "+returns[index2]+", "+returns[index3]+"); }\n";
                    });
                });
            });
            ret+"\n\n"
        */
		public static Vector3 ToXXX(this Vector2 me) { return new Vector3(me.x, me.x, me.x); }
		public static Vector3 ToXXY(this Vector2 me) { return new Vector3(me.x, me.x, me.y); }
		public static Vector3 ToXX0(this Vector2 me) { return new Vector3(me.x, me.x, 0); }
		public static Vector3 ToXX1(this Vector2 me) { return new Vector3(me.x, me.x, 1); }
		public static Vector3 ToXYX(this Vector2 me) { return new Vector3(me.x, me.y, me.x); }
		public static Vector3 ToXYY(this Vector2 me) { return new Vector3(me.x, me.y, me.y); }
		public static Vector3 ToXY0(this Vector2 me) { return new Vector3(me.x, me.y, 0); }
		public static Vector3 ToXY1(this Vector2 me) { return new Vector3(me.x, me.y, 1); }
		public static Vector3 ToX0X(this Vector2 me) { return new Vector3(me.x, 0, me.x); }
		public static Vector3 ToX0Y(this Vector2 me) { return new Vector3(me.x, 0, me.y); }
		public static Vector3 ToX00(this Vector2 me) { return new Vector3(me.x, 0, 0); }
		public static Vector3 ToX01(this Vector2 me) { return new Vector3(me.x, 0, 1); }
		public static Vector3 ToX1X(this Vector2 me) { return new Vector3(me.x, 1, me.x); }
		public static Vector3 ToX1Y(this Vector2 me) { return new Vector3(me.x, 1, me.y); }
		public static Vector3 ToX10(this Vector2 me) { return new Vector3(me.x, 1, 0); }
		public static Vector3 ToX11(this Vector2 me) { return new Vector3(me.x, 1, 1); }
		public static Vector3 ToYXX(this Vector2 me) { return new Vector3(me.y, me.x, me.x); }
		public static Vector3 ToYXY(this Vector2 me) { return new Vector3(me.y, me.x, me.y); }
		public static Vector3 ToYX0(this Vector2 me) { return new Vector3(me.y, me.x, 0); }
		public static Vector3 ToYX1(this Vector2 me) { return new Vector3(me.y, me.x, 1); }
		public static Vector3 ToYYX(this Vector2 me) { return new Vector3(me.y, me.y, me.x); }
		public static Vector3 ToYYY(this Vector2 me) { return new Vector3(me.y, me.y, me.y); }
		public static Vector3 ToYY0(this Vector2 me) { return new Vector3(me.y, me.y, 0); }
		public static Vector3 ToYY1(this Vector2 me) { return new Vector3(me.y, me.y, 1); }
		public static Vector3 ToY0X(this Vector2 me) { return new Vector3(me.y, 0, me.x); }
		public static Vector3 ToY0Y(this Vector2 me) { return new Vector3(me.y, 0, me.y); }
		public static Vector3 ToY00(this Vector2 me) { return new Vector3(me.y, 0, 0); }
		public static Vector3 ToY01(this Vector2 me) { return new Vector3(me.y, 0, 1); }
		public static Vector3 ToY1X(this Vector2 me) { return new Vector3(me.y, 1, me.x); }
		public static Vector3 ToY1Y(this Vector2 me) { return new Vector3(me.y, 1, me.y); }
		public static Vector3 ToY10(this Vector2 me) { return new Vector3(me.y, 1, 0); }
		public static Vector3 ToY11(this Vector2 me) { return new Vector3(me.y, 1, 1); }
		public static Vector3 To0XX(this Vector2 me) { return new Vector3(0, me.x, me.x); }
		public static Vector3 To0XY(this Vector2 me) { return new Vector3(0, me.x, me.y); }
		public static Vector3 To0X0(this Vector2 me) { return new Vector3(0, me.x, 0); }
		public static Vector3 To0X1(this Vector2 me) { return new Vector3(0, me.x, 1); }
		public static Vector3 To0YX(this Vector2 me) { return new Vector3(0, me.y, me.x); }
		public static Vector3 To0YY(this Vector2 me) { return new Vector3(0, me.y, me.y); }
		public static Vector3 To0Y0(this Vector2 me) { return new Vector3(0, me.y, 0); }
		public static Vector3 To0Y1(this Vector2 me) { return new Vector3(0, me.y, 1); }
		public static Vector3 To00X(this Vector2 me) { return new Vector3(0, 0, me.x); }
		public static Vector3 To00Y(this Vector2 me) { return new Vector3(0, 0, me.y); }
		public static Vector3 To000(this Vector2 me) { return new Vector3(0, 0, 0); }
		public static Vector3 To001(this Vector2 me) { return new Vector3(0, 0, 1); }
		public static Vector3 To01X(this Vector2 me) { return new Vector3(0, 1, me.x); }
		public static Vector3 To01Y(this Vector2 me) { return new Vector3(0, 1, me.y); }
		public static Vector3 To010(this Vector2 me) { return new Vector3(0, 1, 0); }
		public static Vector3 To011(this Vector2 me) { return new Vector3(0, 1, 1); }
		public static Vector3 To1XX(this Vector2 me) { return new Vector3(1, me.x, me.x); }
		public static Vector3 To1XY(this Vector2 me) { return new Vector3(1, me.x, me.y); }
		public static Vector3 To1X0(this Vector2 me) { return new Vector3(1, me.x, 0); }
		public static Vector3 To1X1(this Vector2 me) { return new Vector3(1, me.x, 1); }
		public static Vector3 To1YX(this Vector2 me) { return new Vector3(1, me.y, me.x); }
		public static Vector3 To1YY(this Vector2 me) { return new Vector3(1, me.y, me.y); }
		public static Vector3 To1Y0(this Vector2 me) { return new Vector3(1, me.y, 0); }
		public static Vector3 To1Y1(this Vector2 me) { return new Vector3(1, me.y, 1); }
		public static Vector3 To10X(this Vector2 me) { return new Vector3(1, 0, me.x); }
		public static Vector3 To10Y(this Vector2 me) { return new Vector3(1, 0, me.y); }
		public static Vector3 To100(this Vector2 me) { return new Vector3(1, 0, 0); }
		public static Vector3 To101(this Vector2 me) { return new Vector3(1, 0, 1); }
		public static Vector3 To11X(this Vector2 me) { return new Vector3(1, 1, me.x); }
		public static Vector3 To11Y(this Vector2 me) { return new Vector3(1, 1, me.y); }
		public static Vector3 To110(this Vector2 me) { return new Vector3(1, 1, 0); }
		public static Vector3 To111(this Vector2 me) { return new Vector3(1, 1, 1); }


	}


}