using System.Data;

namespace TinyORM
{
	internal interface IExecuteCommand
	{
		int Execute(IDbConnection connection);
		object ExecuteScalar(IDbConnection connection);
	}
}