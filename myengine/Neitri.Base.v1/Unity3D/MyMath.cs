using UnityEngine;

namespace Neitri
{
	/// <summary>
	/// static Math class that unifies float and double, because I just absolutely hate having to change all/some the Math calls when I change float to double.
	/// For example System.Math does not have Round for float only, so if you decide to use floats, you have to add explicit type casting everywhere, etc..
	/// </summary>
    public static class MyMath
    {
        // https://en.wikipedia.org/wiki/Linear_interpolation

        public static void Lerp(ref float from, float towards, float lerp)
        {
            from = from * (1 - lerp) + towards * lerp;
        }
        public static void Lerp(ref double from, double towards, double lerp)
        {
            from = from * (1 - lerp) + towards * lerp;
        }
        public static void Lerp(ref Vector3 from, Vector3 towards, float byAmount)
        {
            from = Vector3.Lerp(from, towards, byAmount);
        }
        public static void Lerp(ref Quaternion from, Quaternion towards, float byAmount)
        {
            from = Quaternion.Lerp(from, towards, byAmount);
        }

    }
}
