using System;
using System.Collections.Generic;

namespace TinyORM
{
	public class TableMap : ITableMap
	{
		public TableMap()
		{
			Columns = new List<ColumnMap>();
		}

		public Type Type { get; set; }
		public string Table { get; set; }
		public string Schema { get; set; }
		public IEnumerable<IColumnMap> Columns { get; set; }
	}
}