using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Version 2017-04-12
/// Many useful extensions I've made over the years.
/// All in one class.
/// </summary>
public static class NeitriExtensions
{
	#region Enum extensions
	public static bool HasFlag(this Enum variable, Enum value)
	{
		if (variable == null)
			return false;

		if (value == null)
			throw new ArgumentNullException("value");

		// Not as good as the .NET 4 version of this function, but should be good enough
		if (!Enum.IsDefined(variable.GetType(), value))
		{
			throw new ArgumentException(string.Format(
				"Enumeration type mismatch.  The flag is of type '{0}', was expecting '{1}'.",
				value.GetType(), variable.GetType()));
		}

		ulong num = Convert.ToUInt64(value);
		return ((Convert.ToUInt64(variable) & num) == num);
	}
	#endregion

	#region Action extensions
	static public void Raise(this Action handler)
	{
		if (handler != null) handler();
	}

	static public void Raise<T1>(this Action<T1> handler, T1 a)
	{
		if (handler != null) handler(a);
	}

	static public void Raise<T1, T2>(this Action<T1, T2> handler, T1 a, T2 b)
	{
		if (handler != null) handler(a, b);
	}

	static public void Raise<T1, T2, T3>(this Action<T1, T2, T3> handler, T1 a, T2 b, T3 c)
	{
		if (handler != null) handler(a, b, c);
	}

