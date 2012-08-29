using System.Collections.Generic;
using System.Data;

namespace TinyORM
{
	internal class ExecuteCommand : IExecuteCommand
	{
		private readonly string _sql;
		private readonly IEnumerable<IDataParameter> _parameters;

		public ExecuteCommand(string sql, IEnumerable<IDataParameter> parameters)
		{
			_sql = sql;
			_parameters = parameters;
		}

		public int Execute(IDbConnection connection)
		{
			var dbCommand = connection.CreateCommand();
			dbCommand.CommandText = _sql;
			foreach (var parameter in _parameters)
				dbCommand.Parameters.Add(parameter);
			return dbCommand.ExecuteNonQuery();
		}

		public object ExecuteScalar(IDbConnection connection)
		{
			var dbCommand = connection.CreateCommand();
			dbCommand.CommandText = _sql;
			foreach (var parameter in _parameters)
				dbCommand.Parameters.Add(parameter);
			return dbCommand.ExecuteScalar();
		}
	}
}