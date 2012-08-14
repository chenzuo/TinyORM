using System;

namespace TinyORM
{
	public class DefaultInstanceFactory : IInstanceFactory
	{
		public object CreateInstance(Type type)
		{
			return Activator.CreateInstance(type);
		}
	}
}