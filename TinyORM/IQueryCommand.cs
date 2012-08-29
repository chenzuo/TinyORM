using System.Collections.Generic;
using System.Data;

namespace TinyORM
{
	internal interface IQueryCommand
	{
		IEnumerable<T> Query<T>(IDbConnection connection);
	}
}