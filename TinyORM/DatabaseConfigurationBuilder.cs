using System;
using System.Linq;

namespace TinyORM
{
	public class DatabaseConfigurationBuilder
	{
		private readonly DatabaseConfiguration _configuration;

		public DatabaseConfigurationBuilder(DatabaseConfiguration configuration)
		{
			_configuration = configuration;
		}

		public void Map<T>(Action<TypeMapBuilder<T>> action)
		{
			lock (_configuration)
			{
				var expression = new TypeMapBuilder<T>();
				action(expression);
				var typeMap = ((ITypeMapProvider)expression).GetTypeMap();
				_configuration.TypeMaps.Add(typeMap.Type, typeMap);
			}
		}

		public void AddInstanceFactory(Predicate<Type> predicate, IInstanceFactory instanceFactory)
		{
			var provider = new InstanceFactoryProvider(predicate, instanceFactory);
			_configuration.InstanceFactoryProviders.Add(provider);
		}

		public void ConnectionStringProvider(IConnectionStringProvider connectionStringProvider)
		{
			_configuration.ConnectionStringProvider = connectionStringProvider;
		}

		public void AddMapsFromAssemblyContaining(Type type)
		{
			foreach (var x in type.Assembly.GetTypes().Where(x => typeof(ITypeMapProvider).IsAssignableFrom(x)))
			{
				var typeMapProvider = (ITypeMapProvider)Activator.CreateInstance(x);
				var typeMap = typeMapProvider.GetTypeMap();
				_configuration.TypeMaps.Add(typeMap.Type, typeMap);
			}
		}

		public void AddMapsFromAssemblyContaining<T>()
		{
			AddMapsFromAssemblyContaining(typeof(T));
		}

		public void AddMapsFromAssemblyContainingTypeOf(object o)
		{
			AddMapsFromAssemblyContaining(o.GetType());
		}
	}
}