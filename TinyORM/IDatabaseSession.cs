using System.Collections.Generic;

namespace TinyORM
{
	public interface IDatabaseSession
	{
		IEnumerable<dynamic> Query(string sql, object @params = null);
		IEnumerable<T> Query<T>(string sql, object @params = null);
		IEnumerable<T> Query<T>(object @params = null);

		int Execute(string sql, object @params = null);
		int Execute<TMap>(string sql, object @params = null);

		int Save<TMap>(object @params);
		int Insert<TMap>(object @params);
		int Update<TMap>(object @params);
		int Delete<TMap>(object @params);

		TResult ExecuteScalar<TResult>(string sql, object @params = null, IValueSerializer serializer = null);
		TResult ExecuteScalar<TMap, TResult>(string sql, object @params = null, IValueSerializer serializer = null);
	}
}