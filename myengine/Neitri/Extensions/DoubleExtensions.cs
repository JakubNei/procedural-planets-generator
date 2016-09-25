namespace Neitri
{
	public static class DoubleExtensions
	{
		public static double Abs(this double a)
		{
			return System.Math.Abs(a);
		}

		public static double Pow(this double x, double power)
		{
			return (double)System.Math.Pow(x, power);
		}

		public static double Round(this double a)
		{
			return (double)System.Math.Round(a);
		}

		public static double Ceil(this double a)
		{
			return (double)System.Math.Ceiling(a);
		}

		public static double Floor(this double a)
		{
			return (double)System.Math.Floor(a);
		}
	}
}