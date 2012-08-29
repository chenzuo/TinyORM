using System.Collections.Generic;
using System.Linq;

namespace TinyORM
{
	internal class ParameterBuilderFactory
	{
		private readonly ITableMap _tableMap;

		public ParameterBuilderFactory(TableMap tableMap)
		{
			_tableMap = tableMap;
		}

		public IEnumerable<IParameterBuilder> GetCommandBuilders(object source)
		{
			return source.GetType() == _tableMap.Type
						 ? GetFromSourceMatchingTypeMap(source)
						 : GetFromAdHocSource(source);
		}

		private IEnumerable<IParameterBuilder> GetFromSourceMatchingTypeMap(object source)
		{
			return _tableMap.Columns.Select(x => CreateParameterBuilder(source, x));
		}

		private IEnumerable<IParameterBuilder> GetFromAdHocSource(object source)
		{
			var sourceType = source.GetType();
			var propertyMaps = _tableMap.Columns.ToDictionary(x => x.PropertyName.Replace(".", "").ToLower());

			foreach (var property in sourceType.GetProperties())
			{
				if (property.PropertyType == _tableMap.Type)
				{
					var getter = PropertyGetter.Create(sourceType, property.Name);
					foreach (var commandBuilder in GetFromSourceMatchingTypeMap(getter.Get(source)))
						yield return commandBuilder;
					continue;
				}

				IColumnMap columnMap;
				if (!propertyMaps.TryGetValue(property.Name.ToLower(), out columnMap))
					continue;

				yield return CreateParameterBuilder(source, columnMap);
			}
		}

		private IParameterBuilder CreateParameterBuilder(object source, IColumnMap columnMap)
		{
			var value = PropertyGetter.Create(source.GetType(), columnMap.PropertyName).Get(source);
			var parameterFactory = value as IParameterFactory ?? new SingleParameterFactory("=", value);
			return new ParameterBuilder(columnMap, parameterFactory);
		}
	}
}