	static public void Raise<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> handler, T1 a, T2 b, T3 c, T4 d)
	{
		if (handler != null) handler(a, b, c, d);
	}
	#endregion

	#region IDictionary<> extensions
	/// <summary>
	/// Tries to TryGetValue value by key, if not found new value is created with value of defaultValue, new value is NOT added to the Dictionary
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	/// <param name="dictionary"></param>
	/// <param name="key"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
	{
		TValue value;
		if (!dictionary.TryGetValue(key, out value))
		{
			value = defaultValue;
		}
		return value;
	}

	/// <summary>
	/// Tries to TryGetValue value by key, if not found new value is created, new value is NOT added to the Dictionary.
	/// If TValue is class new value is new TValue(), otherwise its default(TValue)
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	/// <param name="dictionary"></param>
	/// <param name="key"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
	{
		TValue value;
		if (!dictionary.TryGetValue(key, out value))
		{
			value = default(TValue);
		}
		return value;
	}

	/// <summary>
	/// Tries to TryGetValue value by key, if not found new value is created with value of defaultValue, new value is added to the Dictionary.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	/// <param name="dictionary"></param>
	/// <param name="key"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
	{
		TValue value;
		if (!dictionary.TryGetValue(key, out value))
		{
			value = defaultValue;
			dictionary[key] = value;
		}
		return value;
	}

	/// <summary>
	/// Tries to TryGetValue value by key, if not found new value is created, new value is added to the Dictionary.
	/// TValue must have parameterless constructor: new TValue()
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	/// <param name="dictionary"></param>
	/// <param name="key"></param>
	/// <param name="defaultValue"></param>
	/// <returns></returns>
	public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
	{
		TValue value;
		if (dictionary.TryGetValue(key, out value) == false)
		{
			value = new TValue();
			dictionary[key] = value;
		}
		return value;
	}

	/// <summary>
	/// Tries to TryGetValue value by key, if not found new value is created with value returned by Func defaultValueFunc, new value is added to the Dictionary
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	/// <param name="dictionary"></param>
	/// <param name="key"></param>
	/// <param name="defaultValueFunc">Is caled only if it is needed, that is when key is not found</param>
	/// <returns></returns>
	public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValueFunc)
	{
		TValue value;
		if (!dictionary.TryGetValue(key, out value))
		{
			value = defaultValueFunc();
			dictionary[key] = value;
		}
		return value;
	}

	/// <summary>
	/// Tries to TryGetValue value by key, if not found new value is created with value returned by Func defaultValueFunc, new value is added to the Dictionary
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	/// <param name="dictionary"></param>
	/// <param name="key"></param>
	/// <param name="defaultValueFunc">Is caled only if it is needed, that is when key is not found</param>
	/// <returns></returns>
	public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> defaultValueFunc)
	{
		TValue value;
		if (!dictionary.TryGetValue(key, out value))
		{
			value = defaultValueFunc(key);
			dictionary[key] = value;
		}
		return value;
	}
	#endregion

	#region List<T> extensions

	public static T First<T>(this List<T> list)
	{
		return list[0];
	}
	public static T FirstOrDefault<T>(this List<T> list)
	{
		if (list.Count > 0)
			return list[0];
		return default(T);
	}

	public static T Last<T>(this List<T> list)
	{
		return list[list.Count - 1];
	}
	public static T LastOrDefault<T>(this List<T> list)
	{
		if (list.Count > 0)
			return list[list.Count - 1];
		return default(T);
	}

	public static int Count<T>(this List<T> list)
	{
		return list.Count;
	}
	public static long LongCount<T>(this List<T> list)
	{
		return (long)list.Count;
	}

	public static void Resize<T>(this List<T> list, int newCount, T defaultValue)
	{
		int currentCount = list.Count;
		if (newCount < currentCount)
			list.RemoveRange(newCount, currentCount - newCount);
		else if (newCount > currentCount)
		{
			if (newCount > list.Capacity)//this bit is purely an optimisation, to avoid multiple automatic capacity changes.
				list.Capacity = newCount;
			list.AddRange(Enumerable.Repeat(defaultValue, newCount - currentCount));
		}
	}

	public static void Resize<T>(this List<T> list, int newCount)
	{
		Resize(list, newCount, default(T));
	}

	#endregion

	#region IList<T> and IList extensions
	public static void AddRange(this IList me, IEnumerable enumerable)
	{
		if (me == null) throw new NullReferenceException("me");
		if (enumerable == null) throw new NullReferenceException("enumerable");
		foreach (object e in enumerable)
		{
			me.Add(e);
		}
	}
	public static void AddRange(this IList me, IList other)
	{
		if (me == null) throw new NullReferenceException("me");
		if (other == null) throw new NullReferenceException("other");
		foreach (object e in other)
		{
			me.Add(e);
		}
	}
	public static void AddRange<T>(this IList<T> me, IEnumerable<T> enumerable)
	{
		if (me == null) throw new NullReferenceException("me");
		if (enumerable == null) throw new NullReferenceException("enumerable");
		foreach (var e in enumerable)
		{
			me.Add(e);
		}
	}
	public static void AddRange<T>(this IList<T> me, IList<T> other)
	{
		if (me == null) throw new NullReferenceException("me");
		if (other == null) throw new NullReferenceException("other");
		foreach (var e in other)
		{
			me.Add(e);
		}
	}
	#endregion

	#region IEnumerable<T> extensions
	public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
	{
		foreach (var item in enumerable)
		{
			action(item);
		}
	}

	public static void ForEach<TInput, TResult>(this IEnumerable<TInput> enumerable, Func<TInput, TResult> action)
	{
		foreach (var item in enumerable)
		{
			action(item);
		}
	}

	public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
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

	public static IEnumerable<T> Concat<T>(this IEnumerable<IEnumerable<T>> enumerable)
	{
		IEnumerable<T> result = null;
		foreach (var e in enumerable)
		{
			if (result == null) result = e;
			else result = result.Concat(e);
		}
		return result;
	}

	public static int GetContentsHashCode<T>(this IEnumerable<T> enumerable)
	{
		int hashCode = 0;
		foreach (var e in enumerable)
			hashCode ^= e.GetHashCode();
		return hashCode;
	}

	/// <summary>
	/// Ruby style Array.map
	/// </summary>
	public static IEnumerable<TResult> Map<TInput, TResult>(this IEnumerable<TInput> enumerable, Func<TInput, TResult> selector)
	{
		return enumerable.Select(selector);
	}

	/// <summary>
	/// Ruby style Array.each
	/// </summary>
	public static void Each<T>(this IEnumerable<T> enumerable, Action<T> action)
	{
		enumerable.ForEach(action);
	}

	/// <summary>
	/// Ruby style Array.uniq
	/// </summary>
	public static IEnumerable<T> Uniq<T>(this IEnumerable<T> enumerable)
	{
		return enumerable.Distinct();
	}

	#endregion

	#region IEnumerable<char> extensions
	public static string Join(this IEnumerable<char> enumerable, char glue)
	{
		return string.Join(glue.ToString(), enumerable.Select(c => c.ToString()).ToArray());
	}
	public static string Join(this IEnumerable<char> enumerable, string glue)
	{
		return string.Join(glue, enumerable.Select(c => c.ToString()).ToArray());
	}
	#endregion

	#region IEnumerable<string> extensions	
	public static string Join(this IEnumerable<string> enumerable, string glue)
	{
		return string.Join(glue, enumerable.ToArray());
	}
	public static string Join(this IEnumerable<string> enumerable, char glue)
	{
		return string.Join(glue.ToString(), enumerable.ToArray());
	}
	#endregion

	#region float extensions
	public static float Abs(this float a)
	{
		return System.Math.Abs(a);
	}
	public static float Pow(this float x, float power)
	{
		return (float)System.Math.Pow(x, power);
	}
	public static float Max(this float me, float other)
	{
		if (me > other) return me;
		return other;
	}
	public static float Min(this float me, float other)
	{
		if (me < other) return me;
		return other;
	}
	public static float Clamp(this float me, float inclusiveFrom, float inclusiveTo)
	{
		if (me > inclusiveTo) return inclusiveTo;
		if (me < inclusiveFrom) return inclusiveFrom;
		return me;
	}
	public static float Lerp(this float me, float towards, float t)
	{
		return me * (1 - t) + towards * t;
	}
	public static float MoveTo(this float me, float towards, float byAmount)
	{
		if (me < towards)
		{
			me += byAmount.Abs();
			if (me > towards) me = towards;
		}
		else if (me > towards)
		{
			me -= byAmount.Abs();
			if (me < towards) me = towards;
		}
		return me;
	}

	public static float Round(this float a)
	{
		return (float)System.Math.Round(a);
	}
	public static float RoundToInt(this float a)
	{
		return (int)System.Math.Round(a);
	}

	public static float Ceil(this float a)
	{
		return (float)System.Math.Ceiling(a);
	}

	public static int CeilToInt(this float a)
	{
		return (int)System.Math.Ceiling(a);
	}

	public static float Floor(this float a)
	{
		return (float)System.Math.Floor(a);
	}

	public static int FloorToInt(this float a)
	{
		return (int)System.Math.Floor(a);
	}

	public static long CeilToLong(this float a)
	{
		return (long)System.Math.Ceiling(a);
	}

	public static long FloorToLong(this float a)
	{
		return (long)System.Math.Floor(a);
	}

	public static int ToInt(this float a)
	{
		return (int)a;
	}
	public static long ToLong(this float a)
	{
		return (long)a;
	}
	#endregion

	#region double extensions
	public static double Abs(this double a)
	{
		return System.Math.Abs(a);
	}
	public static double Pow(this double a, double power)
	{
		return (double)System.Math.Pow(a, power);
	}
	public static double Max(this double me, double other)
	{
		if (me > other) return me;
		return other;
	}
	public static double Min(this double me, double other)
	{
		if (me < other) return me;
		return other;
	}
	public static double Clamp(this double me, float inclusiveFrom, double inclusiveTo)
	{
		if (me > inclusiveTo) return inclusiveTo;
		if (me < inclusiveFrom) return inclusiveFrom;
		return me;
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

	public static int RoundToInt(this double a)
	{
		return (int)a.Round();
	}
	public static int CeilToInt(this double a)
	{
		return (int)a.Ceil();
	}
	public static int FloorToInt(this double a)
	{
		return (int)a.Floor();
	}

	public static long RoundToLong(this double a)
	{
		return (long)a.Round();
	}
	public static long CeilToLong(this double a)
	{
		return (long)a.Ceil();
	}
	public static long FloorToLong(this double a)
	{
		return (long)a.Floor();
	}
	#endregion

	#region int extensions
	public static int Abs(this int val)
	{
		if (val >= 0) return val;
		return -val;
	}
	public static int Max(this int me, int other)
	{
		if (me > other) return me;
		return other;
	}
	public static int Min(this int me, int other)
	{
		if (me < other) return me;
		return other;
	}
	public static int Clamp(this int me, int inclusiveFrom, int inclusiveTo)
	{
		if (me > inclusiveTo) return inclusiveTo;
		if (me < inclusiveFrom) return inclusiveFrom;
		return me;
	}
	#endregion

	#region string extensions

	static readonly Regex oneOrMoreWhiteSpaces = new Regex(@"^\s+$", RegexOptions.Compiled);

	/// <summary>
	/// Counts the amount of occurence of <paramref name="needle"/> in <paramref name="haystack"/>;
	/// </summary>
	/// <param name="haystack"></param>
	/// <param name="needle"></param>
	/// <returns></returns>
	public static int Count(this string haystack, char needle)
	{
		int count = 0;
		for (int i = 0; i < haystack.Length; i++)
			if (haystack[i] == needle)
				count++;
		return count;
	}
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
	public static bool IsWhiteSpace(this string s)
	{
		return oneOrMoreWhiteSpaces.IsMatch(s);
	}
	public static bool IsNullOrWhiteSpace(this string s)
	{
		return s == null || IsWhiteSpace(s);
	}
	public static bool IsNullOrEmptyOrWhiteSpace(this string s)
	{
		return s == null || s.Length == 0 || IsWhiteSpace(s);
	}

	public static string RemoveWhiteSpaces(this string str)
	{
		return oneOrMoreWhiteSpaces.Replace(str, string.Empty);
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
		if (text == null) return 0;
		if (text == string.Empty) return 1;
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



	/// <summary>
	/// Number of changes needed to turn one string into another.
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
	#endregion

	#region char extensions
	public static bool IsWhiteSpace(this char c)
	{
		return oneOrMoreWhiteSpaces.IsMatch(c.ToString());
	}
	#endregion

	#region Type extensions
	/// <summary>
	/// Returns first custom attribute from type. Returns null if not found.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="type"></param>
	/// <param name="inherit"></param>
	/// <returns></returns>
	public static T GetCustomAttribute<T>(this Type type, bool inherit) where T : class
	{
		var attributes = type.GetCustomAttributes(typeof(T), inherit);
		if (attributes.Length > 0)
		{
			return attributes[0] as T;
		}
		return null;
	}

	// from http://stackoverflow.com/questions/457676/check-if-a-class-is-derived-from-a-generic-class
	public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
	{
		while (toCheck != null && toCheck != typeof(object))
		{
			var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
			if (generic == cur)
			{
				return true;
			}
			toCheck = toCheck.BaseType;
		}
		return false;
	}

	// from http://stackoverflow.com/questions/325426/programmatic-equivalent-of-defaulttype
	/// <summary>
	/// Returns default boxed value for value type System.Type , returns null for non-value types.
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public static object GetDefault(this Type t)
	{
		Func<object> f = GetDefault_Helper<object>;
		return f.Method.GetGenericMethodDefinition().MakeGenericMethod(t).Invoke(null, null);
	}

	private static T GetDefault_Helper<T>()
	{
		return default(T);
	}

	public static bool TryConvertFromString(this Type type, string input, out object obj)
	{
		try
		{
			var converter = TypeDescriptor.GetConverter(type);
			if (converter != null)
			{
				obj = converter.ConvertFromInvariantString(input);
				return true;
			}
		}
		catch
		{
		}
		obj = null;
		return false;
	}
	#endregion

	#region Random extensions
	public static float Next(this Random random, float minValue, float maxValue)
	{
		return minValue + (float)(random.NextDouble() * (maxValue - minValue));
	}

	public static double Next(this Random random, double minValue, double maxValue)
	{
		return minValue + (double)(random.NextDouble() * (maxValue - minValue));
	}
	#endregion

	#region Array[,] extensions
	public static IEnumerable<T> ToEnumerable<T>(this T[,] target)
	{
		foreach (T item in target)
			yield return item;
	}
	#endregion

	#region DateTime extensions
	public struct DateTimeHandler
	{
		DateTime dateTime;
		public DateTimeHandler(DateTime dateTime)
		{
			this.dateTime = dateTime;
		}
		public bool InPastComparedTo(DateTime other)
		{
			return dateTime < other;
		}
		public bool InFutureComparedTo(DateTime other)
		{
			return dateTime > other;
		}
	}
	public static DateTimeHandler IsOver(this DateTime dateTime, int days = 0, int hours = 0, int minutes = 0, int seconds = 0, int milliseconds = 0)
	{
		return new DateTimeHandler(dateTime + new TimeSpan(days, hours, minutes, seconds, milliseconds));
	}
	public static DateTimeHandler IsUnder(this DateTime dateTime, int days = 0, int hours = 0, int minutes = 0, int seconds = 0, int milliseconds = 0)
	{
		return new DateTimeHandler(dateTime - new TimeSpan(days, hours, minutes, seconds, milliseconds));
	}
	public static bool IsInPastComparedTo(this DateTime dateTime, DateTime other)
	{
		return dateTime < other;
	}
	public static bool IsInFutureComparedTo(this DateTime dateTime, DateTime other)
	{
		return dateTime > other;
	}
	#endregion

	#region byte[] extensions
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
	}
	#endregion
}

