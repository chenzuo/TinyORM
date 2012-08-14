using System;

namespace TinyORM
{
	public class InstanceFactoryProvider
	{
		public InstanceFactoryProvider(Predicate<Type> canHandle, IInstanceFactory instanceFactory)
		{
			CanHandle = canHandle;
			InstanceFactory = instanceFactory;
		}

		public Predicate<Type> CanHandle { get; private set; }
		public IInstanceFactory InstanceFactory { get; private set; }
	}
}