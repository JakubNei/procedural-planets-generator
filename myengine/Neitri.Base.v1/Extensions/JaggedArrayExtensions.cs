using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neitri
{
    public static class JaggedArrayExtensions
    {

        public static IEnumerable<T> ToEnumerable<T>(this T[,] target)
        {
            foreach (T item in target)
                yield return item;
        }

    }
}
