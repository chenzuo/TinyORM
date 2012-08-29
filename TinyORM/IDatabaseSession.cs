using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TinyORM
{
	public interface IDatabase : IDatabaseEngine
	{
		void Configure(Action<DatabaseConfigurationBuilder> action);
		void ValidateConfiguration();
	}

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

	public interface IDatabaseEngine : IDatabaseSession
	{
		void Execute(Action<IDatabaseSession> action);
	}

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

	internal class NoParameters
	{
		public static readonly NoParameters Instance = new NoParameters();

		private NoParameters() { }
	}

	internal interface ICommandFactory
	{
		IQueryCommand CreateSelectCommand(object parameterSource, string sql = null, Type mapType = null);
		IEnumerable<IExecuteCommand> CreateInsertCommands(IEnumerable<object> parameterSources, Type mapType);
		IEnumerable<IExecuteCommand> CreateUpdateCommands(IEnumerable<object> parameterSources, Type mapType);
		IEnumerable<IExecuteCommand> CreateDeleteCommands(IEnumerable<object> parameterSources, Type mapType);
		IEnumerable<IExecuteCommand> CreateExecuteCommands(string sql, IEnumerable<object> parameterSources, Type mapType = null);
		IExecuteCommand CreateExecuteCommand(string sql, object parameterSource, Type mapType = null);
	}

	internal class CommandFactory : ICommandFactory
	{
		private readonly MapConfiguration _configuration;

		public CommandFactory(MapConfiguration configuration)
		{
			_configuration = configuration;
		}

		public IQueryCommand CreateSelectCommand(object parameterSource, string sql = null, Type mapType = null)
		{
			var typeMap = GetTypeMap(mapType ?? parameterSource.GetType());
			var parameterBuilders = GetParameterBuilders(parameterSource, typeMap).ToList();
			var includedColumns = mapType == null ? SqlBuilderColumns.All : SqlBuilderColumns.Mapped;

			if (sql != null)
			{
				sql = parameterBuilders.Aggregate(sql, (current, commandBuilder) => commandBuilder.UpdateSql(current));
			}
			else
			{
				var where = string.Join(" and ", parameterBuilders.Select(x => x.GetSql()));
				sql = SqlBuilder.GetSelect(typeMap, includedColumns, @where);
			}

			var parameters = parameterBuilders.SelectMany(x => x.GetParameters());
			var instanceMapperProvider = new InstanceMapperProvider(_configuration);
			return new QueryCommand(sql, parameters, instanceMapperProvider);
		}

		public IEnumerable<IExecuteCommand> CreateInsertCommands(IEnumerable<object> parameterSources, Type mapType)
		{
			var typeMap = GetTypeMap(mapType);
			var sql = SqlBuilder.GetInsert(typeMap);
			foreach (var parameterSource in parameterSources)
			{
				var parameterBuilders = GetParameterBuilders(parameterSource, typeMap).ToList();
				var parameters = parameterBuilders.SelectMany(x => x.GetParameters());
				//var afterInsertActions = commandBuilders.Select(x => x.GetAfterInsertAction()).Where(x => x != null);
				//yield return new ExecuteCommand(sql, parameters, afterInsertActions);
				yield return new ExecuteCommand(sql, parameters);
			}
		}

		public IEnumerable<IExecuteCommand> CreateUpdateCommands(IEnumerable<object> parameterSources, Type mapType)
		{
			var typeMap = GetTypeMap(mapType);
			var sql = SqlBuilder.GetUpdate(typeMap);
			foreach (var parameterSource in parameterSources)
			{
				var parameterBuilders = GetParameterBuilders(parameterSource, typeMap).ToList();
				var parameters = parameterBuilders.SelectMany(x => x.GetParameters());
				//var afterUpdateActions = commandBuilders.Select(x => x.GetAfterUpdateAction()).Where(x => x != null);
				//yield return new ExecuteCommand(sql, parameters, afterUpdateActions);
				yield return new ExecuteCommand(sql, parameters);
			}
		}

		public IEnumerable<IExecuteCommand> CreateDeleteCommands(IEnumerable<object> parameterSources, Type mapType)
		{
			var typeMap = GetTypeMap(mapType);
			var sql = SqlBuilder.GetDelete(typeMap);
			foreach (var parameterSource in parameterSources)
			{
				var parameterBuilders = GetParameterBuilders(parameterSource, typeMap).ToList();
				var parameters = parameterBuilders.SelectMany(x => x.GetParameters());
				yield return new ExecuteCommand(sql, parameters);
			}
		}

		public IEnumerable<IExecuteCommand> CreateExecuteCommands(string sql, IEnumerable<object> parameterSources, Type mapType = null)
		{
			return parameterSources.Select(x => CreateExecuteCommand(sql, x, mapType));
		}

		public IExecuteCommand CreateExecuteCommand(string sql, object parameterSource, Type mapType = null)
		{
			var typeMap = GetTypeMap(mapType ?? parameterSource.GetType());
			var parameterBuilders = GetParameterBuilders(parameterSource, typeMap).ToList();
			var parameters = parameterBuilders.SelectMany(x => x.GetParameters());
			var cmdSql = parameterBuilders.Aggregate(sql, (current, commandBuilder) => commandBuilder.UpdateSql(current));
			return new ExecuteCommand(cmdSql, parameters);
		}

		protected ISqlBuilder SqlBuilder
		{
			get { return new SqlBuilder(); }
		}

		private TableMap GetTypeMap(Type type)
		{
			return _configuration.Tables.GetOrCreate(type, _configuration.Defaults);
		}

		private IEnumerable<IParameterBuilder> GetParameterBuilders(object parameterSource, TableMap tableMap)
		{
			return new ParameterBuilderFactory(tableMap).GetCommandBuilders(parameterSource);
		}
	}

	internal class ParameterBuilderFactory
	{
		private readonly ITableMap _tableMap;

		public ParameterBuilderFactory(TableMap tableMap)
		{
			_tableMap = tableMap;
		}

		public IEnumerable<IParameterBuilder> GetCommandBuilders(object source)
		{
			return source.GetType() == _tableMap.Type
						 ? GetFromSourceMatchingTypeMap(source)
						 : GetFromAdHocSource(source);
		}

		private IEnumerable<IParameterBuilder> GetFromSourceMatchingTypeMap(object source)
		{
			return _tableMap.Columns.Select(x => CreateParameterBuilder(source, x));
		}

		private IEnumerable<IParameterBuilder> GetFromAdHocSource(object source)
		{
			var sourceType = source.GetType();
			var propertyMaps = _tableMap.Columns.ToDictionary(x => x.PropertyName.Replace(".", "").ToLower());

			foreach (var property in sourceType.GetProperties())
			{
				if (property.PropertyType == _tableMap.Type)
				{
					var getter = PropertyGetter.Create(sourceType, property.Name);
					foreach (var commandBuilder in GetFromSourceMatchingTypeMap(getter.Get(source)))
						yield return commandBuilder;
					continue;
				}

				IColumnMap columnMap;
				if (!propertyMaps.TryGetValue(property.Name.ToLower(), out columnMap))
					continue;

				yield return CreateParameterBuilder(source, columnMap);
			}
		}

		private IParameterBuilder CreateParameterBuilder(object source, IColumnMap columnMap)
		{
			var value = PropertyGetter.Create(source.GetType(), columnMap.PropertyName).Get(source);
			var parameterFactory = value as IParameterFactory ?? new SingleParameterFactory("=", value);
			return new ParameterBuilder(columnMap, parameterFactory);
		}
	}

	public interface IParameterBuilder
	{
		IEnumerable<IDataParameter> GetParameters();
		string GetSql();
		string UpdateSql(string sql);
	}

	internal class ParameterBuilder : IParameterBuilder
	{
		private readonly IColumnMap _columnMap;
		private readonly IParameterFactory _parameterFactory;

		public ParameterBuilder(IColumnMap columnMap, IParameterFactory parameterFactory)
		{
			_columnMap = columnMap;
			_parameterFactory = parameterFactory;
		}

		public IEnumerable<IDataParameter> GetParameters()
		{
			return _parameterFactory.GetParameters(_columnMap);
		}

		public string GetSql()
		{
			return _parameterFactory.GetSql(_columnMap.ColumnName);
		}

		public string UpdateSql(string sql)
		{
			return _parameterFactory.UpdateSql(sql, _columnMap.ColumnName);
		}
	}

	public interface ISqlBuilder
	{
		string GetSelect(ITableMap tableMap, SqlBuilderColumns columns, string whereStatement);
		string GetInsert(ITableMap tableMap);
		string GetUpdate(ITableMap tableMap);
		string GetDelete(ITableMap tableMap);
	}

	public enum SqlBuilderColumns
	{
		All,
		Mapped
	}

	public interface IParameterFactory
	{
		IEnumerable<IDataParameter> GetParameters(IColumnMap columnMap);
		string GetSql(string columnName);
		string UpdateSql(string sql, string columnName);
	}

	internal abstract class ParameterFactory : IParameterFactory
	{
		protected readonly object Value;
		private readonly string _parameterName;

		protected ParameterFactory(object value, string parameterName = null)
		{
			Value = value;
			_parameterName = parameterName;
		}

		public abstract IEnumerable<IDataParameter> GetParameters(IColumnMap columnMap);
		public abstract string GetSql(string columnName);
		public abstract string UpdateSql(string sql, string columnName);

		protected SqlParameter CreateParameter(IColumnMap columnMap, object value)
		{
			if (columnMap.Serializer != null)
				value = columnMap.Serializer.Serialize(value);

			return new SqlParameter(GetParameterName(columnMap.ColumnName), value)
				{
					IsNullable = columnMap.IsNullable,
					Precision = columnMap.ColumnPrecision,
					Scale = columnMap.ColumnScale,
					Size = columnMap.ColumnMaxLength
				};
		}

		protected string GetParameterName(string columnName)
		{
			return "@" + (_parameterName ?? columnName);
		}
	}

	internal class NoParameterFactory : IParameterFactory
	{
		private readonly string _operator;

		public NoParameterFactory(string @operator)
		{
			_operator = @operator;
		}

		public string GetSql(string columnName)
		{
			return string.Format("{0} {1}", columnName, _operator);
		}

		public IEnumerable<IDataParameter> GetParameters(IColumnMap columnMap)
		{
			yield break;
		}

		public string UpdateSql(string sql, string columnName)
		{
			return sql;
		}
	}

	internal class SingleParameterFactory : ParameterFactory
	{
		private readonly string _operator;

		public SingleParameterFactory(string @operator, object value, string parameterName = null)
			: base(value, parameterName)
		{
			_operator = @operator;
		}

		public override IEnumerable<IDataParameter> GetParameters(IColumnMap columnMap)
		{
			yield return CreateParameter(columnMap, Value);
		}

		public override string GetSql(string columnName)
		{
			return string.Format("{0} {1} {2}", columnName, _operator, GetParameterName(columnName));
		}

		public override string UpdateSql(string sql, string columnName)
		{
			return sql;
		}
	}

	internal class MultiParameterFactory : ParameterFactory
	{
		private readonly string _operator;

		public MultiParameterFactory(string @operator, IEnumerable<object> values, string parameterName = null)
			: base(values as ICollection<object> ?? values.ToList(), parameterName)
		{
			_operator = @operator;
		}

		private IEnumerable<object> Values
		{
			get { return (IEnumerable<object>)Value; }
		}

		public override IEnumerable<IDataParameter> GetParameters(IColumnMap columnMap)
		{
			var index = 0;
			var copy = new ColumnMap
				{
					ColumnName = columnMap.ColumnName,
					ColumnType = columnMap.ColumnType,
					IsPrimaryKey = columnMap.IsPrimaryKey,
					PropertyName = columnMap.PropertyName,
					PropertyType = columnMap.PropertyType,
					Serializer = columnMap.Serializer
				};

			foreach (var value in Values)
			{
				copy.ColumnName = string.Format("{0}_{1}", columnMap.ColumnName, index++);
				yield return CreateParameter(copy, value);
			}
		}

		public override string GetSql(string columnName)
		{
			return string.Format("{0} {1} ({2})", columnName, _operator, GetParameterNames(columnName));
		}

		public override string UpdateSql(string sql, string columnName)
		{
			var pattern = Regex.Escape(GetParameterName(columnName));
			var replacement = GetParameterNames(columnName);
			return Regex.Replace(sql, pattern, replacement, RegexOptions.IgnoreCase);
		}

		private string GetParameterNames(string name)
		{
			var parameterNames = from index in Enumerable.Range(0, Values.Count())
										select string.Format("{0}_{1}", GetParameterName(name), index);
			return string.Join(", ", parameterNames);
		}
	}

	internal class SqlBuilder : ISqlBuilder
	{
		public string GetSelect(ITableMap tableMap, SqlBuilderColumns columns, string whereStatement)
		{
			var sql = string.Format("select {0} from {1}", GetColumns(tableMap), GetSchemaAndTable(tableMap));
			return string.IsNullOrWhiteSpace(whereStatement)
						 ? sql
						 : sql + " where " + whereStatement;
		}

		public string GetInsert(ITableMap tableMap)
		{
			var propertyMaps = tableMap.Columns;
			return string.Format("insert {0} ({1}) values ({2})",
										GetSchemaAndTable(tableMap),
										GetColumns(tableMap),
										string.Join(", ", propertyMaps.Select(GetParameter)));
		}

		public string GetUpdate(ITableMap tableMap)
		{
			var propertyMaps = tableMap.Columns;
			var keys = propertyMaps.Where(x => x.IsPrimaryKey);
			var values = propertyMaps.Where(x => !x.IsPrimaryKey);
			return string.Format("update {0} set {1} where {2}",
										GetSchemaAndTable(tableMap),
										GetColumnEqualsParameter(", ", values),
										GetColumnEqualsParameter(" and ", keys));
		}

		public string GetDelete(ITableMap tableMap)
		{
			var propertyMaps = tableMap.Columns;
			var keys = propertyMaps.Where(x => x.IsPrimaryKey);
			return string.Format("delete {0} where {1}",
										GetSchemaAndTable(tableMap),
										GetColumnEqualsParameter(" and ", keys));
		}

		private static string GetSchemaAndTable(ITableMap tableMap)
		{
			return string.Format("{0}.{1}", Enclose(tableMap.Schema), Enclose(tableMap.Table));
		}

		private static string GetColumns(ITableMap tableMap)
		{
			return string.Join(", ", tableMap.Columns.Select(x => Enclose(x.ColumnName)));
		}

		private static string GetColumnEqualsParameter(string separator, IEnumerable<IColumnMap> propertyMaps)
		{
			return string.Join(separator, propertyMaps.Select(x => string.Format("{0} = {1}", Enclose(x.ColumnName), GetParameter(x))));
		}

		private static string GetParameter(IColumnMap columnMap)
		{
			return "@" + columnMap.ColumnName;
		}

		private static string Enclose(string item)
		{
			return "[" + item + "]";
		}
	}

	internal interface IQueryCommand
	{
		IEnumerable<T> Query<T>(IDbConnection connection);
	}

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

	internal interface IInstanceMapperProvider
	{
		IInstanceMapper GetMapper(Type targetType, IEnumerable<DataColumnInfo> schema);
	}

	internal class InstanceMapperProvider : IInstanceMapperProvider
	{
		private readonly MapConfiguration _configuration;
		private readonly ConcurrentDictionary<string, IInstanceMapper> _cache = new ConcurrentDictionary<string, IInstanceMapper>();

		public InstanceMapperProvider(MapConfiguration configuration)
		{
			_configuration = configuration;
		}

		public IInstanceMapper GetMapper(Type targetType, IEnumerable<DataColumnInfo> schema)
		{
			schema = schema.ToList();
			var key = CreateKey(targetType, schema);
			return _cache.GetOrAdd(key, _ => CreateMapper(_configuration.Tables.GetOrCreate(targetType, _configuration.Defaults), schema));
		}

		private static string CreateKey(Type targetType, IEnumerable<DataColumnInfo> schema)
		{
			var strings = schema.Select(x => string.Format("{0}:{1}", x.Index, x.Name.ToLower()));
			return string.Format("{0}<{1}", targetType.FullName, string.Join(";", strings));
		}

		private IInstanceMapper CreateMapper(TableMap tableMap, IEnumerable<DataColumnInfo> schema)
		{
			var targetType = tableMap.Type;
			if (targetType == typeof(ExpandoObject))
				return new ExpandoObjectMapper();

			if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(DataEntityTuple<>))
				return CreateDataEntityTupleMapper(tableMap, schema);

			return CreateObjectMapper(tableMap, schema);
		}

		private IInstanceMapper CreateDataEntityTupleMapper(TableMap tableMap, IEnumerable<DataColumnInfo> schema)
		{
			var expandoObjectMapper = new ExpandoObjectMapper();
			var objectMapper = CreateObjectMapper(tableMap, schema);
			return new DataEntityTupleMapper(expandoObjectMapper, objectMapper);
		}

		private ObjectMapper CreateObjectMapper(TableMap tableMap, IEnumerable<DataColumnInfo> schema)
		{
			var instanceFactory = _configuration.InstanceFactoryProviders.FirstOrDefault(x => x.CanHandle(tableMap.Type)) ??
										 new DefaultInstanceFactory();
			var mapper = new ObjectMapper(instanceFactory);
			mapper.Initialize(tableMap, schema);
			return mapper;
		}
	}

	public class DataColumnInfo
	{
		public int Index { get; set; }
		public string Name { get; set; }
	}


	internal interface IExecuteCommand
	{
		int Execute(IDbConnection connection);
		object ExecuteScalar(IDbConnection connection);
	}

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

	public static class Is
	{
		public static IParameterFactory Null
		{
			get { return new NoParameterFactory("is null"); }
		}

		public static IParameterFactory NotNull
		{
			get { return new NoParameterFactory("is not null"); }
		}

		public static IParameterFactory EqualTo(object value, string parameterName = null)
		{
			return new SingleParameterFactory("=", value, parameterName);
		}

		public static IParameterFactory NotEqualTo(object value, string parameterName = null)
		{
			return new SingleParameterFactory("!=", value, parameterName);
		}

		public static IParameterFactory LessThan(object value, string parameterName = null)
		{
			return new SingleParameterFactory("<", value, parameterName);
		}

		public static IParameterFactory LessThanOrEqualTo(object value, string parameterName = null)
		{
			return new SingleParameterFactory("<=", value, parameterName);
		}

		public static IParameterFactory GreaterThan(object value, string parameterName = null)
		{
			return new SingleParameterFactory(">", value, parameterName);
		}

		public static IParameterFactory GreaterThanOrEqualTo(object value, string parameterName = null)
		{
			return new SingleParameterFactory(">=", value, parameterName);
		}

		public static IParameterFactory Like(object value, string parameterName = null)
		{
			return new SingleParameterFactory("like", value, parameterName);
		}

		public static IParameterFactory NotLike(object value, string parameterName = null)
		{
			return new SingleParameterFactory("not like", value, parameterName);
		}

		public static IParameterFactory In(IEnumerable<object> values, string parameterName = null)
		{
			return new MultiParameterFactory("in", values, parameterName);
		}

		public static IParameterFactory NotIn(IEnumerable<object> values, string parameterName = null)
		{
			return new MultiParameterFactory("not in", values, parameterName);
		}
	}

	internal interface IInstanceMapper
	{
		object Map(IDataRecord record, Type type);
	}

	internal class ExpandoObjectMapper : IInstanceMapper
	{
		public object Map(IDataRecord record, Type type)
		{
			if (type != typeof(ExpandoObject))
				throw new ArgumentException();

			var expandoObject = new ExpandoObject();
			var dictionary = (IDictionary<string, object>)expandoObject;
			var noNameIndex = 0;

			for (var i = 0; i < record.FieldCount; i++)
			{
				var name = record.GetName(i) ?? "NoName" + noNameIndex++;
				dictionary.Add(name, record.IsDBNull(i) ? null : record.GetValue(i));
			}

			return expandoObject;
		}
	}

	internal class ObjectMapper : IInstanceMapper
	{
		private readonly IInstanceFactory _instanceFactory;
		private readonly List<DataRecordToPropertyMapper> _recordToPropertyMappers;

		public ObjectMapper(IInstanceFactory instanceFactory)
		{
			_instanceFactory = instanceFactory;
			_recordToPropertyMappers = new List<DataRecordToPropertyMapper>();
		}

		public void Initialize(TableMap tableMap, IEnumerable<DataColumnInfo> schema)
		{
			var dictionary = schema.ToDictionary(x => x.Name.ToLower(), x => x.Index);
			foreach (var propertyMap in tableMap.Columns)
			{
				int index;
				if (!dictionary.TryGetValue(propertyMap.ColumnName.ToLower(), out index)) continue;
				var setter = PropertySetter.Create(tableMap.Type, propertyMap.PropertyName);
				_recordToPropertyMappers.Add(new DataRecordToPropertyMapper(index, setter, propertyMap.Serializer));
			}
		}

		public object Map(IDataRecord record, Type type)
		{
			var instance = _instanceFactory.CreateInstance(type);
			foreach (var propertySetter in _recordToPropertyMappers)
				propertySetter.Execute(record, instance);
			return instance;
		}

		internal class DataRecordToPropertyMapper
		{
			private readonly int _index;
			private readonly PropertySetter _setter;
			private readonly IValueSerializer _serializer;

			public DataRecordToPropertyMapper(int index, PropertySetter setter, IValueSerializer serializer = null)
			{
				_index = index;
				_setter = setter;
				_serializer = serializer;
			}

			public void Execute(IDataRecord record, object target)
			{
				var value = record.GetValue(_index);

				if (value == DBNull.Value)
					value = null;

				if (_serializer != null)
					value = _serializer.Deserialize(value);

				_setter.Set(target, value);
			}
		}
	}

	public interface IInstanceFactory
	{
		bool CanHandle(Type type);
		object CreateInstance(Type type);
	}

	internal class DataEntityTupleMapper : IInstanceMapper
	{
		private readonly IInstanceMapper _dynamicMapper;
		private readonly IInstanceMapper _entityMapper;

		public DataEntityTupleMapper(IInstanceMapper dynamicMapper, IInstanceMapper entityMapper)
		{
			_dynamicMapper = dynamicMapper;
			_entityMapper = entityMapper;
		}

		public object Map(IDataRecord record, Type type)
		{
			var genericType = typeof(DataEntityTuple<>).MakeGenericType(type);
			var data = _dynamicMapper.Map(record, null);
			var entity = _entityMapper.Map(record, type);
			return Activator.CreateInstance(genericType, data, entity);
		}
	}

	public class MapConfiguration
	{
		public MapConfiguration()
		{
			Defaults = new Defaults();
			Tables = new TableMapDictionary();
			InstanceFactoryProviders = new List<IInstanceFactory>();
			SaveOperationProviders = new List<ISaveOperationProvider>();
		}

		public IDefaults Defaults { get; private set; }
		public TableMapDictionary Tables { get; private set; }
		public List<IInstanceFactory> InstanceFactoryProviders { get; private set; }
		public List<ISaveOperationProvider> SaveOperationProviders { get; private set; }
	}

	public interface IConnectionStringProvider
	{
		string GetConnectionString();
		string[] GetConnectionStrings();
	}

	internal class DefaultConnectionStringProvider : IConnectionStringProvider
	{
		public DefaultConnectionStringProvider()
		{
			;
		}
		public string GetConnectionString()
		{
			return NoOp();
		}

		public string[] GetConnectionStrings()
		{
			return new[] { NoOp() };
		}

		private static string NoOp()
		{
			throw new InvalidOperationException("No connection string provider defined.");
		}
	}

	public interface ISaveOperationProvider
	{
		bool CanHandle(Type type);
		SaveOperation GetSaveOperation(object instance);
	}

	public enum SaveOperation
	{
		Skip,
		Insert,
		Update,
		Delete
	}
}