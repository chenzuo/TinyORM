using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace TinyORM
{
	internal class InstanceMapperProvider : IInstanceMapperProvider
	{
		private readonly MapConfiguration _configuration;
		private readonly ConcurrentDictionary<string, IInstanceMapper> _cache = new ConcurrentDictionary<string, IInstanceMapper>();

		public InstanceMapperProvider(MapConfiguration configuration)
		{
			_configuration = configuration;
		}

		public IInstanceMapper GetMapper(Type targetType, IEnumerable<DataColumnInfo> schema)
		{
			schema = schema.ToList();
			var key = CreateKey(targetType, schema);
			return _cache.GetOrAdd(key, _ => CreateMapper(_configuration.Tables.GetOrCreate(targetType, _configuration.Defaults), schema));
		}

		private static string CreateKey(Type targetType, IEnumerable<DataColumnInfo> schema)
		{
			var strings = schema.Select(x => string.Format("{0}:{1}", x.Index, x.Name.ToLower()));
			return string.Format("{0}<{1}", targetType.FullName, string.Join(";", strings));
		}

		private IInstanceMapper CreateMapper(TableMap tableMap, IEnumerable<DataColumnInfo> schema)
		{
			var targetType = tableMap.Type;
			if (targetType == typeof(ExpandoObject))
				return new ExpandoObjectMapper();

			if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(DataEntityTuple<>))
				return CreateDataEntityTupleMapper(tableMap, schema);

			return CreateObjectMapper(tableMap, schema);
		}

		private IInstanceMapper CreateDataEntityTupleMapper(TableMap tableMap, IEnumerable<DataColumnInfo> schema)
		{
			var expandoObjectMapper = new ExpandoObjectMapper();
			var objectMapper = CreateObjectMapper(tableMap, schema);
			return new DataEntityTupleMapper(expandoObjectMapper, objectMapper);
		}

		private ObjectMapper CreateObjectMapper(TableMap tableMap, IEnumerable<DataColumnInfo> schema)
		{
			var instanceFactory = _configuration.InstanceFactoryProviders.FirstOrDefault(x => x.CanHandle(tableMap.Type)) ??
										 new DefaultInstanceFactory();
			var mapper = new ObjectMapper(instanceFactory);
			mapper.Initialize(tableMap, schema);
			return mapper;
		}
	}
}