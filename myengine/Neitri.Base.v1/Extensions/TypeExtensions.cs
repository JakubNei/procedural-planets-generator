using System.Collections;
using System;
using System.Text;
using System.ComponentModel;


namespace Neitri
{
    static class TypeExtensions
    {
        /// <summary>
        /// Same as keyword is, but works on System.Type
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool Is(this Type left, Type right)
        {
            return left == right || left.IsAssignableFrom(right) || right.IsAssignableFrom(left);
        }

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
            if(attributes.Length > 0)
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
            Func<object> f = GetDefault<object>;
            return f.Method.GetGenericMethodDefinition().MakeGenericMethod(t).Invoke(null, null);
        }
        static T GetDefault<T>()
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
    }

}