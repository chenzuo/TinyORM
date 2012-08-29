using System.Collections.Generic;
using System.Data;

namespace TinyORM
{
	public interface IParameterFactory
	{
		IEnumerable<IDataParameter> GetParameters(IColumnMap columnMap);
		string GetSql(string columnName);
		string UpdateSql(string sql, string columnName);
	}
}