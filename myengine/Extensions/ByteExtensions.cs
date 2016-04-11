using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyEngine
{
    public static class ByteExtensions
    {
        // from http://stackoverflow.com/questions/623104/byte-to-hex-string
        /// <summary>
        /// Returns hex representation of byte array, {1, 2, 4, 8, 16, 32} would return 010204081020.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToHexString(this byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];

            byte b;

            for (int bx = 0, cx = 0; bx < bytes.Length; ++bx, ++cx)
            {
                b = ((byte)(bytes[bx] >> 4));
                c[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);

                b = ((byte)(bytes[bx] & 0x0F));
                c[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
            }

            return new string(c);
        }

    }
}
