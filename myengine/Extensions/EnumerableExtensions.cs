using System.Collections.Generic;
using System;

namespace MyEngine
{
	public static class EnumerableExtensions
	{
	    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
	    {
	        foreach (var item in enumerable)
	        {
	            action(item);
	        }
	    }
	    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T,int> action)
	    {
	        int index = 0;
	        foreach (var item in enumerable)
	        {
	            action(item, index);
	        }			
	    }

		public static T FindClosest<T>(this IEnumerable<T> enumerable, Func<T, float> distanceFunc, float initialClosestDist = float.MaxValue)
		{
			T closestEnumerable = default(T);
			float closestDist = initialClosestDist;
			foreach (var e in enumerable)
			{
				float dist = distanceFunc(e);
				if (dist < closestDist)
				{
					closestDist = dist;
					closestEnumerable = e;
				}
			}
			return closestEnumerable;
		}


	}
}