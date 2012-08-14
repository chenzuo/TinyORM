using System;

namespace TinyORM
{
	public interface IDatabase : IDatabaseSession
	{
		void Configure(Action<DatabaseConfigurationBuilder> action);
		//void Execute(Action<IDatabaseSession> action);
	}
}