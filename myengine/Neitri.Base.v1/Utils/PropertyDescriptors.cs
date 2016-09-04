using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Neitri
{

	/// <summary>
	/// Generic interface to get or set values using reflection into both fields and properties
	/// </summary>
	public interface IPropertyDescriptor
	{
		string Name { get; }
		bool CanRead { get; }
		bool CanWrite { get; }
		Type DeclaringType { get; }
		Type Type { get; }
		bool IsDefined(Type attributeType, bool inherit = true);
		object[] GetCustomAttributes(Type attributeType, bool inherit = true);
		T[] GetCustomAttributes<T>(bool inherit = true) where T : Attribute;
		object Read(object target);
		void Write(object target, object value);
		/// <summary>
		/// Get the target.GetType(), if target is null returns IPropertyDescriptor.Type
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		Type GetRealType(object target);

	}
	public static class PropertyDescriptorUtils
	{
		public static T GetCustomAttribute<T>(this IPropertyDescriptor me, bool inherit = true) where T : Attribute
		{
			var attributes = me.GetCustomAttributes(typeof(T), inherit);
			if (attributes == null || attributes.Length == 0) return null;
			return (T)attributes[0];
		}
		public static bool IsDefined<T>(this IPropertyDescriptor me, bool inherit = true) where T : Attribute
		{
			return me.IsDefined(typeof(T));
		}

		public static IPropertyDescriptor GetOne(
			Type type,
			string name,
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.IgnoreCase
		)
		{
			IPropertyDescriptor ret = null;
			var nameParts = name.Split('.'); // for example DateTime time, we can access time.TotalSeconds so we first find time, then we find TotalSeconds
			foreach (var namePart in nameParts)
			{
				if (ret == null)
				{
					ret = GetOnePart(type, namePart, bindingFlags);
					if (ret == null) return null;
				}
				else
				{
					ret = new SequenceWrapper(ret, GetOnePart(ret.Type, namePart, bindingFlags));
				}
			}
			return ret;
		}

		private static IPropertyDescriptor GetOnePart(
			Type type,
			string name,
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.IgnoreCase
		)
		{
			if (type == null) return null;
			var f = type.GetField(name, bindingFlags);
			if (f != null)
			{
				return new FieldDescriptor(f);
			}
			else
			{
				var p = type.GetProperty(name, bindingFlags);
				if (p != null)
				{
					return new PropertyDescriptor(p);
				}
			}
			return null;
		}

		public static IEnumerable<IPropertyDescriptor> GetAll(
			Type type,
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.IgnoreCase
		)
		{
			if (type == null) return null;
			return
				type.GetFields(bindingFlags).Select(f => (new FieldDescriptor(f)) as IPropertyDescriptor)
				.Concat(
					type.GetProperties(bindingFlags).Select(p => (new PropertyDescriptor(p)) as IPropertyDescriptor)
				);
		}


		#region IPropertyDescriptor implementations for field, property and sequence
		class FieldDescriptor : IPropertyDescriptor
		{
			public string Name { get { return fieldInfo.Name; } }
			public bool CanRead { get { return true; } }
			public bool CanWrite { get { return true; } }
			public Type Type { get { return fieldInfo.FieldType; } }
			public Type DeclaringType { get { return fieldInfo.DeclaringType; } }
			readonly FieldInfo fieldInfo;
			public bool IsDefined(Type attributeType, bool inherit = true)
			{
				return fieldInfo.IsDefined(attributeType, inherit);
			}
			public T[] GetCustomAttributes<T>(bool inherit = true) where T : Attribute
			{
				return fieldInfo.GetCustomAttributes(typeof(T), inherit).Cast<T>().ToArray();
			}
			public object[] GetCustomAttributes(Type attributeType, bool inherit = true)
			{
				return fieldInfo.GetCustomAttributes(attributeType, inherit);
			}
			public static implicit operator FieldDescriptor(FieldInfo fieldInfo)
			{
				return new FieldDescriptor(fieldInfo);
			}
			public FieldDescriptor(FieldInfo fieldInfo)
			{
				this.fieldInfo = fieldInfo;
			}
			public object Read(object target)
			{
				if (!CanRead) return null;
				if (target == null && fieldInfo.IsStatic == false) return null;
				return fieldInfo.GetValue(target);
			}
			public void Write(object target, object value)
			{
				if (!CanWrite) return;
				if (target == null && fieldInfo.IsStatic == false) return;
				fieldInfo.SetValue(target, value);
			}
			public Type GetRealType(object target)
			{
				if (target == null) return Type;
				return target.GetType();
			}
			public override string ToString()
			{
				return Type.FullName + " " + DeclaringType.FullName + "." + Name;
			}
		}

		class PropertyDescriptor : IPropertyDescriptor
		{
			public string Name { get { return propertyInfo.Name; } }
			public bool CanRead { get { return propertyInfo.GetGetMethod(true) != null && propertyInfo.GetIndexParameters().Length == 0; } } // we can not read indexed properties
			public bool CanWrite { get { return propertyInfo.GetSetMethod(true) != null && propertyInfo.GetIndexParameters().Length == 0; } } // we can not write indexed properties
			public Type Type { get { return propertyInfo.PropertyType; } }
			public Type DeclaringType { get { return propertyInfo.DeclaringType; } }
			readonly PropertyInfo propertyInfo;
			public bool IsDefined(Type attributeType, bool inherit = true)
			{
				return propertyInfo.IsDefined(attributeType, inherit);
			}
			public T[] GetCustomAttributes<T>(bool inherit = true) where T : Attribute
			{
				return propertyInfo.GetCustomAttributes(typeof(T), inherit).Cast<T>().ToArray();
			}
			public object[] GetCustomAttributes(Type attributeType, bool inherit = true)
			{
				return propertyInfo.GetCustomAttributes(attributeType, inherit);
			}
			public static implicit operator PropertyDescriptor(PropertyInfo propertyInfo)
			{
				return new PropertyDescriptor(propertyInfo);
			}
			public PropertyDescriptor(PropertyInfo propertyInfo)
			{
				this.propertyInfo = propertyInfo;
			}
			public object Read(object target)
			{
				if (CanRead == false) return null;
				if (target == null && propertyInfo.GetGetMethod().IsStatic == false) return null;
				return propertyInfo.GetValue(target, null);
			}
			public void Write(object target, object value)
			{
				if (CanWrite == false) return;
				if (target == null && propertyInfo.GetSetMethod().IsStatic == false) return;
				propertyInfo.SetValue(target, value, null);
			}
			public Type GetRealType(object target)
			{
				if (target == null) return Type;
				return target.GetType();
			}
			public override string ToString()
			{
				return Type.FullName + " " + DeclaringType.FullName + "." + Name;
			}
		}

		class SequenceWrapper : IPropertyDescriptor
		{
			public string Name { get { return holder.Name + "." + property.Name; } }
			public bool CanRead { get { return holder.CanRead && property.CanRead; } } // we can not read indexed properties
			public bool CanWrite { get { return holder.CanRead && property.CanWrite; } } // we can not write indexed properties
			public Type Type { get { return property.Type; } }
			public Type DeclaringType { get { return holder.DeclaringType; } }
			readonly IPropertyDescriptor holder;
			readonly IPropertyDescriptor property;
			public SequenceWrapper(IPropertyDescriptor holder, IPropertyDescriptor property)
			{
				this.holder = holder;
				this.property = property;
			}
			public bool IsDefined(Type attributeType, bool inherit = true)
			{
				return holder.IsDefined(attributeType, inherit);
			}
			public T[] GetCustomAttributes<T>(bool inherit = true) where T : Attribute
			{
				return holder.GetCustomAttributes<T>(inherit);
			}
			public object[] GetCustomAttributes(Type attributeType, bool inherit = true)
			{
				return holder.GetCustomAttributes(attributeType, inherit);
			}
			public object Read(object target)
			{
				return property.Read(holder.Read(target));
			}
			public void Write(object target, object value)
			{
				property.Write(holder.Read(target), value);
			}
			public Type GetRealType(object target)
			{
				return property.GetRealType(target);
			}
			public override string ToString()
			{
				return property.Type.FullName + " " + holder.DeclaringType.FullName + "." + Name;
			}
		}

		#endregion


	}

}