using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyORM
{
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
}