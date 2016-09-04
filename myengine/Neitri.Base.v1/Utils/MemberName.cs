using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Neitri
{
	// from http://joelabrahamsson.com/getting-property-and-method-names-using-static-reflection-in-c/
	public static class MemberName
	{
		public static string For<T>(Expression<Func<T, object>> expression)
		{
			if (expression == null)
			{
				throw new ArgumentException(
					"The expression cannot be null.");
			}

			return For(expression.Body);
		}
		public static string For<T>(Expression<Func<T>> expression)
		{
			if (expression == null)
			{
				throw new ArgumentException(
					"The expression cannot be null.");
			}

			return For(expression.Body);
		}
		public static string GetMemberName<T>(this T instance, Expression<Action<T>> expression)
		{
			return For(expression);
		}

		public static string For<T>(Expression<Action<T>> expression)
		{
			if (expression == null)
			{
				throw new ArgumentException(
					"The expression cannot be null.");
			}

			return For(expression.Body);
		}
		public static string For(Expression<Action> expression)
		{
			if (expression == null)
			{
				throw new ArgumentException(
					"The expression cannot be null.");
			}

			return For(expression.Body);
		}


		private static string For(Expression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException(
					"The expression cannot be null.");
			}

			if (expression is MemberExpression)
			{
				// Reference type property or field
				var memberExpression =
					(MemberExpression)expression;
				return memberExpression.Member.Name;
			}

			if (expression is MethodCallExpression)
			{
				// Reference type method
				var methodCallExpression =
					(MethodCallExpression)expression;
				return methodCallExpression.Method.Name;
			}

			if (expression is UnaryExpression)
			{
				// Property, field of method returning value type
				var unaryExpression = (UnaryExpression)expression;
				return For(unaryExpression);
			}

			throw new ArgumentException("Invalid expression");
		}

		private static string For(UnaryExpression unaryExpression)
		{
			if (unaryExpression.Operand is MethodCallExpression)
			{
				var methodExpression =
					(MethodCallExpression)unaryExpression.Operand;
				return methodExpression.Method.Name;
			}

			return ((MemberExpression)unaryExpression.Operand)
				.Member.Name;
		}
	}
}