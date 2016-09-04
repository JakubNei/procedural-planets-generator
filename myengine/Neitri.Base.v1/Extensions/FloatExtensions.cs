
namespace Neitri
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
		public static float Lerp(this float me, float towards, float t)
		{
			return me * (1 - t) + towards * t;
		}
		public static float MoveTo(this float me, float towards, float byAmount)
		{
			if (me < towards)
			{
				me += byAmount.Abs();
				if (me > towards) me = towards;
			}
			else if (me > towards)
			{
				me -= byAmount.Abs();
				if (me < towards) me = towards;
			}
			return me;
		}
	}

}