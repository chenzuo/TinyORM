using System;
using System.Data;

namespace TinyORM
{
	public interface IColumnMap
	{
		string ColumnName { get; }
		SqlDbType ColumnType { get; }
		byte ColumnPrecision { get; }
		byte ColumnScale { get; }
		int ColumnMaxLength { get; }
		bool IsGenerated { get; }
		bool IsNullable { get; }
		bool IsPrimaryKey { get; }
		string PropertyName { get; }
		Type PropertyType { get; }
		IValueSerializer Serializer { get; }
	}

	public class ColumnMap : IColumnMap
	{
		public string ColumnName { get; set; }
		public SqlDbType ColumnType { get; set; }
		public byte ColumnPrecision { get; set; }
		public byte ColumnScale { get; set; }
		public int ColumnMaxLength { get; set; }
		public bool IsGenerated { get; set; }
		public bool IsNullable { get; set; }
		public bool IsPrimaryKey { get; set; }
		public string PropertyName { get; set; }
		public Type PropertyType { get; set; }
		public IValueSerializer Serializer { get; set; }
	}
}