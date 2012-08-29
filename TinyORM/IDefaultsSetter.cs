using System;
using System.Data;

namespace TinyORM
{
	public interface IDefaultsSetter
	{
		void SetSqlDbType(Type type, SqlDbType sqlDbType);
		void SetMaxLength(SqlDbType sqlDbType, int maxLength);
		void SetPrecision(SqlDbType sqlDbType, byte precision);
		void SetScale(SqlDbType sqlDbType, byte scale);
		void SetParameterDefaults(SqlDbType sqlDbType, ParameterDefaults defaults);
		void SetSerializer(Type type, IValueSerializer serializer);
		void SetSerializer(Type type, SqlDbType sqlDbType, IValueSerializer serializer);
	}
}