using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Neitri.DependencyInjection
{
	public class DependencyManager : IDependencyManager
	{
		HashSet<object> instances = new HashSet<object>();
		HashSet<Type> registeredTypes = new HashSet<Type>();

		IDependencyManager parent;

		public DependencyManager()
		{
			Register(this);
		}

		public DependencyManager(IDependencyManager parent) : this()
		{
			this.parent = parent;
		}

		public IDependencyManager Register(object instance)
		{
			if (instance == null) throw new NullReferenceException("tried to Register null instance");
			instances.Add(instance);
			return this;
		}

		public IDependencyManager RegisterType(Type type)
		{
			registeredTypes.Add(type);
			return this;
		}

		public object Resolve(Type type)
		{
			object instance = GetInstance(type);
			if (instance != null) return instance;
			if (CanCreateFor(type)) return CreateAndRegister(GetTypeFor(type));
			if (parent == null) throw new NullReferenceException("unable to resolve " + type);
			return parent.Resolve(type);
		}

		public bool CanResolve(Type type)
		{
			var instance = GetInstance(type);
			if (instance != null) return true;
			if (CanCreateFor(type)) return true;
			if (parent == null) return false;
			return parent.CanResolve(type);
		}

		public IDependencyManager BuildUp(object instance)
		{
			if (instance == null) throw new NullReferenceException("tried to BuildUp null instance");
			var members = PropertyDescriptorUtils.GetAll(
				instance.GetType(),
				BindingFlags.Instance |
				BindingFlags.Public | BindingFlags.NonPublic |
				BindingFlags.GetProperty | BindingFlags.SetProperty |
				BindingFlags.FlattenHierarchy
			).Where(m => m.CanWrite && m.CanRead && m.IsDefined<DependencyAttribute>(true))
			.OrderBy(p => p.GetCustomAttribute<DependencyAttribute>(true).Order);

			foreach (var member in members)
			{
				var dependencyAttribute = member.GetCustomAttribute<DependencyAttribute>(true);
				if (dependencyAttribute != null)
				{
					if (dependencyAttribute.Register) this.RegisterType(member.Type);
				}

				if (member.Read(instance) == null)
				{
					var resolvedMemberValue = Resolve(member.Type);
					member.Write(instance, resolvedMemberValue);
				}
			}

			if (instance is IOnDependenciesResolved)
			{
				(instance as IOnDependenciesResolved).OnDependenciesResolved();
			}

			return this;
		}

		/// <summary>
		/// Creates new instance and then resolves it's dependencies.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public object Create(Type type, params object[] args)
		{
			var instance = CoreCreate(type, args);
			BuildUp(instance);
			return instance;
		}

		public object CreateAndRegister(Type type, params object[] args)
		{
			var instance = CoreCreate(type, args);
			Register(instance);
			BuildUp(instance);
			return instance;
		}

		bool IsConstructorUsable(ConstructorInfo ctor, Type typeToCreate, object[] paramsToStartWith)
		{
			var parameters = ctor.GetParameters();
			for (int i = 0; i < parameters.Length; i++)
			{
				var param = parameters[i];
				var paramType = param.ParameterType;
				if ((paramType == typeof(string) || paramType.IsValueType) && param.HasDefaultValue) continue;
				if (paramsToStartWith != null && i < paramsToStartWith.Length)
				{
					if (paramType.IsAssignableFrom(paramsToStartWith[i].GetType()) == false) return false;
				}
				else
				{
					if (CanResolve(paramType) == false) return false;
				}
			}
			return true;
		}

		object[] BuildConstrutorArgs(ConstructorInfo ctor, Type typeToCreate, object[] paramsToStartWith)
		{
			var parameters = ctor.GetParameters();
			var args = new object[parameters.Length];
			for (int i = 0; i < args.Length; i++)
			{
				var param = parameters[i];
				if (paramsToStartWith != null && i < paramsToStartWith.Length) args[i] = paramsToStartWith[i];
				else if (!CanResolve(param.ParameterType) && param.HasDefaultValue) args[i] = param.DefaultValue;
				else args[i] = Resolve(param.ParameterType);
			}
			return args;
		}

		/// <summary>
		/// Prefers constructors with greatest amount of parameters.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		object CoreCreate(Type type, params object[] args)
		{
			if (type.IsInterface) throw new Exception("unable to create instance of interface " + type);
			if (type.IsAbstract) throw new Exception("unable to create instance of abstract class " + type);

			ConstructorInfo constructor = null;

			foreach (var c in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (IsConstructorUsable(c, type, args)) constructor = c;
			}

			if (constructor == null) throw new NullReferenceException("unable to create instance of " + type + ", unable to resolve all parameters");

			//var instance = Activator.CreateInstance(type, BuildConstrutorArgs(constructor, type, args));
			args = BuildConstrutorArgs(constructor, type, args);
			var instance = constructor.Invoke(args);
			return instance;
		}

		Type GetTypeFor(Type type)
		{
			return registeredTypes.FirstOrDefault(t => TypeImplements(t, type));
		}

		bool CanCreateFor(Type type)
		{
			type = GetTypeFor(type);
			return type != null && type.IsInterface == false && type.IsAbstract == false;
		}

		bool TypeImplements(Type concreteType, Type implementsRootType)
		{
			return concreteType == implementsRootType ||
					concreteType.IsSubclassOf(implementsRootType) ||
					concreteType.GetInterfaces().Contains(implementsRootType);
		}

		object GetInstance(Type type)
		{
			return instances.FirstOrDefault(i => TypeImplements(i.GetType(), type));
		}
	}
}