using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{
    public static class RandomExtensions
    {
        public static float Next(this Random random, float minValue, float maxValue)
        {
            return minValue + (float)(random.NextDouble() * (maxValue - minValue));
        }
        public static double Next(this Random random, double minValue, double maxValue)
        {
            return minValue + (double)(random.NextDouble() * (maxValue - minValue));
        }
    }
}
