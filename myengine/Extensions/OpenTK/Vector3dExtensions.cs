using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

using OpenTK;

namespace MyEngine
{
    public static class Vector3dExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d RotateBy(this Vector3d vector, Quaterniond rotation)
        {
            Matrix4d rot = Matrix4d.CreateFromQuaternion(rotation);
            Vector3d newDirection;
            Vector3d.TransformVector(ref vector, ref rot, out newDirection);
            return newDirection;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d CompomentWiseMult(this Vector3d a, Vector3d b)
        {
            return new Vector3d(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Distance(this Vector3d a, Vector3d b)
        {
            return (a - b).Length;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double DistanceSqr(this Vector3d a, Vector3d b)
        {
            return (a - b).LengthSquared;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Cross(this Vector3d a, Vector3d b)
        {
            return Vector3d.Cross(a, b);

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Dot(this Vector3d a, Vector3d b)
        {
            double result;
            Vector3d.Dot(ref a, ref b, out result);
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Multiply(this Vector3d a, double scale)
        {
            return Vector3d.Multiply(a, scale);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Divide(this Vector3d a, double scale)
        {
            Vector3d.Divide(ref a, scale, out a);
            //return Vector3d.Divide(a, scale);
            return a;
        }
        /// <summary>
        /// returns -1.0 if x is less than 0.0, 0.0 if x is equal to 0.0, and +1.0 if x is greater than 0.0.
        /// https://www.opengl.org/sdk/docs/man/html/sign.xhtml
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Sign(this Vector3d a)
        {
            var ret = new Vector3d(0, 0, 0);

            if (a.X > 0) ret.X = 1;
            else if (a.X < 0) ret.X = -1;

            if (a.Y > 0) ret.Y = 1;
            else if (a.Y < 0) ret.Y = -1;

            if (a.Z > 0) ret.Z = 1;
            else if (a.Z < 0) ret.Z = -1;

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Abs(this Vector3d a)
        {
            if (a.X < 0) a.X *= -1;
            if (a.Y < 0) a.Y *= -1;
            if (a.Z < 0) a.Z *= -1;
            return a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Towards(this Vector3d from, Vector3d to)
        {
            return to - from;
        }

        public static Vector3 ToVector3(this Vector3d doubleVector)
        {
            return new Vector3(
                (float)doubleVector.X,
                (float)doubleVector.Y,
                (float)doubleVector.Z
            );
        }
    }
}
