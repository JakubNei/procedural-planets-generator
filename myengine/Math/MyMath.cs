using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

using OpenTK;

namespace MyEngine
{
    public static class MyMath
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cot(float x)
        {
            //#define cot(V) 1.0/tan(V)
            return (float)(1.0 / Math.Tan(x));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothStep(float edge0, float edge1, float x)
        {
            var tmp = (x - edge0) / (edge1 - edge0);
            Clamp01(ref tmp);
            return tmp * tmp * (3.0f - 2.0f * tmp);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double SmoothStep(double edge0, double edge1, double x)
        {
            var tmp = (x - edge0) / (edge1 - edge0);
            Clamp01(ref tmp);
            return tmp * tmp * (3.0 - 2.0 * tmp);
        }
        // https://www.opengl.org/sdk/docs/man/html/smoothstep.xhtml
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float x, float min, float max)
        {
            if (x > max) return max;
            if (x < min) return min;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(double x, double min, double max)
        {
            if (x > max) return max;
            if (x < min) return min;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp01(ref float x)
        {
            if (x > 1) x = 1;
            if (x < 0) x = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp01(ref double x)
        {
            if (x > 1) x = 1;
            if (x < 0) x = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp01(ref int x)
        {
            if (x > 1) x = 1;
            if (x < 0) x = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int x, int min, int max)
        {
            if (x > max) return max;
            if (x < min) return min;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sqrt(float x)
        {
            return (float)Math.Sqrt(x);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Sqrt(double x)
        {
            return Math.Sqrt(x);
        }

        // http://stackoverflow.com/questions/2656899/mapping-a-sphere-to-a-cube
        const float isqrt2 = 0.70710676908493042f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            v *= isqrt2;

            return s.Sign().CompomentWiseMult(new Vector3(v.X, v.Y, 1.0f));
        }
        /// <summary>
        /// Transforms spherical direction into cube coordinates.
        /// </summary>
        /// <param name="sphere"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Sphere2Cube(Vector3 sphere)
        {

            Vector3 f = sphere.Abs();

            bool a = f.Y >= f.X && f.Y >= f.Z;
            bool b = f.X >= f.Z;

            return a ? Cubify(sphere.Xzy).Xzy : b ? Cubify(sphere.Yzx).Zxy : Cubify(sphere);
        }
    }
}
