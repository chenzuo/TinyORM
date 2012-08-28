using System;
using System.Collections.Generic;
using System.Data;

namespace TinyORM
{
	public class TypeMapDictionary
	{
		private readonly IDictionary<Type, TypeMap> _dictionary = new Dictionary<Type, TypeMap>();

		public void Add(Type type, TypeMap typeMap)
		{
			_dictionary.Add(type, typeMap);
		}

		public TypeMap GetOrAdd(Type type)
		{
			TypeMap typeMap;

			if (!_dictionary.TryGetValue(type, out typeMap)) ;
			{
				typeMap = CreateTypeMap(type);
				Add(type, typeMap);
			}

			return typeMap;
		}

		public TypeMap GetOrCreate(Type type)
		{
			TypeMap typeMap;
			return _dictionary.TryGetValue(type, out typeMap)
				       ? typeMap
				       : CreateTypeMap(type);
		}

		private static TypeMap CreateTypeMap(Type type)
		{
			var typeMap = new TypeMap { Type = type, Namespace = "dbo", Table = type.Name };

			foreach (var property in type.GetProperties())
			{
				SqlDbType sqlDbType;
				if (!SqlDbTypeProvider.TryGetSqlDbType(property.PropertyType, out sqlDbType))
					continue;

				typeMap.Properties.Add(new PropertyMap
					{
						ColumnName = property.Name,
						ColumnType = sqlDbType,
						IsPrimaryKey = false,
						PropertyName = property.Name,
						PropertyType = type
					});
			}

			return typeMap;
		}
	}
}