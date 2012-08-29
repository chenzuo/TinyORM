using System;

namespace TinyORM
{
	public interface IInstanceFactory
	{
		bool CanHandle(Type type);
		object CreateInstance(Type type);
	}
}