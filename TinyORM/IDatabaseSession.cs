using System.Collections.Generic;

namespace TinyORM
{
	public interface IDatabaseSession
	{
		//int Execute(string sql);
		IEnumerable<dynamic> Query(string sql);
		IEnumerable<T> Query<T>(string sql);
		IEnumerable<DataEntityTuple<T>> QueryData<T>(string sql);
	}
}