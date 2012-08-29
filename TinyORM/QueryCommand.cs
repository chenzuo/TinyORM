using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace TinyORM
{
	internal class QueryCommand : IQueryCommand
	{
		private readonly string _sql;
		private readonly IEnumerable<IDataParameter> _parameters;
		private readonly IInstanceMapperProvider _instanceMapperProvider;

		public QueryCommand(string sql, IEnumerable<IDataParameter> parameters, IInstanceMapperProvider instanceMapperProvider)
		{
			_sql = sql;
			_parameters = parameters;
			_instanceMapperProvider = instanceMapperProvider;
		}

		public IEnumerable<T> Query<T>(IDbConnection connection)
		{
			var dbCommand = connection.CreateCommand();

			dbCommand.CommandText = _sql;
			foreach (var parameter in _parameters)
				dbCommand.Parameters.Add(parameter);

			var reader = dbCommand.ExecuteReader();
			var schemaTable = reader.GetSchemaTable();
			var mapper = GetMapper<T>(schemaTable);

			while (reader.Read())
				yield return (T)mapper.Map(reader, typeof(T));
		}

		private IInstanceMapper GetMapper<T>(DataTable schemaTable)
		{
			var anonymousColumnCounter = 0;
			var columns = from row in schemaTable.Rows.Cast<DataRow>()
							  let index = (int)row["ColumnOrdinal"]
							  let name = ((string)row["ColumnName"] ?? "NoName" + anonymousColumnCounter++).ToLower()
							  orderby index
							  select new DataColumnInfo
								  {
									  Index = index,
									  Name = name
								  };

			return _instanceMapperProvider.GetMapper(typeof(T), columns);
		}
	}
}