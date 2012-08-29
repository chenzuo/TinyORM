using System;

namespace TinyORM
{
	public interface IDatabase : IDatabaseEngine
	{
		void Configure(Action<DatabaseConfigurationBuilder> action);
		void ValidateConfiguration();
	}
}