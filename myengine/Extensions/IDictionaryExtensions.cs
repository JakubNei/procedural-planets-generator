using System;
using System.Collections.Generic;

namespace MyEngine
{
	public static class IDictionaryExtensions
	{
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
	            if (typeof(TValue).IsClass) value = System.Activator.CreateInstance<TValue>();
	            else value = default(TValue);
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


    }
}