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
}