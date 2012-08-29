using System.Collections.Generic;
using System.Data;

namespace TinyORM
{
	internal class ParameterBuilder : IParameterBuilder
	{
		private readonly IColumnMap _columnMap;
		private readonly IParameterFactory _parameterFactory;

		public ParameterBuilder(IColumnMap columnMap, IParameterFactory parameterFactory)
		{
			_columnMap = columnMap;
			_parameterFactory = parameterFactory;
		}

		public IEnumerable<IDataParameter> GetParameters()
		{
			return _parameterFactory.GetParameters(_columnMap);
		}

		public string GetSql()
		{
			return _parameterFactory.GetSql(_columnMap.ColumnName);
		}

		public string UpdateSql(string sql)
		{
			return _parameterFactory.UpdateSql(sql, _columnMap.ColumnName);
		}
	}
}