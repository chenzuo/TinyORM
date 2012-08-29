using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace TinyORM
{
	public class TableMapDictionary
	{
		private readonly IDictionary<Type, TableMap> _dictionary = new Dictionary<Type, TableMap>();

		public void Add(Type type, TableMap tableMap)
		{
			_dictionary.Add(type, tableMap);
		}

		public TableMap GetOrCreate(Type type, IDefaultsGetter defaults)
		{
			TableMap tableMap;
			return _dictionary.TryGetValue(type, out tableMap)
						 ? tableMap
						 : Create(type, defaults);
		}

		private static TableMap Create(Type type, IDefaultsGetter defaults)
		{
			var sqlDbType = default(SqlDbType);
			var columns = from property in type.GetProperties()
			              let propertyType = property.PropertyType
			              where defaults.TryGetSqlDbType(propertyType, out sqlDbType)
							  let parameterDefaults = defaults.GetParameterDefaults(sqlDbType)
							  select new ColumnMap
								  {
									  ColumnMaxLength = parameterDefaults.MaxLength,
									  ColumnName = property.Name,
									  ColumnPrecision = parameterDefaults.Precision,
									  ColumnScale = parameterDefaults.Scale,
									  ColumnType = sqlDbType,
									  IsGenerated = false,
									  IsNullable = false,
									  IsPrimaryKey = false,
									  PropertyName = property.Name,
									  PropertyType = propertyType,
									  Serializer = defaults.GetSerializerOrNull(propertyType, sqlDbType)
								  };
			return new TableMap
				{
					Columns = columns.ToList(),
					Schema = "dbo",
					Table = type.Name,
					Type = type
				};
		}
	}
}