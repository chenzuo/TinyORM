using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;

namespace TinyORM
{
	internal class DatabaseSession : IDatabaseSession, IDisposable
	{
		private readonly DatabaseConfiguration _configuration;
		private IDbConnection _connection;

		public DatabaseSession(DatabaseConfiguration configuration)
		{
			_configuration = configuration;
		}

		private IDbConnection GetConnection()
		{
			if (_connection == null)
			{
				var connectionString = _configuration.ConnectionStringProvider.GetConnectionString();
				var connection = new SqlConnection(connectionString);
				connection.Open();
				_connection = connection;
			}

			return _connection;
		}

		protected ICommandFactory Factory
		{
			get { return new CommandFactory(_configuration); }
		}

		private SaveGrouping GroupBySaveOperation(IEnumerable<object> parameterSources)
		{
			var skip = new List<object>();
			var insert = new List<object>();
			var update = new List<object>();
			var delete = new List<object>();

			foreach (var source in parameterSources)
			{
				var sourceType = source.GetType();
				var saveOperationProvider = _configuration.SaveOperationProviders.FirstOrDefault(x => x.CanHandle(sourceType));

				if (saveOperationProvider == null)
					throw new InvalidOperationException();

				var saveOperation = saveOperationProvider.GetSaveOperation(source);
				switch (saveOperation)
				{
					case SaveOperation.Skip:
						skip.Add(source);
						break;
					case SaveOperation.Insert:
						insert.Add(source);
						break;
					case SaveOperation.Update:
						update.Add(source);
						break;
					case SaveOperation.Delete:
						delete.Add(source);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			return new SaveGrouping
				{
					Skip = skip,
					Insert = insert,
					Update = update,
					Delete = delete
				};
		}

		private class SaveGrouping
		{
			public IEnumerable<object> Skip { get; set; }
			public IEnumerable<object> Insert { get; set; }
			public IEnumerable<object> Update { get; set; }
			public IEnumerable<object> Delete { get; set; }
		}

		public IEnumerable<dynamic> Query(string sql, object @params = null)
		{
			var parameterSource = GetParameterSource(@params);
			var command = Factory.CreateSelectCommand(@parameterSource, sql);
			return Query<ExpandoObject>(command);
		}

		public IEnumerable<T> Query<T>(string sql, object @params = null)
		{
			var parameterSource = GetParameterSource(@params);
			var command = Factory.CreateSelectCommand(@parameterSource, sql, typeof(T));
			return Query<T>(command);
		}

		public IEnumerable<DataEntityTuple<T>> QueryData<T>(string sql, object @params = null)
		{
			var parameterSource = GetParameterSource(@params);
			var command = Factory.CreateSelectCommand(@parameterSource, sql, typeof(T));
			return Query<DataEntityTuple<T>>(command);
		}

		public IEnumerable<T> Query<T>(object @params = null)
		{
			var parameterSource = GetParameterSource(@params);
			var command = Factory.CreateSelectCommand(@parameterSource, mapType: typeof(T));
			return Query<T>(command);
		}

		private IEnumerable<T> Query<T>(IQueryCommand command)
		{
			var connection = GetConnection();
			return command.Query<T>(connection);
		}

		public int Execute(string sql, object @params = null)
		{
			var parameterSources = GetParameterSources(@params);
			var commands = Factory.CreateExecuteCommands(sql, parameterSources);
			return Execute(commands);
		}

		public int Execute<TMap>(string sql, object @params = null)
		{
			var parameterSources = GetParameterSources(@params);
			var commands = Factory.CreateExecuteCommands(sql, parameterSources, typeof(TMap));
			return Execute(commands);
		}

		public int Save<T>(object @params)
		{
			var parameterSources = GetParameterSources(@params);
			var grouping = GroupBySaveOperation(parameterSources);
			var insertCommands = Factory.CreateInsertCommands(grouping.Insert, typeof(T));
			var updateCommands = Factory.CreateUpdateCommands(grouping.Update, typeof(T));
			var deleteCommands = Factory.CreateDeleteCommands(grouping.Delete, typeof(T));
			var allCommands = deleteCommands.Concat(insertCommands).Concat(updateCommands);
			return Execute(allCommands);
		}

		public int Insert<T>(object @params)
		{
			var parameterSources = GetParameterSources(@params);
			var commands = Factory.CreateInsertCommands(parameterSources, typeof(T));
			return Execute(commands);
		}

		public int Update<T>(object @params)
		{
			var parameterSources = GetParameterSources(@params);
			var commands = Factory.CreateUpdateCommands(parameterSources, typeof(T));
			return Execute(commands);
		}

		public int Delete<T>(object @params)
		{
			var parameterSources = GetParameterSources(@params);
			var commands = Factory.CreateDeleteCommands(parameterSources, typeof(T));
			return Execute(commands);
		}

		public TResult ExecuteScalar<TResult>(string sql, object @params = null, IValueSerializer serializer = null)
		{
			var parameterSource = GetParameterSource(@params);
			var command = Factory.CreateExecuteCommand(sql, parameterSource);
			return ExecuteScalar<TResult>(command, serializer);
		}

		public TResult ExecuteScalar<TMap, TResult>(string sql, object @params = null, IValueSerializer serializer = null)
		{
			var parameterSource = GetParameterSource(@params);
			var command = Factory.CreateExecuteCommand(sql, parameterSource, typeof(TMap));
			return ExecuteScalar<TResult>(command, serializer);
		}

		private T ExecuteScalar<T>(IExecuteCommand command, IValueSerializer serializer = null)
		{
			var connection = GetConnection();
			var value = command.ExecuteScalar(connection);

			if (serializer != null || _configuration.Defaults.TryGetSerializer(typeof(T), out serializer))
				value = serializer.Deserialize(value);

			return (T)value;
		}

		private static object GetParameterSource(object @params)
		{
			return @params as IEnumerable<IDataParameter> ?? @params ?? NoParameters.Instance;
		}

		private static IEnumerable<object> GetParameterSources(object @params)
		{
			if (@params == null)
				return new[] { NoParameters.Instance };

			var dataParameters = @params as IEnumerable<IDataParameter>;
			if (dataParameters != null)
				return new[] { dataParameters };

			var enumerable = @params as IEnumerable<object>;
			return enumerable ?? new[] { @params };
		}

		private int Execute(IEnumerable<IExecuteCommand> commands)
		{
			var connection = GetConnection();
			return commands.Sum(x => x.Execute(connection));
		}

		public void Dispose()
		{
			if (_connection != null)
				_connection.Dispose();
		}
	}
}