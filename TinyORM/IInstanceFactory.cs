using System;

namespace TinyORM
{
	public interface IInstanceFactory
	{
		object CreateInstance(Type type);
	}
}