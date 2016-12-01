using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace MyEngine
{
	public static class QuaternionUtility
	{
		public static Quaternion LookRotation(Vector3 forward)
		{
			var up = Vector3.UnitY;
			if (up == forward) up = Vector3.UnitX;
			Matrix4 rot = Matrix4.LookAt(Vector3.Zero, forward, up).Inverted();
			return rot.ExtractRotation();
		}
		public static Quaternion LookRotation(Vector3 forward, Vector3 up)
		{
			Matrix4 rot = Matrix4.LookAt(Vector3.Zero, forward, up).Inverted();
			return rot.ExtractRotation();
		}



		public static Quaternion FromEulerAngles(float x, float y, float z)
		{
			return FromEulerAngles(new Vector3(x, y, z));
		}
		public static Quaternion FromEulerAngles(Vector3 eulerAngles)
		{
			return FromEulerAngles(ref eulerAngles);
		}
		/// <summary>
		/// Taken from https://github.com/opentk/opentk/pull/187
		/// </summary>
		/// <param name="eulerAngles"></param>
		/// <returns></returns>
		public static Quaternion FromEulerAngles(ref Vector3 eulerAngles)
		{
			float c1 = (float)Math.Cos(eulerAngles.Y * 0.5f);
			float c2 = (float)Math.Cos(eulerAngles.X * 0.5f);
			float c3 = (float)Math.Cos(eulerAngles.Z * 0.5f);
			float s1 = (float)Math.Sin(eulerAngles.Y * 0.5f);
			float s2 = (float)Math.Sin(eulerAngles.X * 0.5f);
			float s3 = (float)Math.Sin(eulerAngles.Z * 0.5f);

			var result = new Vector3();
			float w = c1 * c2 * c3 - s1 * s2 * s3;
			result.X = s1 * s2 * c3 + c1 * c2 * s3;
			result.Y = s1 * c2 * c3 + c1 * s2 * s3;
			result.Z = c1 * s2 * c3 - s1 * c2 * s3;

			return new Quaternion(result, w);
		}

	}

}
