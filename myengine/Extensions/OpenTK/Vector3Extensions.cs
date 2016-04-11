using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace MyEngine
{
    public static class ExtensionMethods
    {
        public static Vector3 RotateBy(this Vector3 vector, Quaternion rotation)
        {
            Matrix4 rot = Matrix4.CreateFromQuaternion(rotation);
            Vector3 newDirection;
            Vector3.TransformVector(ref vector, ref rot, out newDirection);
            return newDirection;
        }

        public static Vector3 CompomentWiseMult(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }
        public static float Distance(this Vector3 a, Vector3 b)
        {
            return (a - b).Length;
        }
        public static float DistanceSqr(this Vector3 a, Vector3 b)
        {
            return (a - b).LengthSquared;
        }
        public static Vector3 Cross(this Vector3 a, Vector3 b)
        {
            return Vector3.Cross(a, b);
        }
        public static float Dot(this Vector3 a, Vector3 b)
        {
            return Vector3.Dot(a, b);
        }
        public static Vector3 Multiply(this Vector3 a, float scale)
        {
            return Vector3.Multiply(a, scale);
        }
        public static Vector3 Divide(this Vector3 a, float scale)
        {
            return Vector3.Divide(a, scale);
        }
    }
}
