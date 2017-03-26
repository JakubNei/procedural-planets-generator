using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace MyEngine
{
    public static partial class Vector3Extensions
    {
        public static Vector3d ToVector3d(this Vector3 floatVector)
        {
            return new Vector3d(
                floatVector.X,
                floatVector.Y,
                floatVector.Z
            );
        }

		public static Vector3 ToVector3(this Vector3d floatVector)
		{
			return new Vector3(
				(float)floatVector.X,
				(float)floatVector.Y,
				(float)floatVector.Z
			);
		}


		public static Vector3 RotateBy(this Vector3 direction, Quaternion rotation)
		{
			var rot = Matrix3.CreateFromQuaternion(rotation);
			Vector3 newDirection;
			Vector3.Transform(ref direction, ref rot, out newDirection);
			return newDirection;
		}
		public static Vector3d RotateBy(this Vector3d direction, Quaterniond rotation)
		{
			var rot = Matrix4d.CreateFromQuaternion(rotation);
			Vector3d newDirection;
			Vector3d.Transform(ref direction, ref rot, out newDirection);
			return newDirection;
		}


		public static Vector3 Mult(this Vector3d v3, ref Matrix4 mat)
		{
			var v4 = new Vector4(v3.ToVector3(), 1);
			v4 = v4 * mat;
			return new Vector3(v4 / v4.W);
		}


	}
}
