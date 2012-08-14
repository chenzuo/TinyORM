using System;
using System.Data;

namespace TinyORM
{
	public class PropertyMap
	{
		public string ColumnName { get; set; }
		public SqlDbType ColumnType { get; set; }
		public bool IsPrimaryKey { get; set; }
		public string PropertyName { get; set; }
		public Type PropertyType { get; set; }
		public IValueSerializer ValueSerializer { get; set; }
	}
}