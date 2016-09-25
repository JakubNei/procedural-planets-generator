using System;

namespace Neitri
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

		// from http://stackoverflow.com/questions/16340/how-do-i-generate-a-hashcode-from-a-byte-array-in-c
		public static int ComputeHashCodeSlow(this byte[] data)
		{
			unchecked
			{
				const int p = 16777619;
				int hash = (int)2166136261;

				for (int i = 0; i < data.Length; i++)
					hash = (hash ^ data[i]) * p;

				hash += hash << 13;
				hash ^= hash >> 7;
				hash += hash << 3;
				hash ^= hash >> 17;
				hash += hash << 5;
				return hash;
			}
		}

		public static int ComputeHashCodeFast(this byte[] byteArray)
		{
			// http://stackoverflow.com/questions/16340/how-do-i-generate-a-hashcode-from-a-byte-array-in-c
			var str = Convert.ToBase64String(byteArray);
			return str.GetHashCode();
			/*
			int ret = 0;
			for(int i=0; i<byteArray.Length;i++)
			{
				ret ^= byteArray[i];
			}
			return ret;
			*/
		}
	}
}