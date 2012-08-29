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

		public IDefaultsSetter Defaults
		{
			get { return _configuration.Defaults; }
		}

		public void Map<T>(Action<TypeMapBuilder<T>> action)
		{
			var expression = new TypeMapBuilder<T>();
			action(expression);
			var typeMap = ((ITypeMapProvider)expression).GetTypeMap(_configuration.Defaults);
			AddMaps(new[] { typeMap });
		}

		public void AddInstanceFactory(IInstanceFactory instanceFactory)
		{
			_configuration.InstanceFactoryProviders.Add(instanceFactory);
		}

		public void AddSaveOperationProvider(ISaveOperationProvider saveOperationProvider)
		{
			_configuration.SaveOperationProviders.Add(saveOperationProvider);
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
								select typeMapProvider.GetTypeMap(_configuration.Defaults);
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

		private void AddMaps(IEnumerable<TableMap> typeMaps)
		{
			lock (_configuration)
				foreach (var typeMap in typeMaps)
					_configuration.Tables.Add(typeMap.Type, typeMap);
		}
	}
}