using System;
using System.Collections.Generic;

namespace TinyORM
{
	public interface ITableMap
	{
		Type Type { get; }
		string Table { get; }
		string Schema { get; }
		IEnumerable<IColumnMap> Columns { get; }
	}
}