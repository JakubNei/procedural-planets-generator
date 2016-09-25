using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MyEngine
{
	public static class Require
	{
		/// <summary>
		/// Require.NotNull(() => scene);
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="expression"></param>
		public static void NotNull<T>(Expression<Func<T>> expression) where T : class
		{
			if (expression.Compile()() == null)
			{
				throw new NullReferenceException(MemberName.For(expression));
			}
		}

		public static void NotNull<T, T2>(Expression<Func<T>> expression, Expression<Func<T2>> expression2) where T : class where T2 : class
		{
			NotNull(expression);
			NotNull(expression2);
		}

		public static void NotNull<T, T2, T3>(Expression<Func<T>> expression, Expression<Func<T2>> expression2, Expression<Func<T3>> expression3) where T : class where T2 : class where T3 : class
		{
			NotNull(expression);
			NotNull(expression2);
			NotNull(expression3);
		}

		public static void True(Expression<Func<bool>> expression)
		{
			if (expression.Compile()() == false)
			{
				throw new IsNotTrueException(expression.Body.ToString());
			}
		}
	}

	public class IsNotTrueException : Exception
	{
		public IsNotTrueException(string message) : base(message)
		{
		}
	}
}