using System;

namespace TinyORM
{
	public interface ISaveOperationProvider
	{
		bool CanHandle(Type type);
		SaveOperation GetSaveOperation(object instance);
	}
}