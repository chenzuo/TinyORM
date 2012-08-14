using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace TinyORM
{
	public class InstanceMapperProvider
	{
		// TODO Make this class thread safe

		private readonly DatabaseConfiguration _configuration;
		private readonly Dictionary<string, IInstanceMapper> _cache = new Dictionary<string, IInstanceMapper>();

		public InstanceMapperProvider(DatabaseConfiguration configuration)
		{
			_configuration = configuration;
		}

		public IInstanceMapper GetMapper(Type targetType, IEnumerable<DataColumnInfo> schema)
		{
			schema = schema.ToList();
			var key = CreateKey(targetType, schema);
			IInstanceMapper mapper;

			if (!_cache.TryGetValue(key, out mapper))
			{
				var typeMap = _configuration.TypeMaps.GetOrCreate(targetType);
				mapper = CreateMapper(typeMap, schema);
				_cache.Add(key, mapper);
			}

			return mapper;
		}

		private static string CreateKey(Type targetType, IEnumerable<DataColumnInfo> schema)
		{
			var strings = schema.Select(x => string.Format("{0}:{1}", x.Index, x.Name.ToLower()));
			return string.Format("{0}<{1}", targetType.FullName, string.Join(";", strings));
		}

		private IInstanceMapper CreateMapper(TypeMap typeMap, IEnumerable<DataColumnInfo> schema)
		{
			var targetType = typeMap.Type;
			if (targetType == typeof(ExpandoObject))
				return new ExpandoObjectMapper();

			if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(DataEntityTuple<>))
				return CreateDataEntityTupleMapper(typeMap, schema);

			return CreateObjectMapper(typeMap, schema);
		}

		private IInstanceMapper CreateDataEntityTupleMapper(TypeMap typeMap, IEnumerable<DataColumnInfo> schema)
		{
			var expandoObjectMapper = new ExpandoObjectMapper();
			var objectMapper = CreateObjectMapper(typeMap, schema);
			return new DataEntityTupleMapper(expandoObjectMapper, objectMapper);
		}

		private ObjectMapper CreateObjectMapper(TypeMap typeMap, IEnumerable<DataColumnInfo> schema)
		{
			var instanceFactory = _configuration.InstanceFactoryProviders
				.Where(x => x.CanHandle(typeMap.Type))
				.Select(x => x.InstanceFactory)
				.FirstOrDefault();
			var mapper = new ObjectMapper(instanceFactory ?? new DefaultInstanceFactory());
			mapper.Initialize(typeMap, schema);
			return mapper;
		}
	}

	public class DataColumnInfo
	{
		public int Index { get; set; }
		public string Name { get; set; }
	}
}