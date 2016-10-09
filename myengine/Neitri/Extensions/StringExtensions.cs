using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Neitri
{
	public static class StringExtensions
	{
		// from: http://stackoverflow.com/a/39696868/782022
		public static string Replace(this string str, string oldValue, string newValue, StringComparison comparison)
		{
			if (oldValue == null)
				throw new ArgumentNullException("oldValue");
			if (oldValue.Length == 0)
				throw new ArgumentException("String cannot be of zero length.", "oldValue");

			StringBuilder sb = null;

			int startIndex = 0;
			int foundIndex = str.IndexOf(oldValue, comparison);
			while (foundIndex != -1)
			{
				if (sb == null)
					sb = new StringBuilder(str.Length + (newValue != null ? Math.Max(0, 5 * (newValue.Length - oldValue.Length)) : 0));
				sb.Append(str, startIndex, foundIndex - startIndex);
				sb.Append(newValue);

				startIndex = foundIndex + oldValue.Length;
				foundIndex = str.IndexOf(oldValue, startIndex, comparison);
			}

			if (startIndex == 0)
				return str;
			sb.Append(str, startIndex, str.Length - startIndex);
			return sb.ToString();
		}

		public static string RemoveFromEnd(this string s, int count)
		{
			return s.Substring(0, s.Length - count);
		}

		public static string RemoveFromBegin(this string s, int count)
		{
			return s.Substring(count);
		}

		public static string TakeFromBegin(this string s, int count)
		{
			return s.Substring(0, count);
		}

		public static string TakeFromEnd(this string s, int count)
		{
			return s.Substring(s.Length - count, count);
		}

		public static bool IsNullOrEmpty(this string s)
		{
			return string.IsNullOrEmpty(s);
		}

		public static bool IsNullOrWhiteSpace(this string s)
		{
			return string.IsNullOrWhiteSpace(s);
		}

		public static string TakeStringBetween(this string str, string start, string end, StringComparison comparison = StringComparison.InvariantCulture)
		{
			var startIndex = str.IndexOf(start, comparison);
			if (startIndex == -1) throw new Exception("start string:'" + start + "' was not found in:'" + str + "'");
			startIndex += start.Length;
			var endIndex = str.IndexOf(end, startIndex);
			if (startIndex > endIndex) throw new Exception("start string:'" + start + "' is after end string: '" + end + "' in: '" + str + "'");
			if (endIndex == -1) throw new Exception("end string:'" + end + "' was not found in:'" + str + "'");
			return str.Substring(startIndex, endIndex - startIndex);
		}

		public static string TakeStringBetweenLast(this string str, string start, string end, StringComparison comparison = StringComparison.InvariantCulture)
		{
			var startIndex = str.LastIndexOf(start, comparison);
			if (startIndex == -1) throw new Exception("start string:'" + start + "' was not found in:'" + str + "'");
			startIndex += start.Length;
			var endIndex = str.LastIndexOf(end);
			if (startIndex > endIndex) throw new Exception("start string:'" + start + "' is after end string: '" + end + "' in: '" + str + "'");
			if (endIndex == -1) throw new Exception("end string:'" + end + "' was not found in:'" + str + "'");
			return str.Substring(startIndex, endIndex - startIndex);
		}

		public static string TakeStringAfter(this string str, string start, StringComparison comparison = StringComparison.InvariantCulture)
		{
			var startIndex = str.IndexOf(start, comparison);
			if (startIndex == -1) throw new Exception("start string:'" + start + "' was not found in:'" + str + "'");
			startIndex += start.Length;
			return str.RemoveFromBegin(startIndex);
		}

		public static string TakeStringAfterLast(this string str, string start, StringComparison comparison = StringComparison.InvariantCulture)
		{
			var startIndex = str.LastIndexOf(start, comparison);
			if (startIndex == -1) throw new Exception("start string:'" + start + "' was not found in:'" + str + "'");
			startIndex += start.Length;
			return str.RemoveFromBegin(startIndex);
		}

		public static string TakeStringBefore(this string str, string end, StringComparison comparison = StringComparison.InvariantCulture)
		{
			var endIndex = str.IndexOf(end, comparison);
			if (endIndex == -1) throw new Exception("end string:'" + end + "' was not found in:'" + str + "'");
			return str.Substring(0, endIndex);
		}

		public static string TakeStringBeforeLast(this string str, string end, StringComparison comparison = StringComparison.InvariantCulture)
		{
			var endIndex = str.LastIndexOf(end, comparison);
			if (endIndex == -1) throw new Exception("end string:'" + end + "' was not found in:'" + str + "'");
			return str.Substring(0, endIndex);
		}

		// from http://stackoverflow.com/questions/623104/byte-to-hex-string
		/// <summary>
		/// Returns byte array from string hex representation, 010204081020 would return {1, 2, 4, 8, 16, 32}.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static byte[] FromHexString(this string str)
		{
			if (str.Length == 0 || str.Length % 2 != 0)
				return new byte[0];

			byte[] buffer = new byte[str.Length / 2];
			char c;
			for (int bx = 0, sx = 0; bx < buffer.Length; ++bx, ++sx)
			{
				// Convert first half of byte
				c = str[sx];
				buffer[bx] = (byte)((c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0')) << 4);

				// Convert second half of byte
				c = str[++sx];
				buffer[bx] |= (byte)(c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0'));
			}

			return buffer;
		}

		//from http://stackoverflow.com/questions/5154970/how-do-i-create-a-hashcode-in-net-c-for-a-string-that-is-safe-to-store-in-a
		public static int GetPlatformIndependentHashCode(this string text)
		{
			if (text.IsNullOrEmpty()) return 0;
			unchecked
			{
				int hash = 23;
				foreach (char c in text)
				{
					hash = hash * 31 + c;
				}
				return hash;
			}
		}

		static Regex whiteSpaces = new Regex("\\s+", RegexOptions.Compiled);

		public static string RemoveWhiteSpaces(this string str)
		{
			return whiteSpaces.Replace(str, string.Empty);
		}

		/// <summary>
		/// Number of edits needed to turn one string into another.
		/// Taken from https://www.dotnetperls.com/levenshtein
		/// </summary>
		/// <param name="s"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static int LevenshteinDistanceTo(this string s, string t)
		{
			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			// Step 1
			if (n == 0) return m;
			if (m == 0) return n;

			// Step 2
			for (int i = 0; i <= n; d[i, 0] = i++) { }
			for (int j = 0; j <= m; d[0, j] = j++) { }

			// Step 3
			for (int i = 1; i <= n; i++)
			{
				//Step 4
				for (int j = 1; j <= m; j++)
				{
					// Step 5
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

					// Step 6
					d[i, j] = Math.Min(
						Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
						d[i - 1, j - 1] + cost);
				}
			}
			// Step 7
			return d[n, m];
		}
	}
}