using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace TinyORM
{
	internal class ObjectMapper : IInstanceMapper
	{
		private readonly IInstanceFactory _instanceFactory;
		private readonly List<DataRecordToPropertyMapper> _recordToPropertyMappers;

		public ObjectMapper(IInstanceFactory instanceFactory)
		{
			_instanceFactory = instanceFactory;
			_recordToPropertyMappers = new List<DataRecordToPropertyMapper>();
		}

		public void Initialize(TableMap tableMap, IEnumerable<DataColumnInfo> schema)
		{
			var dictionary = schema.ToDictionary(x => x.Name.ToLower(), x => x.Index);
			foreach (var propertyMap in tableMap.Columns)
			{
				int index;
				if (!dictionary.TryGetValue(propertyMap.ColumnName.ToLower(), out index)) continue;
				var setter = PropertySetter.Create(tableMap.Type, propertyMap.PropertyName);
				_recordToPropertyMappers.Add(new DataRecordToPropertyMapper(index, setter, propertyMap.Serializer));
			}
		}

		public object Map(IDataRecord record, Type type)
		{
			var instance = _instanceFactory.CreateInstance(type);
			foreach (var propertySetter in _recordToPropertyMappers)
				propertySetter.Execute(record, instance);
			return instance;
		}

		internal class DataRecordToPropertyMapper
		{
			private readonly int _index;
			private readonly PropertySetter _setter;
			private readonly IValueSerializer _serializer;

			public DataRecordToPropertyMapper(int index, PropertySetter setter, IValueSerializer serializer = null)
			{
				_index = index;
				_setter = setter;
				_serializer = serializer;
			}

			public void Execute(IDataRecord record, object target)
			{
				var value = record.GetValue(_index);

				if (value == DBNull.Value)
					value = null;

				if (_serializer != null)
					value = _serializer.Deserialize(value);

				_setter.Set(target, value);
			}
		}
	}
}