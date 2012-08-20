
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
		IEnumerable<IDatabaseCommand> CreateInsertCommands<T>(IEnumerable<object> parameterSource);
		IEnumerable<IDatabaseCommand> CreateUpdateCommands<T>(IEnumerable<object> parameterSource);
		IEnumerable<IDatabaseCommand> CreateDeleteCommands<T>(IEnumerable<object> parameterSource);
		IEnumerable<IDatabaseCommand> CreateExecuteCommands(string sql, IEnumerable<object> parameterSources, Type type = null);
	}

	internal class CommandFactory : ICommandFactory
	{
		public IEnumerable<IDatabaseCommand> CreateInsertCommands<T>(IEnumerable<object> parameterSource)
		{
			foreach (var source in parameterSource)
			{
				var parameterFoos = GetParameterFoos<T>(source);
			}
		}

		private IEnumerable<object> GetParameterFoos<T>(object source)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IDatabaseCommand> CreateUpdateCommands<T>(IEnumerable<object> parameterSource)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IDatabaseCommand> CreateDeleteCommands<T>(IEnumerable<object> parameterSource)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IDatabaseCommand> CreateExecuteCommands(string sql, IEnumerable<object> parameterSources, Type type = null)
		{
			throw new NotImplementedException();
		}

		internal class ParameterInfo
		{
			public PropertyGetter Getter { get; set; }
			public IValueSerializer ValueSerializer { get; set; }
			public string ColumnName { get; set; }
			public ICommandBuilder CommandBuilder { get; set; }
		}
	}

	internal interface IDatabaseCommand
	{
	}
}