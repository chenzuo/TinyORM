using System;
using System.Data;

namespace TinyORM
{
	public interface IPropertyMap
	{
		string ColumnName { get; set; }
		SqlDbType ColumnType { get; set; }
		bool IsPrimaryKey { get; set; }
		string PropertyName { get; set; }
		Type PropertyType { get; set; }
		IValueSerializer ValueSerializer { get; set; }
	}

	public class PropertyMap : IPropertyMap
	{
		public string ColumnName { get; set; }
		public SqlDbType ColumnType { get; set; }
		public bool IsPrimaryKey { get; set; }
		public string PropertyName { get; set; }
		public Type PropertyType { get; set; }
		public IValueSerializer ValueSerializer { get; set; }
	}
}