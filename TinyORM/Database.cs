using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
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

		public void Execute(Action<IDatabaseSession> action)
		{
			throw new NotImplementedException();
		}

		public int Execute(string sql, object parameters)
		{
			using (var connection = CreateConnection())
			{
				using (var command = connection.CreateCommand())
				{
					command.CommandText = sql;
					//new CommandBuilder(command, _configuration).AddParameters(sql, parameters);
					return command.ExecuteNonQuery();
				}
			}
		}

		public IEnumerable<dynamic> Query(string sql)
		{
			return Query<ExpandoObject>(sql);
		}

		public IEnumerable<DataEntityTuple<T>> QueryData<T>(string sql)
		{
			return Query<DataEntityTuple<T>>(sql);
		}

		public IEnumerable<T> Query<T>(string sql)
		{
			using (var connection = CreateConnection())
			{
				using (var command = connection.CreateCommand())
				{
					command.CommandText = sql;
					using (var reader = command.ExecuteReader())
					{
						var instanceMapperProvider = new InstanceMapperProvider(_configuration);
						var schema = reader.GetSchemaTable();
						var columns = GetColumns(schema);
						var mapper = instanceMapperProvider.GetMapper(typeof(T), columns);
						var list = new List<T>();

						while (reader.Read())
							list.Add((T)mapper.Map(reader, typeof(T)));

						return list;
					}
				}
			}
		}

		private static IEnumerable<DataColumnInfo> GetColumns(DataTable schema)
		{
			var anonymousColumnCounter = 0;
			return from row in schema.Rows.Cast<DataRow>()
					 let index = (int)row["ColumnOrdinal"]
					 let name = ((string)row["ColumnName"] ?? "NoName" + anonymousColumnCounter++).ToLower()
					 orderby index
					 select new DataColumnInfo
					 {
						 Index = index,
						 Name = name
					 };
		}

		private IDbConnection CreateConnection()
		{
			throw new System.NotImplementedException();
		}
	}

	#region Is

	//public static class Is
	//{
	//   public static DbParameterInfo NotEqualTo<T>(T value)
	//   {
	//      throw new NotImplementedException();
	//   }

	//   public static DbParameterInfo LessThan<T>(T value)
	//   {
	//      throw new NotImplementedException();
	//   }

	//   public static DbParameterInfo LessThanOrEqualTo<T>(T value)
	//   {
	//      throw new NotImplementedException();
	//   }

	//   public static DbParameterInfo GreaterThan<T>(T value)
	//   {
	//      throw new NotImplementedException();
	//   }

	//   public static DbParameterInfo GreaterThanOrEqualTo<T>(T value)
	//   {
	//      throw new NotImplementedException();
	//   }

	//   public static DbParameterInfo In<T>(IEnumerable<T> values)
	//   {
	//      throw new NotImplementedException();
	//   }

	//   public static DbParameterInfo NotIn<T>(T value)
	//   {
	//      throw new NotImplementedException();
	//   }

	//   public static DbParameterInfo Like(string value)
	//   {
	//      throw new NotImplementedException();
	//   }
	//}

	#endregion

	#region LINQ

	//internal class Query<T> : IQuery<T>, IOrderedQuery<T>
	//{
	//   private const string SortOrderAscending = "asc";
	//   private const string SortOrderDescending = "desc";

	//   private readonly TypeMapper _typeMapper;
	//   private readonly List<string> _sortBy;

	//   public Query(TypeMapper typeMapper)
	//   {
	//      _typeMapper = typeMapper;
	//      _sortBy = new List<string>();
	//   }

	//   public IEnumerator<T> GetEnumerator()
	//   {
	//      var builder = new StringBuilder();
	//      builder.AppendLine("select");
	//      foreach (var propertyMapper in _typeMapper.PropertyMappers)
	//      builder.AppendFormat("")
	//      throw new NotImplementedException();
	//   }

	//   IEnumerator IEnumerable.GetEnumerator()
	//   {
	//      return GetEnumerator();
	//   }

	//   public IQuery<T> Where(Expression<Func<T, bool>> expression)
	//   {
	//      throw new NotImplementedException();
	//   }

	//   public IOrderedQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
	//   {
	//      return OrderBy(keySelector, SortOrderAscending);
	//   }

	//   public IOrderedQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
	//   {
	//      return OrderBy(keySelector, SortOrderDescending);
	//   }

	//   public IOrderedQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
	//   {
	//      return OrderBy(keySelector, SortOrderAscending);
	//   }

	//   public IOrderedQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
	//   {
	//      return OrderBy(keySelector, SortOrderDescending);
	//   }

	//   private IOrderedQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector, string order)
	//   {
	//      _sortBy.Add(string.Format("{0} {1}", "TODO", order));
	//      return this;
	//   }
	//}

	//public interface IQuery<T> : IEnumerable<T>
	//{
	//   IQuery<T> Where(Expression<Func<T, bool>> expression);
	//   IOrderedQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);
	//   IOrderedQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
	//}

	//public interface IOrderedQuery<T> : IEnumerable<T>
	//{
	//   IOrderedQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector);
	//   IOrderedQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
	//}

	#endregion
}