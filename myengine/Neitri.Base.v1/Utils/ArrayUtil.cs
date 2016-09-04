using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neitri
{
    public static class ArrayUtil
    {
        public static void RemoveNulls<T>(ref T[] dst) where T : class
        {
            List<T> newArr = null;
            for (int i = 0; i < dst.Length; i++)
            {
                if (dst[i] == null)
                {
                    if (newArr == null) newArr = dst.ToList();
                    newArr.Remove(dst[i]);
                }
            }
            if (newArr != null) dst = newArr.ToArray();
        }
    }
}
