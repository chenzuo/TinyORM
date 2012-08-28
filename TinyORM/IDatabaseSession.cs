using System;
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
		void COnfigure(Action<DatabaseConfigurationBuilder> action);
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

		//TResult ExecuteScalar<TResult>(string sql, object @params = null);
		//TResult ExecuteScalar<TMap, TResult>(string sql, object @params = null);

		int Save<TMap>(object @params);
		int Insert<TMap>(object @params);
		int Update<TMap>(object @params);
		int Delete<TMap>(object @params);
	}

	internal class DatabaseSession : IDatabaseSession
	{
		private MapConfiguration _configuration;

		private IDbConnection GetConnection()
		{
			throw new NotImplementedException();
		}

		protected ICommandFactory Factory
		{
			get { throw new NotImplementedException(); }
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
				var saveOperationProvider = _configuration.SaveOperationProviders.GetFirstItemMatching(sourceType);

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
			var command = Factory.CreateSelectCommand<ExpandoObject>(@params, sql);
			return Query<dynamic>(command);
		}

		public IEnumerable<T> Query<T>(string sql, object @params = null)
		{
			var command = Factory.CreateSelectCommand<T>(@params, sql);
			return Query<T>(command);
		}

		public IEnumerable<DataEntityTuple<T>> QueryData<T>(string sql, object @params = null)
		{
			var command = Factory.CreateSelectCommand<T>(@params, sql);
			return Query<DataEntityTuple<T>>(command);
		}

		public IEnumerable<T> Query<T>(object @params = null)
		{
			var command = Factory.CreateSelectCommand<T>(@params);
			return Query<T>(command);
		}

		private IEnumerable<T> Query<T>(IQueryCommand command)
		{
			var connection = GetConnection();
			return command.Query<T>(connection);
		}

		public int Save<TMap>(object @params)
		{
			var parameterSources = GetParameterSources(@params);
			var grouping = GroupBySaveOperation(parameterSources);
			var insertCommands = Factory.CreateInsertCommands<TMap>(grouping.Insert);
			var updateCommands = Factory.CreateUpdateCommands<TMap>(grouping.Update);
			var deleteCommands = Factory.CreateDeleteCommands<TMap>(grouping.Delete);
			var allCommands = deleteCommands.Concat(insertCommands).Concat(updateCommands);
			return Execute(allCommands);
		}

		public int Insert<TMap>(object @params)
		{
			var parameterSources = GetParameterSources(@params);
			var commands = Factory.CreateInsertCommands<TMap>(parameterSources);
			return Execute(commands);
		}

		public int Update<TMap>(object @params)
		{
			var parameterSources = GetParameterSources(@params);
			var commands = Factory.CreateUpdateCommands<TMap>(parameterSources);
			return Execute(commands);
		}

		public int Delete<TMap>(object @params)
		{
			var parameterSources = GetParameterSources(@params);
			var commands = Factory.CreateDeleteCommands<TMap>(parameterSources);
			return Execute(commands);
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

		private IEnumerable<object> GetParameterSources(object @params)
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
	}

	internal class NoParameters
	{
		public static readonly NoParameters Instance = new NoParameters();

		private NoParameters() { }
	}

	internal interface ICommandFactory
	{
		IQueryCommand CreateSelectCommand<TMap>(object parameterSource, string sql = null);
		IEnumerable<IExecuteCommand> CreateInsertCommands<TMap>(IEnumerable<object> parameterSources);
		IEnumerable<IExecuteCommand> CreateUpdateCommands<TMap>(IEnumerable<object> parameterSources);
		IEnumerable<IExecuteCommand> CreateDeleteCommands<TMap>(IEnumerable<object> parameterSources);
		IEnumerable<IExecuteCommand> CreateExecuteCommands(string sql, IEnumerable<object> parameterSources, Type mapType = null);
	}

	internal class CommandFactory : ICommandFactory
	{
		public IQueryCommand CreateSelectCommand<TMap>(object parameterSource, string sql = null)
		{
			var typeMap = GetTypeMap(typeof(TMap));
			var commandBuilders = GetCommandBuilders(parameterSource, typeMap).ToList();
			var includedColumns = typeof(TMap) == typeof(ExpandoObject) ||
										 typeof(TMap).IsAssignableFrom(typeof(DataEntityTuple<>))
											 ? IncludedColumns.All
											 : IncludedColumns.Mapped;

			if (sql != null)
			{
				sql = commandBuilders.Aggregate(sql, (current, commandBuilder) => commandBuilder.UpdateSql(current));
			}
			else
			{
				var where = string.Join(", ", commandBuilders.Select(x => x.GetSql()));
				sql = SqlBuilder.GetSelect(typeMap, includedColumns, @where);
			}

			var parameters = commandBuilders.SelectMany(x => x.GetParameters());

			//------------------------------------------------------------------------------
			//--  ToDo  --------------------------------------------------------------------
			//------------------------------------------------------------------------------

			return new QueryCommand(sql, parameters);
		}

		public IEnumerable<IExecuteCommand> CreateInsertCommands<TMap>(IEnumerable<object> parameterSources)
		{
			var typeMap = GetTypeMap(typeof(TMap));
			var sql = SqlBuilder.GetInsert(typeMap);
			foreach (var parameterSource in parameterSources)
			{
				var commandBuilders = GetCommandBuilders(parameterSource, typeMap).ToList();
				var parameters = commandBuilders.SelectMany(x => x.GetParameters());
				//var afterInsertActions = commandBuilders.Select(x => x.GetAfterInsertAction()).Where(x => x != null);
				//yield return new ExecuteCommand(sql, parameters, afterInsertActions);
				yield return new ExecuteCommand(sql, parameters);
			}
		}

		public IEnumerable<IExecuteCommand> CreateUpdateCommands<TMap>(IEnumerable<object> parameterSources)
		{
			var typeMap = GetTypeMap(typeof(TMap));
			var sql = SqlBuilder.GetUpdate(typeMap);
			foreach (var parameterSource in parameterSources)
			{
				var commandBuilders = GetCommandBuilders(parameterSource, typeMap).ToList();
				var parameters = commandBuilders.SelectMany(x => x.GetParameters());
				//var afterUpdateActions = commandBuilders.Select(x => x.GetAfterUpdateAction()).Where(x => x != null);
				//yield return new ExecuteCommand(sql, parameters, afterUpdateActions);
				yield return new ExecuteCommand(sql, parameters);
			}
		}

		public IEnumerable<IExecuteCommand> CreateDeleteCommands<TMap>(IEnumerable<object> parameterSources)
		{
			var typeMap = GetTypeMap(typeof(TMap));
			var sql = SqlBuilder.GetDelete(typeMap);
			foreach (var parameterSource in parameterSources)
			{
				var commandBuilders = GetCommandBuilders(parameterSource, typeMap).ToList();
				var parameters = commandBuilders.SelectMany(x => x.GetParameters());
				yield return new ExecuteCommand(sql, parameters);
			}
		}

		public IEnumerable<IExecuteCommand> CreateExecuteCommands(string sql, IEnumerable<object> parameterSources, Type mapType = null)
		{
			parameterSources = parameterSources as ICollection<object> ?? parameterSources.ToList();

			var typeMap = GetTypeMap(mapType ?? parameterSources.First().GetType());
			foreach (var parameterSource in parameterSources)
			{
				var commandBuilders = GetCommandBuilders(parameterSource, typeMap).ToList();
				var parameters = commandBuilders.SelectMany(x => x.GetParameters());
				var cmdSql = commandBuilders.Aggregate(sql, (current, commandBuilder) => commandBuilder.UpdateSql(current));
				yield return new ExecuteCommand(cmdSql, parameters);
			}
		}

		protected ISqlBuilder SqlBuilder
		{
			get { throw new NotImplementedException(); }
		}

		private TypeMap GetTypeMap(Type type)
		{
			throw new NotImplementedException();
		}

		private IEnumerable<ICommandBuilder> GetCommandBuilders(object parameterSource, TypeMap typeMap)
		{
			return new CommandBuilderFactory(typeMap).GetCommandBuilders(parameterSource).ToList();
		}
	}

	internal class CommandBuilderFactory
	{
		private readonly TypeMap _typeMap;

		public CommandBuilderFactory(TypeMap typeMap)
		{
			_typeMap = typeMap;
		}

		public IEnumerable<ICommandBuilder> GetCommandBuilders(object source)
		{
			return source.GetType() == _typeMap.Type
						 ? GetFromSourceMatchingTypeMap(source)
						 : GetFromAdHocSource(source);
		}

		private IEnumerable<ICommandBuilder> GetFromSourceMatchingTypeMap(object source)
		{
			return _typeMap.Properties.Select(x => CreateCommandBuilder(source, x.PropertyName, x));
		}

		private IEnumerable<ICommandBuilder> GetFromAdHocSource(object source)
		{
			var sourceType = source.GetType();
			var propertyMaps = _typeMap.Properties.ToDictionary(x => x.PropertyName.Replace(".", "").ToLower());

			foreach (var property in sourceType.GetProperties())
			{
				if (property.PropertyType == _typeMap.Type)
				{
					var getter = PropertyGetter.Create(sourceType, property.Name);
					foreach (var commandBuilder in GetFromSourceMatchingTypeMap(getter.Get(source)))
						yield return commandBuilder;
					continue;
				}

				PropertyMap propertyMap;
				if (!propertyMaps.TryGetValue(property.Name.ToLower(), out propertyMap))
					continue;

				yield return CreateCommandBuilder(source, property.Name, propertyMap);
			}
		}

		private ICommandBuilder CreateCommandBuilder(object source, string propertyName, PropertyMap propertyMap)
		{
			var value = PropertyGetter.Create(source.GetType(), propertyName).Get(source);
			var dataParameterBuilder = value as IParameterBuilder ?? new SingleParameterBuilder("=", value);
			return new CommandBuilder(propertyMap, dataParameterBuilder);
		}
	}

	public interface ICommandBuilder
	{
		IEnumerable<IDataParameter> GetParameters();
		string GetSql();
		string UpdateSql(string sql);
	}

	internal class CommandBuilder : ICommandBuilder
	{
		private readonly IPropertyMap _propertyMap;
		private readonly IParameterBuilder _parameterBuilder;

		public CommandBuilder(IPropertyMap propertyMap, IParameterBuilder parameterBuilder)
		{
			_propertyMap = propertyMap;
			_parameterBuilder = parameterBuilder;
		}

		public IEnumerable<IDataParameter> GetParameters()
		{
			return _parameterBuilder.GetParameters(_propertyMap.ColumnName);
		}

		public string GetSql()
		{
			return _parameterBuilder.GetSql(_propertyMap.ColumnName);
		}

		public string UpdateSql(string sql)
		{
			return _parameterBuilder.UpdateSql(sql, _propertyMap.ColumnName);
		}
	}

	public interface ISqlBuilder
	{
		string GetSelect(ITypeMap typeMap, IncludedColumns columns, string whereStatement);
		string GetInsert(ITypeMap typeMap);
		string GetUpdate(ITypeMap typeMap);
		string GetDelete(ITypeMap typeMap);
	}

	public enum IncludedColumns
	{
		All,
		Mapped
	}

	public interface IParameterBuilder
	{
		IEnumerable<IDataParameter> GetParameters(string columnName);
		string GetSql(string columnName);
		string UpdateSql(string sql, string columnName);
	}

	internal abstract class ParameterBuilder : IParameterBuilder
	{
		protected readonly object Value;
		private readonly string _parameterName;

		protected ParameterBuilder(object value, string parameterName = null)
		{
			Value = value;
			_parameterName = parameterName;
		}

		public abstract IEnumerable<IDataParameter> GetParameters(string columnName);
		public abstract string GetSql(string columnName);
		public abstract string UpdateSql(string sql, string columnName);

		protected SqlParameter CreateParameter(string columnName, object value)
		{
			return new SqlParameter(GetParameterName(columnName), value);
		}

		protected string GetParameterName(string columnName)
		{
			return "@" + (_parameterName ?? columnName);
		}
	}

	internal class NoParameterBuilder : IParameterBuilder
	{
		private readonly string _operator;

		public NoParameterBuilder(string @operator)
		{
			_operator = @operator;
		}

		public string GetSql(string columnName)
		{
			return string.Format("{0} {1}", columnName, _operator);
		}

		public IEnumerable<IDataParameter> GetParameters(string columnName)
		{
			yield break;
		}

		public string UpdateSql(string sql, string columnName)
		{
			return sql;
		}
	}

	internal class SingleParameterBuilder : ParameterBuilder
	{
		private readonly string _operator;

		public SingleParameterBuilder(string @operator, object value, string parameterName = null)
			: base(value, parameterName)
		{
			_operator = @operator;
		}

		public override IEnumerable<IDataParameter> GetParameters(string columnName)
		{
			yield return CreateParameter(columnName, Value);
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

	internal class MultiParameterBuilder : ParameterBuilder
	{
		private readonly string _operator;

		public MultiParameterBuilder(string @operator, IEnumerable<object> values, string parameterName = null)
			: base(values as ICollection<object> ?? values.ToList(), parameterName)
		{
			_operator = @operator;
		}

		private IEnumerable<object> Values
		{
			get { return (IEnumerable<object>)Value; }
		}

		public override IEnumerable<IDataParameter> GetParameters(string columnName)
		{
			var index = 0;
			return from value in Values
					 let parameterName = string.Format("{0}_{1}", GetParameterName(columnName), index++)
					 select CreateParameter(parameterName, value);
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
		public string GetSelect(ITypeMap typeMap, IncludedColumns columns, string whereStatement)
		{
			var sql = string.Format("select {0} from {1}", GetColumns(typeMap), GetSchemaAndTable(typeMap));
			return string.IsNullOrWhiteSpace(whereStatement)
						 ? sql
						 : sql + " where " + whereStatement;
		}

		public string GetInsert(ITypeMap typeMap)
		{
			var propertyMaps = typeMap.Properties;
			return string.Format("insert into {0} ({1}) values ({2})",
										GetSchemaAndTable(typeMap),
										GetColumns(typeMap),
										string.Join(", ", propertyMaps.Select(GetParameter)));
		}

		public string GetUpdate(ITypeMap typeMap)
		{
			var propertyMaps = typeMap.Properties;
			var keys = propertyMaps.Where(x => x.IsPrimaryKey);
			var values = propertyMaps.Where(x => !x.IsPrimaryKey);
			return string.Format("update {0} set {1} where {2}",
										GetSchemaAndTable(typeMap),
										GetColumnEqualsParameter(values),
										GetColumnEqualsParameter(keys));
		}

		public string GetDelete(ITypeMap typeMap)
		{
			var propertyMaps = typeMap.Properties;
			var keys = propertyMaps.Where(x => x.IsPrimaryKey);
			return string.Format("delete from {0} where {1}",
										GetSchemaAndTable(typeMap),
										GetColumnEqualsParameter(keys));
		}

		private static string GetSchemaAndTable(ITypeMap typeMap)
		{
			return string.Format("{0}.{1}", Enclose(typeMap.Namespace), Enclose(typeMap.Table));
		}

		private static string GetColumns(ITypeMap typeMap)
		{
			return string.Join(", ", typeMap.Properties.Select(x => Enclose(x.ColumnName)));
		}

		private static string GetColumnEqualsParameter(IEnumerable<PropertyMap> propertyMaps)
		{
			return string.Join(", ", propertyMaps.Select(x => string.Format("{0} = @{1}", Enclose(x.ColumnName), GetParameter(x))));
		}

		private static string GetParameter(PropertyMap propertyMap)
		{
			return "@" + propertyMap.ColumnName;
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
		// TODO Make this class thread safe

		private readonly DatabaseConfiguration _configuration;
		private readonly Dictionary<string, IInstanceMapper> _cache = new Dictionary<string, IInstanceMapper>();

		public InstanceMapperProvider(DatabaseConfiguration configuration)
		{
			_configuration = configuration;
		}

		public IInstanceMapper GetMapper(Type targetType, IEnumerable<DataColumnInfo> schema)
		{
			schema = schema.ToList();
			var key = CreateKey(targetType, schema);
			IInstanceMapper mapper;

			if (!_cache.TryGetValue(key, out mapper))
			{
				var typeMap = _configuration.Types.GetOrCreate(targetType);
				mapper = CreateMapper(typeMap, schema);
				_cache.Add(key, mapper);
			}

			return mapper;
		}

		private static string CreateKey(Type targetType, IEnumerable<DataColumnInfo> schema)
		{
			var strings = schema.Select(x => string.Format("{0}:{1}", x.Index, x.Name.ToLower()));
			return string.Format("{0}<{1}", targetType.FullName, string.Join(";", strings));
		}

		private IInstanceMapper CreateMapper(TypeMap typeMap, IEnumerable<DataColumnInfo> schema)
		{
			var targetType = typeMap.Type;
			if (targetType == typeof(ExpandoObject))
				return new ExpandoObjectMapper();

			if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(DataEntityTuple<>))
				return CreateDataEntityTupleMapper(typeMap, schema);

			return CreateObjectMapper(typeMap, schema);
		}

		private IInstanceMapper CreateDataEntityTupleMapper(TypeMap typeMap, IEnumerable<DataColumnInfo> schema)
		{
			var expandoObjectMapper = new ExpandoObjectMapper();
			var objectMapper = CreateObjectMapper(typeMap, schema);
			return new DataEntityTupleMapper(expandoObjectMapper, objectMapper);
		}

		private ObjectMapper CreateObjectMapper(TypeMap typeMap, IEnumerable<DataColumnInfo> schema)
		{
			var instanceFactory = _configuration.InstanceFactoryProviders.GetFirstItemMatching(typeMap.Type) ??
										 new DefaultInstanceFactory();
			var mapper = new ObjectMapper(instanceFactory);
			mapper.Initialize(typeMap, schema);
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
	}

	public static class Is
	{
		public static IParameterBuilder Null
		{
			get { return new NoParameterBuilder("is null"); }
		}

		public static IParameterBuilder NotNull
		{
			get { return new NoParameterBuilder("is not null"); }
		}

		public static IParameterBuilder EqualTo(object value, string parameterName = null)
		{
			return new SingleParameterBuilder("=", value, parameterName);
		}

		public static IParameterBuilder NotEqualTo(object value, string parameterName = null)
		{
			return new SingleParameterBuilder("!=", value, parameterName);
		}

		public static IParameterBuilder LessThan(object value, string parameterName = null)
		{
			return new SingleParameterBuilder("<", value, parameterName);
		}

		public static IParameterBuilder LessThanOrEqualTo(object value, string parameterName = null)
		{
			return new SingleParameterBuilder("<=", value, parameterName);
		}

		public static IParameterBuilder GreaterThan(object value, string parameterName = null)
		{
			return new SingleParameterBuilder(">", value, parameterName);
		}

		public static IParameterBuilder GreaterThanOrEqualTo(object value, string parameterName = null)
		{
			return new SingleParameterBuilder(">=", value, parameterName);
		}

		public static IParameterBuilder Like(object value, string parameterName = null)
		{
			return new SingleParameterBuilder("like", value, parameterName);
		}

		public static IParameterBuilder NotLike(object value, string parameterName = null)
		{
			return new SingleParameterBuilder("not like", value, parameterName);
		}

		public static IParameterBuilder In(IEnumerable<object> values, string parameterName = null)
		{
			return new MultiParameterBuilder("in", values, parameterName);
		}

		public static IParameterBuilder NotIn(IEnumerable<object> values, string parameterName = null)
		{
			return new MultiParameterBuilder("not in", values, parameterName);
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

		public void Initialize(TypeMap typeMap, IEnumerable<DataColumnInfo> schema)
		{
			var dictionary = schema.ToDictionary(x => x.Name.ToLower(), x => x.Index);
			foreach (var propertyMap in typeMap.Properties)
			{
				int index;
				if (!dictionary.TryGetValue(propertyMap.ColumnName.ToLower(), out index)) continue;
				_recordToPropertyMappers.Add(new DataRecordToPropertyMapper
				{
					Index = index,
					PropertySetter = PropertySetter.Create(typeMap.Type, propertyMap.PropertyName),
					Serializer = propertyMap.ValueSerializer
				});
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
			public int Index { get; set; }
			public PropertySetter PropertySetter { get; set; }
			public IValueSerializer Serializer { get; set; }

			public void Execute(IDataRecord record, object target)
			{
				var value = record.IsDBNull(Index) ? null : record.GetValue(Index);

				if (Serializer != null)
					value = Serializer.Deserialize(value);

				PropertySetter.Set(target, value);
			}
		}
	}

	public interface IInstanceFactory
	{
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
			Types = new TypeMapDictionary();
			InstanceFactoryProviders = new PredicateCollection<IInstanceFactory>();
			SaveOperationProviders = new PredicateCollection<ISaveOperationProvider>();
		}

		public TypeMapDictionary Types { get; private set; }
		public PredicateCollection<IInstanceFactory> InstanceFactoryProviders { get; private set; }
		public PredicateCollection<ISaveOperationProvider> SaveOperationProviders { get; private set; }

		public class PredicateCollection<T> where T : class
		{
			private readonly List<PredicateItemPair> _items = new List<PredicateItemPair>();

			public void Add(Predicate<Type> filter, T item)
			{
				_items.Add(new PredicateItemPair { Predicate = filter, Item = item });
			}

			public T GetFirstItemMatching(Type type)
			{
				return (from pair in _items where pair.Predicate(type) select pair.Item).FirstOrDefault();
			}

			private class PredicateItemPair
			{
				public Predicate<Type> Predicate;
				public T Item;
			}
		}
	}

	public interface ISaveOperationProvider
	{
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