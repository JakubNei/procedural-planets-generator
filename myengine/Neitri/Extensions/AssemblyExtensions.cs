using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Neitri
{
	public static class AssemblyExtensions
	{
		/*

        public static IEnumerable<Type> GetClassesOf(this Assembly assembly, Type targetType, Predicate<Type> typeFilter)
        {
            var result = new List<Type>();
            foreach (Type type in assembly.GetLoadableTypes())
            {
                if (typeFilter(type) == false) continue;
                if (type.IsSubclassOf(targetType))
                {
                    result.Add(type);
                }
            }
            return result;
        }*/

		// http://haacked.com/archive/2012/07/23/get-all-types-in-an-assembly.aspx/
		public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
		{
			if (assembly == null) throw new ArgumentNullException("assembly");
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				return e.Types.Where(t => t != null);
			}
		}

		/// <summary>
		/// Returns pair IEnumerable of System.Type and ATTRIBUTE_TYPE Instance you are looking for, returns only the first attribute (there might be more on the same type or the type's ancestors)
		/// if(targetAssemblies == null) targetAssemblies = AppDomain.CurrentDomain.GetAssemblies().
		/// Does not search inside dynamic AssemblyBuilder assemblies.
		/// Do not change into Dictionary for performance reasons
		/// </summary>
		/// <typeparam name="ATTRIBUTE_TYPE"></typeparam>
		/// <returns></returns>
		public static IEnumerable<Tuple<Type, ATTRIBUTE_TYPE>> GetTypeAttribPairs<ATTRIBUTE_TYPE>(this Assembly assembly)
			where ATTRIBUTE_TYPE : Attribute
		{
			var result = new HashSet<Tuple<Type, ATTRIBUTE_TYPE>>();
			foreach (Type type in GetLoadableTypes(assembly))
			{
				var attribs = type.GetCustomAttributes(typeof(ATTRIBUTE_TYPE), false);
				if (attribs != null && attribs.Length > 0)
				{
					ATTRIBUTE_TYPE attrib = (ATTRIBUTE_TYPE)attribs[0];
					if (attrib != null)
					{
						result.Add(new Tuple<Type, ATTRIBUTE_TYPE>(type, attrib));
					}
				}
			}
			return result;
		}
	}
}