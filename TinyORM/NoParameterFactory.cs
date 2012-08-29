using System.Collections.Generic;
using System.Data;

namespace TinyORM
{
	internal class NoParameterFactory : IParameterFactory
	{
		private readonly string _operator;

		public NoParameterFactory(string @operator)
		{
			_operator = @operator;
		}

		public string GetSql(string columnName)
		{
			return string.Format("{0} {1}", columnName, _operator);
		}

		public IEnumerable<IDataParameter> GetParameters(IColumnMap columnMap)
		{
			yield break;
		}

		public string UpdateSql(string sql, string columnName)
		{
			return sql;
		}
	}
}