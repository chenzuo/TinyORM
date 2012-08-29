using System;

namespace TinyORM
{
	public interface IDatabaseEngine : IDatabaseSession
	{
		void Execute(Action<IDatabaseSession> action);
	}
}