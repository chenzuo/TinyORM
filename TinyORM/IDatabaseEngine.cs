
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace TinyORM
{
	internal interface IDatabaseEngine
	{
		//IEnumerable<object> Query(string sql, object @params = null);
		//IEnumerable<T> Query<T>(string sql, object @params = null);
		//IEnumerable<T> Query<T>(object @params = null);
		////IEnumerable<dynamic> Query(string sql, object @params = null);

		int Execute(string sql, object @params = null);
		int Execute<T>(string sql, object @params = null);

		//TResult ExecuteScalar<TResult>(string sql, object @params = null);
		//TResult ExecuteScalar<T, TResult>(string sql, object @params = null);

		int Save<T>(object @params);
		int Insert<T>(object @params);
		int Update<T>(object @params);
		int Delete<T>(object @params);

		////int Update<T>(object set, object where);
	}

	internal class DatabaseEngine : IDatabaseEngine
	{
		public int Save<T>(object @params)
		{
			var parameterSources = GetParameterSources(@params);
			var grouping = GroupBySaveOperation(parameterSources);
			var insertCommands = Factory.CreateInsertCommands<T>(grouping.Insert);
			var updateCommands = Factory.CreateUpdateCommands<T>(grouping.Update);
			var deleteCommands = Factory.CreateDeleteCommands<T>(grouping.Delete);
			var allCommands = deleteCommands.Concat(insertCommands).Concat(updateCommands);
			return Execute(allCommands);
		}

		private int Execute(IEnumerable<IDatabaseCommand> commands)
		{
			var connection = GetConnection();
			return commands.Sum(x => x.Execute(connection));
		}

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
			throw new NotImplementedException();
		}

		private class SaveGrouping
		{
			public IEnumerable<object> Skip { get; set; }
			public IEnumerable<object> Insert { get; set; }
			public IEnumerable<object> Update { get; set; }
			public IEnumerable<object> Delete { get; set; }
		}

		public int Insert<T>(object @params)
		{
			var parameterSources = GetParameterSources(@params);
			var commands = Factory.CreateInsertCommands<T>(parameterSources);
			return Execute(commands);
		}

		public int Update<T>(object @params)
		{
			var parameterSources = GetParameterSources(@params);
			var commands = Factory.CreateUpdateCommands<T>(parameterSources);
			return Execute(commands);
		}

		public int Delete<T>(object @params)
		{
			var parameterSources = GetParameterSources(@params);
			var commands = Factory.CreateDeleteCommands<T>(parameterSources);
			return Execute(commands);
		}

		public int Execute(string sql, object @params = null)
		{
			var parameterSources = GetParameterSources(@params);
			var commands = Factory.CreateExecuteCommands(sql, parameterSources);
			return Execute(commands);
		}

		public int Execute<T>(string sql, object @params = null)
		{
			var parameterSources = GetParameterSources(@params);
			var commands = Factory.CreateExecuteCommands(sql, parameterSources, typeof(T));
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
	}

	internal class NoParameters
	{
		public static readonly NoParameters Instance = new NoParameters();

		private NoParameters() { }
	}

	internal interface ICommandFactory
	{
		IDatabaseCommand CreateSelectCommand<T>(object parameterSource);
		IEnumerable<IDatabaseCommand> CreateInsertCommands<T>(IEnumerable<object> parameterSources);
		IEnumerable<IDatabaseCommand> CreateUpdateCommands<T>(IEnumerable<object> parameterSources);
		IEnumerable<IDatabaseCommand> CreateDeleteCommands<T>(IEnumerable<object> parameterSources);
		IEnumerable<IDatabaseCommand> CreateExecuteCommands(string sql, IEnumerable<object> parameterSources, Type mapType = null);
	}

	internal class CommandFactory : ICommandFactory
	{
		public IDatabaseCommand CreateSelectCommand<T>(object parameterSource)
		{
			var typeMap = GetTypeMap(typeof(T));
			var commandBuilders = GetCommandBuilders(parameterSource, typeMap).ToList();
			var where = string.Join(", ", commandBuilders.Select(x => x.GetSql()));
			var sql = SqlBuilder.GetSelect(typeMap, where);
			var parameters = commandBuilders.SelectMany(x => x.GetParameters());
			return new DatabaseCommand(sql, parameters);
		}

		public IEnumerable<IDatabaseCommand> CreateInsertCommands<T>(IEnumerable<object> parameterSources)
		{
			var typeMap = GetTypeMap(typeof(T));
			var sql = SqlBuilder.GetInsert(typeMap);
			foreach (var parameterSource in parameterSources)
			{
				var commandBuilders = GetCommandBuilders(parameterSource, typeMap).ToList();
				var parameters = commandBuilders.SelectMany(x => x.GetParameters());
				//var afterInsertActions = commandBuilders.Select(x => x.GetAfterInsertAction()).Where(x => x != null);
				//yield return new DatabaseCommand(sql, parameters, afterInsertActions);
				yield return new DatabaseCommand(sql, parameters);
			}
		}

		public IEnumerable<IDatabaseCommand> CreateUpdateCommands<T>(IEnumerable<object> parameterSources)
		{
			var typeMap = GetTypeMap(typeof(T));
			var sql = SqlBuilder.GetUpdate(typeMap);
			foreach (var parameterSource in parameterSources)
			{
				var commandBuilders = GetCommandBuilders(parameterSource, typeMap).ToList();
				var parameters = commandBuilders.SelectMany(x => x.GetParameters());
				//var afterUpdateActions = commandBuilders.Select(x => x.GetAfterUpdateAction()).Where(x => x != null);
				//yield return new DatabaseCommand(sql, parameters, afterUpdateActions);
				yield return new DatabaseCommand(sql, parameters);
			}
		}

		public IEnumerable<IDatabaseCommand> CreateDeleteCommands<T>(IEnumerable<object> parameterSources)
		{
			var typeMap = GetTypeMap(typeof(T));
			var sql = SqlBuilder.GetDelete(typeMap);
			foreach (var parameterSource in parameterSources)
			{
				var commandBuilders = GetCommandBuilders(parameterSource, typeMap).ToList();
				var parameters = commandBuilders.SelectMany(x => x.GetParameters());
				yield return new DatabaseCommand(sql, parameters);
			}
		}

		public IEnumerable<IDatabaseCommand> CreateExecuteCommands(string sql, IEnumerable<object> parameterSources, Type mapType = null)
		{
			parameterSources = parameterSources as ICollection<object> ?? parameterSources.ToList();

			var typeMap = GetTypeMap(mapType ?? parameterSources.First().GetType());
			foreach (var parameterSource in parameterSources)
			{
				var commandBuilders = GetCommandBuilders(parameterSource, typeMap).ToList();
				var parameters = commandBuilders.SelectMany(x => x.GetParameters());
				var cmdSql = commandBuilders.Aggregate(sql, (current, commandBuilder) => commandBuilder.UpdateSql(current));
				yield return new DatabaseCommand(cmdSql, parameters);
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
			var dataParameterBuilder = value as IParameterBuilder ?? new EqualToParameterBuilder(value);
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
			throw new NotImplementedException();
		}

		public string UpdateSql(string sql)
		{
			throw new NotImplementedException();
		}
	}

	public interface ISqlBuilder
	{
		string GetSelect(TypeMap typeMap, string commandBuilders);
		string GetInsert(TypeMap typeMap);
		string GetUpdate(TypeMap typeMap);
		string GetDelete(TypeMap typeMap);
	}

	public interface IParameterBuilder
	{
		IEnumerable<IDataParameter> GetParameters(string name);
		string GetSql(string name);
		string UpdateSql(string sql, string name);
	}

	internal abstract class ParameterBuilder : IParameterBuilder
	{
		protected readonly object Value;

		protected ParameterBuilder(object value)
		{
			Value = value;
		}

		public virtual IEnumerable<IDataParameter> GetParameters(string name)
		{
			yield return CreateParameter(name, Value);
		}

		protected static SqlParameter CreateParameter(string name, object value)
		{
			return new SqlParameter(name, value);
		}

		protected static string GetParameterName(string name)
		{
			return "@" + name;
		}

		public virtual string UpdateSql(string sql, string name)
		{
			return sql;
		}

		public abstract string GetSql(string name);
	}

	internal class EqualToParameterBuilder : ParameterBuilder
	{
		public EqualToParameterBuilder(object value)
			: base(value)
		{ }

		public override string GetSql(string name)
		{
			return string.Format("{0} = {1}", name, GetParameterName(name));
		}
	}

	internal class NotEqualToParameterBuilder : ParameterBuilder
	{
		public NotEqualToParameterBuilder(object value)
			: base(value)
		{ }

		public override string GetSql(string name)
		{
			return string.Format("{0} != {1}", name, GetParameterName(name));
		}
	}

	internal class InParameterBuilder<T> : ParameterBuilder
	{
		private readonly ICollection<T> _collection;

		public InParameterBuilder(IEnumerable<T> collection)
			: base(collection)
		{
			_collection = collection.ToList();
		}

		public override IEnumerable<IDataParameter> GetParameters(string name)
		{
			var index = 0;
			return from value in _collection
					 let parameterName = string.Format("{0}_{1}", GetParameterName(name), index++)
					 select CreateParameter(parameterName, value);
		}

		public override string GetSql(string name)
		{
			return string.Format("{0} in ({1})", name, GetParameterNames(name));
		}

		public override string UpdateSql(string sql, string name)
		{
			return sql.Replace(GetParameterName(name), GetParameterNames(name));
		}

		private string GetParameterNames(string name)
		{
			var parameterNames = from index in Enumerable.Range(0, _collection.Count)
										select string.Format("{0}_{1}", GetParameterName(name), index);
			return string.Join(", ", parameterNames);
		}
	}

	internal interface ISqlGenerator
	{
		string GenerateSelect(ITypeMap typeMap);
		string GenerateInsert(ITypeMap typeMap);
		string GenerateUpdate(ITypeMap typeMap);
		string GenerateDelete(ITypeMap typeMap);
	}

	internal class SqlGenerator : ISqlGenerator
	{
		public string GenerateSelect(ITypeMap typeMap)
		{
			throw new NotImplementedException();
		}

		public string GenerateInsert(ITypeMap typeMap)
		{
			var propertyMaps = typeMap.Properties;
			return string.Format("insert into {0} ({1}) values ({2})",
										GetSchemaAndTable(typeMap),
										string.Join(", ", propertyMaps.Select(x => Enclose(x.ColumnName))),
										string.Join(", ", propertyMaps.Select(x => "@" + x.ColumnName)));
		}

		public string GenerateUpdate(ITypeMap typeMap)
		{
			var propertyMaps = typeMap.Properties;
			var keys = propertyMaps.Where(x => x.IsPrimaryKey);
			var values = propertyMaps.Where(x => !x.IsPrimaryKey);
			return string.Format("update {0} set {1} where {2}",
										GetSchemaAndTable(typeMap),
										GetColumnEqualsParameter(values),
										GetColumnEqualsParameter(keys));
		}

		public string GenerateDelete(ITypeMap typeMap)
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

		private static string GetColumnEqualsParameter(IEnumerable<PropertyMap> propertyMaps)
		{
			return string.Join(", ", propertyMaps.Select(x => string.Format("{0} = @{1}", Enclose(x.ColumnName), x.ColumnName)));
		}

		private static string Enclose(string item)
		{
			return "[" + item + "]";
		}
	}

	internal interface IDatabaseCommand
	{
		int Execute(IDbConnection connection);
	}

	internal class DatabaseCommand : IDatabaseCommand
	{
		public DatabaseCommand(string sql, IEnumerable<IDataParameter> parameters)
		{
			throw new NotImplementedException();
		}

		public int Execute(IDbConnection connection)
		{
			var dbCommand = connection.CreateCommand();
			return dbCommand.ExecuteNonQuery();
		}
	}

	public static class Is
	{
		public static IParameterBuilder EqualTo(object value)
		{
			return new EqualToParameterBuilder(value);
		}

		public static IParameterBuilder NotEqualTo(object value)
		{
			return new NotEqualToParameterBuilder(value);
		}

		public static IParameterBuilder In<T>(IEnumerable<T> collection)
		{
			return new InParameterBuilder<T>(collection);
		}
	}
}