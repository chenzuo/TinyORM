using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace TinyORM
{
	internal abstract class ParameterFactory : IParameterFactory
	{
		protected readonly object Value;
		private readonly string _parameterName;

		protected ParameterFactory(object value, string parameterName = null)
		{
			Value = value;
			_parameterName = parameterName;
		}

		public abstract IEnumerable<IDataParameter> GetParameters(IColumnMap columnMap);
		public abstract string GetSql(string columnName);
		public abstract string UpdateSql(string sql, string columnName);

		protected SqlParameter CreateParameter(IColumnMap columnMap, object value)
		{
			if (columnMap.Serializer != null)
				value = columnMap.Serializer.Serialize(value);

			return new SqlParameter(GetParameterName(columnMap.ColumnName), value)
				{
					IsNullable = columnMap.IsNullable,
					Precision = columnMap.ColumnPrecision,
					Scale = columnMap.ColumnScale,
					Size = columnMap.ColumnMaxLength
				};
		}

		protected string GetParameterName(string columnName)
		{
			return "@" + (_parameterName ?? columnName);
		}
	}
}