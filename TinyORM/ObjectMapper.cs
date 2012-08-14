using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace TinyORM
{
	public class ObjectMapper : IInstanceMapper
	{
		private readonly IInstanceFactory _instanceFactory;
		private readonly List<DataRecordToPropertyMapper> _recordToPropertyMappers;

		public ObjectMapper(IInstanceFactory instanceFactory)
		{
			_instanceFactory = instanceFactory;
			_recordToPropertyMappers = new List<DataRecordToPropertyMapper>();
		}

		public void Initialize(TypeMap typeMap, IEnumerable<DataColumnInfo> schema)
		{
			var dictionary = schema.ToDictionary(x => x.Name.ToLower(), x => x.Index);
			foreach (var propertyMap in typeMap.Properties)
			{
				int index;
				if (!dictionary.TryGetValue(propertyMap.ColumnName.ToLower(), out index)) continue;
				_recordToPropertyMappers.Add(new DataRecordToPropertyMapper
					{
						Index = index,
						PropertySetter = PropertySetter.Create(typeMap.Type, propertyMap.PropertyName),
						Serializer = propertyMap.ValueSerializer
					});
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
			public int Index { get; set; }
			public PropertySetter PropertySetter { get; set; }
			public IValueSerializer Serializer { get; set; }

			public void Execute(IDataRecord record, object target)
			{
				var value = record.IsDBNull(Index) ? null : record.GetValue(Index);

				if (Serializer != null)
					value = Serializer.Deserialize(value);

				PropertySetter.Set(target, value);
			}
		}
	}
}