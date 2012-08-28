using System;
using System.Collections.Generic;
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
			var expression = new TypeMapBuilder<T>();
			action(expression);
			var typeMap = ((ITypeMapProvider)expression).GetTypeMap();
			AddMaps(new[] { typeMap });
		}

		public void AddInstanceFactory(Predicate<Type> predicate, IInstanceFactory instanceFactory)
		{
			_configuration.InstanceFactoryProviders.Add(predicate, instanceFactory);
		}

		public void AddSaveOperationProvider(Predicate<Type> predicate, ISaveOperationProvider saveOperationProvider)
		{
			_configuration.SaveOperationProviders.Add(predicate, saveOperationProvider);
		}

		public void ConnectionStringProvider(IConnectionStringProvider connectionStringProvider)
		{
			_configuration.ConnectionStringProvider = connectionStringProvider;
		}

		public void AddMapsFromAssemblyContaining(Type referenceType)
		{
			var typeMaps = from type in referenceType.Assembly.GetTypes()
								where !type.IsAbstract && typeof(ITypeMapProvider).IsAssignableFrom(type)
								let typeMapProvider = (ITypeMapProvider)Activator.CreateInstance(type)
								select typeMapProvider.GetTypeMap();
			AddMaps(typeMaps.ToList());
		}

		public void AddMapsFromAssemblyContaining<T>()
		{
			AddMapsFromAssemblyContaining(typeof(T));
		}

		public void AddMapsFromAssemblyContainingTypeOf(object o)
		{
			AddMapsFromAssemblyContaining(o.GetType());
		}

		private void AddMaps(IEnumerable<TypeMap> typeMaps)
		{
			lock (_configuration)
				foreach (var typeMap in typeMaps)
					_configuration.Types.Add(typeMap.Type, typeMap);
		}
	}
}