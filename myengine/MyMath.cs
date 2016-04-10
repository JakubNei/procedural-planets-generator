using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{
    public static class MyMath
    {
        public static float Cot(float x)
        {
            //#define cot(V) 1.0/tan(V)
            return (float)(1.0 / Math.Tan(x));
        }
        public static float SmoothStep(float edge0, float edge1, float x)
        {
            float tmp = Clamp((x - edge0) / (edge1 - edge0), 0, 1);
            return tmp * tmp * (3.0f - 2.0f * tmp);
        }

        public static float Clamp(float x, float min, float max)
        {
            if (x > max) return max;
            if (x < min) return min;
            return x;
        }

        public static int Clamp(int x, int min, int max)
        {
            if (x > max) return max;
            if (x < min) return min;
            return x;
        }

        public static float Sqrt(float x)
        {
            return (float)Math.Sqrt(x);
        }
    }
}
