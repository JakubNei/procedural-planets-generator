using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;


namespace MyEngine
{
	public static class ICollectionExtensions
	{
	    /// <summary>
	    /// Unififed extension method that retuns number of elements of ICollection, array and others
	    /// </summary>
	    /// <typeparam name="T"></typeparam>
	    /// <param name="dst"></param>
	    /// <returns></returns>
	    public static long Size(this ICollection dst)
	    {
	        return dst.Count;
	    }

        /// <summary>
        /// Unififed extension method that retuns number of elements of ICollection, array and others
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dst"></param>
        /// <returns></returns>
        /*public static long Size<T>(this ICollection<T> dst)
	    {
	        return dst.Count;
	    }*/

        /// <summary>
        /// Returns comma separated values of ToString() of elements of ICollection, array and others
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dst"></param>
        /// <returns></returns>
        public static string ToBetterString<T>(this ICollection<T> dst)
	    {
	        return string.Join(", ", dst.Select(e => e.ToString()).ToArray());
	    }
	}
	
	
}