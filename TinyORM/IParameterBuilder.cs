using System.Collections.Generic;
using System.Data;

namespace TinyORM
{
	public interface IParameterBuilder
	{
		IEnumerable<IDataParameter> GetParameters();
		string GetSql();
		string UpdateSql(string sql);
	}
}