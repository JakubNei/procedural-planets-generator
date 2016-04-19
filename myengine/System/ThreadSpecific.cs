using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace MyEngine
{

	/// <summary>
	/// A value of type T that has different value on each thread, uses dictionary to save value for each thread.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ThreadSpecific<T>
	{
		Dictionary<Thread, T> threadToValue = new Dictionary<Thread, T>();
		public T Value
		{
			get
			{
				T ret;
				threadToValue.TryGetValue(Thread.CurrentThread, out ret);
				return ret;
			}
			set
			{
				threadToValue[Thread.CurrentThread] = value;
			}
		}
	}
}