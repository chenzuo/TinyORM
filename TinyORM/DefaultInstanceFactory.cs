using System;

namespace TinyORM
{
	public class DefaultInstanceFactory : IInstanceFactory
	{
		public bool CanHandle(Type type)
		{
			throw new NotImplementedException();
		}

		public object CreateInstance(Type type)
		{
			return Activator.CreateInstance(type);
		}
	}
}