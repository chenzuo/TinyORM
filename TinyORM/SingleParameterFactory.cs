using System.Collections.Generic;
using System.Data;

namespace TinyORM
{
	internal class SingleParameterFactory : ParameterFactory
	{
		private readonly string _operator;

		public SingleParameterFactory(string @operator, object value, string parameterName = null)
			: base(value, parameterName)
		{
			_operator = @operator;
		}

		public override IEnumerable<IDataParameter> GetParameters(IColumnMap columnMap)
		{
			yield return CreateParameter(columnMap, Value);
		}

		public override string GetSql(string columnName)
		{
			return string.Format("{0} {1} {2}", columnName, _operator, GetParameterName(columnName));
		}

		public override string UpdateSql(string sql, string columnName)
		{
			return sql;
		}
	}
}