using UnityEngine;
using System.Collections;


namespace Neitri
{
	public static class QuaternionExtensions
	{
		static bool IsFloatSane(float f)
		{
			return !(float.IsInfinity(f) || float.IsNaN(f));
		}
		public static bool IsSane(this Quaternion me)
		{
			return me.w != 0 && IsFloatSane(me.x) && IsFloatSane(me.y) && IsFloatSane(me.z) && IsFloatSane(me.w);
		}
		public static Quaternion Inverse(this Quaternion me)
		{
			return Quaternion.Inverse(me);
		}
	}
}
