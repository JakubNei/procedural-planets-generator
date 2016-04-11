
namespace MyEngine
{
    public static class FloatExtensions
    {
        public static float Abs(this float a)
        {
            return System.Math.Abs(a);
        }
        public static float Pow(this float x, float power)
        {
            return (float)System.Math.Pow(x, power);
        }
        public static float Round(this float a)
        {
            return (float)System.Math.Round(a);
        }
        public static float Ceil(this float a)
        {
            return (float)System.Math.Ceiling(a);
        }
        public static float Floor(this float a)
        {
            return (float)System.Math.Floor(a);
        }
        public static int CeilToInt(this float a)
        {
            return (int)System.Math.Ceiling(a);
        }
        public static int FloorToInt(this float a)
        {
            return (int)System.Math.Floor(a);
        }
        public static long CeilToLong(this float a)
        {
            return (long)System.Math.Ceiling(a);
        }
        public static long FloorToLong(this float a)
        {
            return (long)System.Math.Floor(a);
        }
    }

}