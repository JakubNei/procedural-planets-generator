using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace MyEngine
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
        object[] GetCustomAttributes(Type attributeType, bool inherit = true);
        T[] GetCustomAttributes<T>(bool inherit = true) where T : Attribute;
        object Read(object target);
        void Write(object target, object value);
        /// <summary>
        /// Get the target.GetType(), if target it null returns IPropertyDescriptor.Type
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        Type GetType(object target);

    }
    public static class PropertyDescriptorUtils
    {
        class FieldDescriptor : IPropertyDescriptor
        {
            readonly FieldInfo fieldInfo;
            public string Name { get { return fieldInfo.Name; } }
            public bool CanRead { get { return true; } }
            public bool CanWrite { get { return true; } }
            public Type Type { get { return fieldInfo.FieldType; } }
            public Type DeclaringType { get { return fieldInfo.DeclaringType; } }

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
            public Type GetType(object target)
            {
                if (target == null) return Type;
                return target.GetType();
            }
            public override string ToString()
            {
                return fieldInfo.FieldType + " " + fieldInfo.DeclaringType.FullName + "." + fieldInfo.Name;
            }
        }


        class PropertyDescriptor : IPropertyDescriptor
        {
            readonly PropertyInfo propertyInfo;
            public string Name { get { return propertyInfo.Name; } }
            public bool CanRead { get { return propertyInfo.GetGetMethod(true) != null && propertyInfo.GetIndexParameters().Length == 0; } } // we can not read indexed properties
            public bool CanWrite { get { return propertyInfo.GetSetMethod(true) != null && propertyInfo.GetIndexParameters().Length == 0; } } // we can not write indexed properties
            public Type Type { get { return propertyInfo.PropertyType; } }
            public Type DeclaringType { get { return propertyInfo.DeclaringType; } }

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
            public Type GetType(object target)
            {
                if (target == null) return Type;
                return target.GetType();
            }
            public override string ToString()
            {
                return propertyInfo.PropertyType + " " + propertyInfo.DeclaringType.FullName + "." + propertyInfo.Name;
            }
        }



        public static IPropertyDescriptor GetOne(
            Type type,
            string name,
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty
        )
        {
            if (type == null) throw new NullReferenceException("type is null");
            var f = type.GetField(name, bindingFlags);
            if (f != null)
            {
                return (new FieldDescriptor(f)) as IPropertyDescriptor;
            }
            else
            {
                var p = type.GetProperty(name, bindingFlags);
                if (p != null)
                {
                    return (new PropertyDescriptor(p)) as IPropertyDescriptor;
                }
            }
            throw new NullReferenceException(type + " no field or property of name:" + name + " found");
            return null;
        }

        public static IEnumerable<IPropertyDescriptor> GetAll(
            Type type,
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty
        )
        {
            if (type == null) throw new NullReferenceException("type is null");
            return
                type.GetFields(bindingFlags).Select(f => (new FieldDescriptor(f)) as IPropertyDescriptor)
                .Concat(
                    type.GetProperties(bindingFlags).Select(p => (new PropertyDescriptor(p)) as IPropertyDescriptor)
                );
        }
    }

}