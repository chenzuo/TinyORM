using System;
using System.Data;

namespace TinyORM
{
	public interface IDefaultsGetter
	{
		SqlDbType GetSqlDbType(Type type);
		bool TryGetSqlDbType(Type type, out SqlDbType sqlDbType);
		ParameterDefaults GetParameterDefaults(SqlDbType sqlDbType);
		IValueSerializer GetSerializerOrNull(Type type, SqlDbType? sqlDbType = null);
		bool TryGetSerializer(Type type, out IValueSerializer serializer);
		bool IsNullable(Type type, SqlDbType? sqlDbType = null);
	}
}