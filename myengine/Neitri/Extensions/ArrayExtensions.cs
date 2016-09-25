using System;
using System.Linq;

namespace Neitri
{
	public static class ArrayExtensions
	{
		public static void ForEach<T>(this T[] array, Action<T> action)
		{
			for (int i = 0; i < array.Length; i++) action(array[i]);
		}

		/// <summary>
		/// Returns new array with element added at the end
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dst"></param>
		/// <param name="element"></param>
		/// <returns></returns>
		public static T[] Add<T>(this T[] dst, T element)
		{
			Array.Resize(ref dst, dst.Length + 1);
			dst[dst.Length - 1] = element;
			return dst;
		}

		/// <summary>
		/// Returns new array with elements added at the end
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dst"></param>
		/// <param name="elements"></param>
		/// <returns></returns>
		public static T[] Add<T>(this T[] dst, T[] elements)
		{
			Array.Resize(ref dst, dst.Length + elements.Length);
			Array.Copy(elements, 0, dst, dst.Length - elements.Length, elements.Length);
			return dst;
		}

		/// <summary>
		/// Unififed extension method that retuns number of elements of ICollection, array and others
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dst"></param>
		/// <returns></returns>
		public static long Size<T>(this T[] dst)
		{
			return dst.LongLength;
		}

		/// <summary>
		/// Returns comma separated values of ToString() of elements
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dst"></param>
		/// <returns></returns>
		public static string ToBetterString<T>(this T[] dst)
		{
			return string.Join(", ", dst.Select(e => e.ToString()).ToArray());
		}
	}
}