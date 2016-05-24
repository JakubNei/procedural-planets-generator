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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Cot(double x)
        {
            //#define cot(V) 1.0/tan(V)
            return (double)(1.0 / Math.Tan(x));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double SmoothStep(double edge0, double edge1, double x)
        {
            var tmp = (x - edge0) / (edge1 - edge0);
            Clamp01(ref tmp);
            return tmp * tmp * (3.0f - 2.0f * tmp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Lerp(double edge0, double edge1, double x)
        {
            return edge0 * x + edge1 * (1 - x);
        }


        public static Vector3d Slerp(Vector3d start, Vector3d end, double percent)
        {
            Vector3d ret;
            Slerp(ref start, ref end, percent, out ret);
            return ret;
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

            Vector3d newDirection;
            Vector3d vector = Vector3d.UnitX;
            Matrix4d rotMat = Matrix4d.CreateFromQuaternion(rot);
            Vector3d.TransformVector(ref vector, ref rotMat, out newDirection);

            return newDirection * mag
                */
        }


        // https://www.opengl.org/sdk/docs/man/html/smoothstep.xhtml
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(double x, double min, double max)
        {
            if (x > max) return max;
            if (x < min) return min;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp01(ref double x)
        {
            if (x > 1) x = 1;
            if (x < 0) x = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp01(double x)
        {
            if (x > 1) return 1;
            if (x < 0) return 0;
            return x;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp01(ref long x)
        {
            if (x > 1) x = 1;
            if (x < 0) x = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Clamp(long x, long min, long max)
        {
            if (x > max) return max;
            if (x < min) return min;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Sqrt(double x)
        {
            return (double)Math.Sqrt(x);
        }




        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Cos(double x)
        {
            return (double)System.Math.Cos(x);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Acos(double x)
        {
            return (double)System.Math.Acos(x);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Sin(double x)
        {
            return (double)System.Math.Sin(x);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Asin(double x)
        {
            return (double)System.Math.Asin(x);
        }


        const double double__isqrt2 = (double)0.70710676908493042;

        // http://stackoverflow.com/questions/2656899/mapping-a-sphere-to-a-cube        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Vector3d Cubify(Vector3d s)
        {
            double xx2 = s.X * s.X * 2.0f;
            double yy2 = s.Y * s.Y * 2.0f;

            Vector2d v = new Vector2d(xx2 - yy2, yy2 - xx2);

            double ii = v.Y - 3.0f;
            ii *= ii;

            double isqrt = -Sqrt(ii - 12.0f * xx2) + 3.0f;

            v.X = Sqrt(v.X + isqrt);
            v.Y = Sqrt(v.Y + isqrt);
            v *= double__isqrt2;

            return s.Sign().CompomentWiseMult(new Vector3d(v.X, v.Y, 1.0f));
        }
        /// <summary>
        /// Transforms spherical direction into cube coordinates.
        /// </summary>
        /// <param name="sphere"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Sphere2Cube(Vector3d sphere)
        {

            Vector3d f = sphere.Abs();

            bool a = f.Y >= f.X && f.Y >= f.Z;
            bool b = f.X >= f.Z;

            return a ? Cubify(sphere.Xzy).Xzy : b ? Cubify(sphere.Yzx).Zxy : Cubify(sphere);
        }

    }
}
