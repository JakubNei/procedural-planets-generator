using System;

namespace Neitri
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