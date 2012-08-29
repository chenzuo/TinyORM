using System.Collections.Generic;
using System.Linq;

namespace TinyORM
{
	internal class SqlBuilder : ISqlBuilder
	{
		public string GetSelect(ITableMap tableMap, SqlBuilderColumns columns, string whereStatement)
		{
			var sql = string.Format("select {0} from {1}", GetColumns(tableMap), GetSchemaAndTable(tableMap));
			return string.IsNullOrWhiteSpace(whereStatement)
						 ? sql
						 : sql + " where " + whereStatement;
		}

		public string GetInsert(ITableMap tableMap)
		{
			var propertyMaps = tableMap.Columns;
			return string.Format("insert {0} ({1}) values ({2})",
										GetSchemaAndTable(tableMap),
										GetColumns(tableMap),
										string.Join(", ", propertyMaps.Select(GetParameter)));
		}

		public string GetUpdate(ITableMap tableMap)
		{
			var propertyMaps = tableMap.Columns;
			var keys = propertyMaps.Where(x => x.IsPrimaryKey);
			var values = propertyMaps.Where(x => !x.IsPrimaryKey);
			return string.Format("update {0} set {1} where {2}",
										GetSchemaAndTable(tableMap),
										GetColumnEqualsParameter(", ", values),
										GetColumnEqualsParameter(" and ", keys));
		}

		public string GetDelete(ITableMap tableMap)
		{
			var propertyMaps = tableMap.Columns;
			var keys = propertyMaps.Where(x => x.IsPrimaryKey);
			return string.Format("delete {0} where {1}",
										GetSchemaAndTable(tableMap),
										GetColumnEqualsParameter(" and ", keys));
		}

		private static string GetSchemaAndTable(ITableMap tableMap)
		{
			return string.Format("{0}.{1}", Enclose(tableMap.Schema), Enclose(tableMap.Table));
		}

		private static string GetColumns(ITableMap tableMap)
		{
			return string.Join(", ", tableMap.Columns.Select(x => Enclose(x.ColumnName)));
		}

		private static string GetColumnEqualsParameter(string separator, IEnumerable<IColumnMap> propertyMaps)
		{
			return string.Join(separator, propertyMaps.Select(x => string.Format("{0} = {1}", Enclose(x.ColumnName), GetParameter(x))));
		}

		private static string GetParameter(IColumnMap columnMap)
		{
			return "@" + columnMap.ColumnName;
		}

		private static string Enclose(string item)
		{
			return "[" + item + "]";
		}
	}
}