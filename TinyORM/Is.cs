using System.Collections.Generic;

namespace TinyORM
{
	public static class Is
	{
		public static IParameterFactory Null
		{
			get { return new NoParameterFactory("is null"); }
		}

		public static IParameterFactory NotNull
		{
			get { return new NoParameterFactory("is not null"); }
		}

		public static IParameterFactory EqualTo(object value, string parameterName = null)
		{
			return new SingleParameterFactory("=", value, parameterName);
		}

		public static IParameterFactory NotEqualTo(object value, string parameterName = null)
		{
			return new SingleParameterFactory("!=", value, parameterName);
		}

		public static IParameterFactory LessThan(object value, string parameterName = null)
		{
			return new SingleParameterFactory("<", value, parameterName);
		}

		public static IParameterFactory LessThanOrEqualTo(object value, string parameterName = null)
		{
			return new SingleParameterFactory("<=", value, parameterName);
		}

		public static IParameterFactory GreaterThan(object value, string parameterName = null)
		{
			return new SingleParameterFactory(">", value, parameterName);
		}

		public static IParameterFactory GreaterThanOrEqualTo(object value, string parameterName = null)
		{
			return new SingleParameterFactory(">=", value, parameterName);
		}

		public static IParameterFactory Like(object value, string parameterName = null)
		{
			return new SingleParameterFactory("like", value, parameterName);
		}

		public static IParameterFactory NotLike(object value, string parameterName = null)
		{
			return new SingleParameterFactory("not like", value, parameterName);
		}

		public static IParameterFactory In(IEnumerable<object> values, string parameterName = null)
		{
			return new MultiParameterFactory("in", values, parameterName);
		}

		public static IParameterFactory NotIn(IEnumerable<object> values, string parameterName = null)
		{
			return new MultiParameterFactory("not in", values, parameterName);
		}
	}
}