using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;

namespace TinyORM
{
	internal class SqlDbTypeProvider
	{
		private static readonly Dictionary<Type, SqlDbType> Map = new Dictionary<Type, SqlDbType>
			{
				{typeof (bool), SqlDbType.Bit},
				{typeof (byte), SqlDbType.TinyInt},
				{typeof (byte[]), SqlDbType.VarBinary},
				{typeof (DateTime), SqlDbType.DateTime},
				{typeof (DateTimeOffset), SqlDbType.DateTimeOffset},
				{typeof (decimal), SqlDbType.Decimal},
				{typeof (double), SqlDbType.Float},
				{typeof (float), SqlDbType.Real},
				{typeof (Guid), SqlDbType.UniqueIdentifier},
				{typeof (int), SqlDbType.Int},
				{typeof (long), SqlDbType.BigInt},
				{typeof (short), SqlDbType.SmallInt},
				{typeof (string), SqlDbType.NVarChar},
				{typeof (TimeSpan), SqlDbType.Time},
				{typeof (XElement), SqlDbType.Xml}
			};

		public static SqlDbType GetSqlDbType(Type type)
		{
			SqlDbType sqlDbType;
			if (TryGetSqlDbType(type, out sqlDbType))
				return sqlDbType;

			throw new ArgumentException();
		}

		public static bool TryGetSqlDbType(Type type, out SqlDbType sqlDbType)
		{
			if (Map.TryGetValue(type, out sqlDbType))
				return true;

			if (type.IsEnum)
				return TryGetSqlDbType(type.GetEnumUnderlyingType(), out sqlDbType);

			if (type.IsValueType && type.IsGenericType && typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
				return TryGetSqlDbType(type.GetGenericArguments()[0], out sqlDbType);

			return false;
		}

		public static int? GetDefaultScale(SqlDbType sqlDbType)
		{
			throw new NotImplementedException();
		}

		public static int? GetDefaultPrecision(SqlDbType sqlDbType)
		{
			throw new NotImplementedException();
		}

		public static int? GetDefaultMax(SqlDbType sqlDbType)
		{
			throw new NotImplementedException();
		}
	}
}