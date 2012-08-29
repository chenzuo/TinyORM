using System;
using System.Data;

namespace TinyORM
{
	internal class DataEntityTupleMapper : IInstanceMapper
	{
		private readonly IInstanceMapper _dynamicMapper;
		private readonly IInstanceMapper _entityMapper;

		public DataEntityTupleMapper(IInstanceMapper dynamicMapper, IInstanceMapper entityMapper)
		{
			_dynamicMapper = dynamicMapper;
			_entityMapper = entityMapper;
		}

		public object Map(IDataRecord record, Type type)
		{
			var genericType = typeof(DataEntityTuple<>).MakeGenericType(type);
			var data = _dynamicMapper.Map(record, null);
			var entity = _entityMapper.Map(record, type);
			return Activator.CreateInstance(genericType, data, entity);
		}
	}
}