
using System;
using System.Collections.Generic;
using System.Data;
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
				return new[] { new object() };

			var dataParameters = @params as IEnumerable<IDataParameter>;
			if (dataParameters != null)
				return new[] { dataParameters };

			var enumerable = @params as IEnumerable<object>;
			return enumerable ?? new[] { @params };
		}
	}

	internal interface ICommandFactory
	{
		IDatabaseCommand CreateSelectCommand<T>(IEnumerable<object> parameterSources);
		IEnumerable<IDatabaseCommand> CreateInsertCommands<T>(IEnumerable<object> parameterSources);
		IEnumerable<IDatabaseCommand> CreateUpdateCommands<T>(IEnumerable<object> parameterSource);
		IEnumerable<IDatabaseCommand> CreateDeleteCommands<T>(IEnumerable<object> parameterSource);
		IEnumerable<IDatabaseCommand> CreateExecuteCommands(string sql, IEnumerable<object> parameterSources, Type type = null);
	}

	internal class CommandFactory : ICommandFactory
	{
		public IDatabaseCommand CreateSelectCommand<T>(IEnumerable<object> parameterSources)
		{
			var typeMap = GetTypeMapFor<T>();
			var commandBuilders = GetCommandBuilders(parameterSource);
			var sql = SqlGenerator.GenerateSelect(typeMap, commandBuilders);
			var parameters = commandBuilders.SelectMany(x => x.GetParameters());
			return new DatabaseCommand(sql, parameters);
		}

		public IEnumerable<IDatabaseCommand> CreateInsertCommands<T>(IEnumerable<object> parameterSources)
		{
			var typeMap = GetTypeMapFor<T>();
			var sql = SqlGenerator.GenerateInsert(typeMap);
			foreach (var parameterSource in parameterSources)
			{
				var commandBuilders = GetCommandBuilders(parameterSource);
				var parameters = commandBuilders.SelectMany(x => x.GetParameters());
				var afterInsertActions = commandBuilders.Select(x => x.GetAfterInsertAction()).Where(x => x != null);
				yield return new DatabaseCommand(sql, parameters, afterInsertActions);
			}
		}

		public IEnumerable<IDatabaseCommand> CreateUpdateCommands<T>(IEnumerable<object> parameterSource)
		{
			var typeMap = GetTypeMapFor<T>();
			var sql = SqlGenerator.GenerateUpdate(typeMap);
			foreach (var parameterSource in parameterSources)
			{
				var commandBuilders = GetCommandBuilders(parameterSource);
				var parameters = commandBuilders.SelectMany(x => x.GetParameters());
				var afterUpdateActions = commandBuilders.Select(x => x.GetAfterUpdateAction()).Where(x => x != null);
				yield return new DatabaseCommand(sql, parameters, afterUpdateActions);
			}
		}

		public IEnumerable<IDatabaseCommand> CreateDeleteCommands<T>(IEnumerable<object> parameterSource)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IDatabaseCommand> CreateExecuteCommands(string sql, IEnumerable<object> parameterSources, Type type = null)
		{
			var typeMap = GetTypeMapFor<T>();
			foreach (var parameterSource in parameterSources)
			{
				var commandBuilders = GetCommandBuilders(parameterSource);
				var parameters = commandBuilders.SelectMany(x => x.GetParameters());
				var cmdSql = commandBuilders.Aggregate(sql, (current, commandBuilder) => commandBuilder.UpdateSql(current));
				yield return new DatabaseCommand(cmdSql, parameters);
			}
		}

		protected ISqlGenerator SqlGenerator
		{
			get { throw new NotImplementedException(); }
		}

		private TypeMap GetTypeMapFor<T>()
		{
			throw new NotImplementedException();
		}

		private IEnumerable<ICommandBuilder> GetCommandBuilders(object parameterSource, TypeMap typeMap)
		{
			throw new NotImplementedException();
		}
	}

	internal class CommandBuilder : ICommandBuilder
	{
		private readonly string _sql;

		public CommandBuilder(string sql)
		{
			_sql = sql;
		}

		public void AddParameters()
		{
			throw new NotImplementedException();
		}

		public void AddParametersAndWhereSql()
		{
			throw new NotImplementedException();
		}

		public IDatabaseCommand CreateCommand()
		{
			throw new NotImplementedException();
		}
	}

	internal interface ICommandBuilder
	{
		void AddParameters();
		void AddParametersAndWhereSql();
	}

	//internal class CommandBuilder
	//{
	//	public PropertyGetter Getter { get; set; }
	//	public IValueSerializer ValueSerializer { get; set; }
	//	public string ColumnName { get; set; }
	//	public IParameterBuilder ParameterBuilder { get; set; }
	//}

	internal interface IParameterFactory
	{
		void AddParameters(ICommandBuilder commandBuilder);
		void AddParametersAndWhereSql(ICommandBuilder commandBuilder);
	}

	internal class ParameterFactory : IParameterFactory
	{
		private readonly string _name;
		private readonly IParameterBuilder _parameterBuilder;

		public void AddParameters(ICommandBuilder commandBuilder)
		{
			throw new NotImplementedException();
		}

		public void AddParametersAndWhereSql(ICommandBuilder commandBuilder)
		{
			throw new NotImplementedException();
		}
	}

	internal interface IParameterBuilder
	{
		void AddParameters(ICommandBuilder commandBuilder, Column column, IValueSerializer valueSerializer = null);
		void AddParametersAndWhereSql(ICommandBuilder commandBuilder, Column column, IValueSerializer valueSerializer = null);
	}

	internal abstract class ParameterBuilder : IParameterBuilder
	{
		public virtual void AddParameters(ICommandBuilder commandBuilder, Column column, IValueSerializer valueSerializer = null)
		{
			var parameters = GetParameters(valueSerializer);
			commandBuilder.AddParameters();
		}

		private IEnumerable<IDataParameter> GetParameters(IValueSerializer valueSerializer = null)
		{
			throw new NotImplementedException();
		}

		public abstract void AddParametersAndWhereSql(ICommandBuilder commandBuilder, Column column, IValueSerializer valueSerializer = null)
		{
			AddParameters(commandBuilder, column, valueSerializer);
			commandBuilder.
		}
	}

	public class EqualToParameterBuilder : ParameterBuilder
	{ }

	internal class SelectSqlBuilder
	{
		private readonly string _sql;
		private readonly IList<string> _where;

		public SelectSqlBuilder(TypeMap typeMap)
		{
			var columns = string.Join(", ", typeMap.Properties.Select(x => "[" + x.ColumnName + "]"));
			_sql = string.Format("select {0} from [{1}].[{1}]", columns, typeMap.Namespace, typeMap.Table);
			_where = new List<string>();
		}

		public void Where(string condition)
		{

		}
	}

	internal interface ISqlGenerator
	{
		string GenerateSelect(TypeMap typeMap);
		string GenerateInsert(TypeMap typeMap);
		string GenerateUpdate(TypeMap typeMap);
		string GenerateDelete(TypeMap typeMap);
	}

	internal class SqlGenerator : ISqlGenerator
	{
		public string GenerateInsert(TypeMap typeMap)
		{
			var propertyMaps = typeMap.Properties;
			return string.Format("insert into {0} ({1}) values ({2})",
										GetSchemaAndTable(typeMap),
										string.Join(", ", propertyMaps.Select(x => Enclose(x.ColumnName))),
										string.Join(", ", propertyMaps.Select(x => "@" + x.ColumnName)));
		}

		public string GenerateUpdate(TypeMap typeMap)
		{
			var propertyMaps = typeMap.Properties;
			var keys = propertyMaps.Where(x => x.IsPrimaryKey);
			var values = propertyMaps.Where(x => !x.IsPrimaryKey);
			return string.Format("update {0} set {1} where {2}",
										GetSchemaAndTable(typeMap),
										GetColumnEqualsParameter(values),
										GetColumnEqualsParameter(keys));
		}

		public string GenerateDelete(TypeMap typeMap)
		{
			var propertyMaps = typeMap.Properties;
			var keys = propertyMaps.Where(x => x.IsPrimaryKey);
			return string.Format("delete from {0} where {1}",
										GetSchemaAndTable(typeMap),
										GetColumnEqualsParameter(keys));
		}

		private static string GetSchemaAndTable(TypeMap typeMap)
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
		public int Execute(IDbConnection connection)
		{
			var dbCommand = connection.CreateCommand();
			return dbCommand.ExecuteNonQuery();
		}
	}
}