using System;
using System.Collections.Generic;

namespace TinyORM
{
	internal interface ICommandFactory
	{
		IQueryCommand CreateSelectCommand(object parameterSource, string sql = null, Type mapType = null);
		IEnumerable<IExecuteCommand> CreateInsertCommands(IEnumerable<object> parameterSources, Type mapType);
		IEnumerable<IExecuteCommand> CreateUpdateCommands(IEnumerable<object> parameterSources, Type mapType);
		IEnumerable<IExecuteCommand> CreateDeleteCommands(IEnumerable<object> parameterSources, Type mapType);
		IEnumerable<IExecuteCommand> CreateExecuteCommands(string sql, IEnumerable<object> parameterSources, Type mapType = null);
		IExecuteCommand CreateExecuteCommand(string sql, object parameterSource, Type mapType = null);
	}
}