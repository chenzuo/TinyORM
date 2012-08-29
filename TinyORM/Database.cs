using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyORM
{
	public class Database : IDatabase
	{
		private static IDatabase _instance;
		private readonly DatabaseConfiguration _configuration = new DatabaseConfiguration();

		public static IDatabase Instance
		{
			get { return _instance ?? (_instance = new Database()); }
		}

		public void Configure(Action<DatabaseConfigurationBuilder> action)
		{
			var expression = new DatabaseConfigurationBuilder(_configuration);
			action(expression);
		}

		public void ValidateConfiguration()
		{
			throw new NotImplementedException();
		}

		public void Execute(Action<IDatabaseSession> action)
		{
			using (var session = new DatabaseSession(_configuration))
				action(session);
		}

		public int Execute(string sql, object @params = null)
		{
			using (var session = new DatabaseSession(_configuration))
				return session.Execute(sql, @params);
		}

		public IEnumerable<dynamic> Query(string sql, object @params = null)
		{
			using (var session = new DatabaseSession(_configuration))
				return session.Query(sql, @params).ToList();
		}

		public IEnumerable<T> Query<T>(string sql, object @params = null)
		{
			using (var session = new DatabaseSession(_configuration))
				return session.Query<T>(sql, @params).ToList();
		}

		public IEnumerable<T> Query<T>(object @params = null)
		{
			using (var session = new DatabaseSession(_configuration))
				return session.Query<T>(@params).ToList();
		}

		public int Execute<TMap>(string sql, object @params = null)
		{
			using (var session = new DatabaseSession(_configuration))
				return session.Execute<TMap>(sql, @params);
		}

		public int Save<TMap>(object @params)
		{
			using (var session = new DatabaseSession(_configuration))
				return session.Save<TMap>(@params);
		}

		public int Insert<TMap>(object @params)
		{
			using (var session = new DatabaseSession(_configuration))
				return session.Insert<TMap>(@params);
		}

		public int Update<TMap>(object @params)
		{
			using (var session = new DatabaseSession(_configuration))
				return session.Update<TMap>(@params);
		}

		public int Delete<TMap>(object @params)
		{
			using (var session = new DatabaseSession(_configuration))
				return session.Delete<TMap>(@params);
		}

		public TResult ExecuteScalar<TResult>(string sql, object @params = null, IValueSerializer serializer = null)
		{
			using (var session = new DatabaseSession(_configuration))
				return session.ExecuteScalar<TResult>(sql, @params, serializer);
		}

		public TResult ExecuteScalar<TMap, TResult>(string sql, object @params = null, IValueSerializer serializer = null)
		{
			using (var session = new DatabaseSession(_configuration))
				return session.ExecuteScalar<TMap, TResult>(sql, @params, serializer);
		}
	}
}