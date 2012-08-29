using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;

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

	public interface IDefaults : IDefaultsGetter, IDefaultsSetter
	{
	}

	internal class Defaults : IDefaults
	{
		private struct Types : IEquatable<Types>
		{
			public Types(Type type, SqlDbType sqlDbType)
				: this()
			{
				Type = type;
				SqlDbType = sqlDbType;
			}

			public Type Type { get; private set; }
			public SqlDbType SqlDbType { get; private set; }

			public bool Equals(Types other)
			{
				return Type == other.Type && SqlDbType == other.SqlDbType;
			}
		}

		private readonly Dictionary<Type, SqlDbType> _sqlDbTypes = new Dictionary<Type, SqlDbType>();
		private readonly Dictionary<SqlDbType, ParameterDefaults> _parameterDefaults = new Dictionary<SqlDbType, ParameterDefaults>();
		private readonly ConcurrentDictionary<Types, IValueSerializer> _serializers = new ConcurrentDictionary<Types, IValueSerializer>();

		public Defaults()
		{
			SetSqlDbType(typeof(bool), SqlDbType.Bit);
			SetSqlDbType(typeof(byte), SqlDbType.TinyInt);
			SetSqlDbType(typeof(byte[]), SqlDbType.VarBinary);
			SetSqlDbType(typeof(DateTime), SqlDbType.DateTime2);
			SetSqlDbType(typeof(DateTimeOffset), SqlDbType.DateTimeOffset);
			SetSqlDbType(typeof(decimal), SqlDbType.Decimal);
			SetSqlDbType(typeof(double), SqlDbType.Float);
			SetSqlDbType(typeof(float), SqlDbType.Real);
			SetSqlDbType(typeof(Guid), SqlDbType.UniqueIdentifier);
			SetSqlDbType(typeof(int), SqlDbType.Int);
			SetSqlDbType(typeof(long), SqlDbType.BigInt);
			SetSqlDbType(typeof(short), SqlDbType.SmallInt);
			SetSqlDbType(typeof(string), SqlDbType.NVarChar);
			SetSqlDbType(typeof(TimeSpan), SqlDbType.Time);
			SetSqlDbType(typeof(XElement), SqlDbType.Xml);

			SetParameterDefaults(SqlDbType.BigInt, new ParameterDefaults { MaxLength = 8, Precision = 19 });
			SetParameterDefaults(SqlDbType.Bit, new ParameterDefaults { MaxLength = 1, Precision = 1 });
			SetParameterDefaults(SqlDbType.DateTime2, new ParameterDefaults { MaxLength = 8, Precision = 27, Scale = 7 });
			SetParameterDefaults(SqlDbType.DateTimeOffset, new ParameterDefaults { MaxLength = 10, Precision = 34, Scale = 7 });
			SetParameterDefaults(SqlDbType.Decimal, new ParameterDefaults { MaxLength = 17, Precision = 38, Scale = 38 });
			SetParameterDefaults(SqlDbType.Float, new ParameterDefaults { MaxLength = 8, Precision = 53 });
			SetParameterDefaults(SqlDbType.Int, new ParameterDefaults { MaxLength = 4, Precision = 10 });
			SetParameterDefaults(SqlDbType.NVarChar, new ParameterDefaults { MaxLength = -1 });
			SetParameterDefaults(SqlDbType.Real, new ParameterDefaults { MaxLength = 4, Precision = 24 });
			SetParameterDefaults(SqlDbType.SmallInt, new ParameterDefaults { MaxLength = 2, Precision = 5 });
			SetParameterDefaults(SqlDbType.Time, new ParameterDefaults { MaxLength = 5, Precision = 16, Scale = 7 });
			SetParameterDefaults(SqlDbType.TinyInt, new ParameterDefaults { MaxLength = 1, Precision = 3 });
			SetParameterDefaults(SqlDbType.UniqueIdentifier, new ParameterDefaults { MaxLength = 16 });
			SetParameterDefaults(SqlDbType.VarBinary, new ParameterDefaults { MaxLength = -1 });
			SetParameterDefaults(SqlDbType.Xml, new ParameterDefaults { MaxLength = -1 });

			SetSerializer(typeof(XElement), new XmlValueSerializer());
		}

		public SqlDbType GetSqlDbType(Type type)
		{
			SqlDbType sqlDbType;
			if (TryGetSqlDbType(type, out sqlDbType))
				return sqlDbType;

			throw new ArgumentException();
		}

		public bool TryGetSqlDbType(Type type, out SqlDbType sqlDbType)
		{
			if (_sqlDbTypes.TryGetValue(type, out sqlDbType))
				return true;

			if (type.IsEnum)
				return TryGetSqlDbType(type.GetEnumUnderlyingType(), out sqlDbType);

			if (type.IsValueType && type.IsGenericType && typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
				return TryGetSqlDbType(type.GetGenericArguments()[0], out sqlDbType);

			return false;
		}

		public void SetSqlDbType(Type type, SqlDbType sqlDbType)
		{
			_sqlDbTypes[type] = sqlDbType;
		}

		public ParameterDefaults GetParameterDefaults(SqlDbType sqlDbType)
		{
			ParameterDefaults value;
			_parameterDefaults.TryGetValue(sqlDbType, out value);
			return value;
		}

		public void SetMaxLength(SqlDbType sqlDbType, int maxLength)
		{
			var defaults = GetParameterDefaults(sqlDbType);
			SetParameterDefaults(sqlDbType,
										new ParameterDefaults
											{
												MaxLength = maxLength,
												Precision = defaults.Precision,
												Scale = defaults.Scale
											});
		}

		public void SetPrecision(SqlDbType sqlDbType, byte precision)
		{
			var defaults = GetParameterDefaults(sqlDbType);
			SetParameterDefaults(sqlDbType,
										new ParameterDefaults
											{
												MaxLength = defaults.MaxLength,
												Precision = precision,
												Scale = defaults.Scale
											});
		}

		public void SetScale(SqlDbType sqlDbType, byte scale)
		{
			var defaults = GetParameterDefaults(sqlDbType);
			SetParameterDefaults(sqlDbType,
										new ParameterDefaults
											{
												MaxLength = defaults.MaxLength,
												Precision = defaults.Precision,
												Scale = scale
											});
		}

		public void SetParameterDefaults(SqlDbType sqlDbType, ParameterDefaults defaults)
		{
			_parameterDefaults[sqlDbType] = defaults;
		}

		public IValueSerializer GetSerializerOrNull(Type type, SqlDbType? sqlDbType = null)
		{
			var types = new Types(type, sqlDbType ?? GetSqlDbType(type));

			IValueSerializer serializer;
			if (_serializers.TryGetValue(types, out serializer))
				return serializer;

			if (type.IsEnum && GetSqlDbType(Enum.GetUnderlyingType(type)) == sqlDbType)
				return _serializers.GetOrAdd(types, CreateEnumValueSerializer(type));

			if (type.IsValueType && type.IsGenericType && typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
				return GetSerializerOrNull(type.GetGenericArguments()[0]);

			return null;
		}

		public bool TryGetSerializer(Type type, out IValueSerializer serializer)
		{
			serializer = GetSerializerOrNull(type);
			return serializer != null;
		}

		public bool IsNullable(Type type, SqlDbType? sqlDbType = null)
		{
			return type.IsValueType && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		private static IValueSerializer CreateEnumValueSerializer(Type type)
		{
			var serializerType = typeof(EnumValueSerializer<>).MakeGenericType(type);
			var instance = Activator.CreateInstance(serializerType);
			return (IValueSerializer)instance;
		}

		public void SetSerializer(Type type, IValueSerializer serializer)
		{
			var sqlDbType = GetSqlDbType(type);
			SetSerializer(type, sqlDbType, serializer);
		}

		public void SetSerializer(Type type, SqlDbType sqlDbType, IValueSerializer serializer)
		{
			_serializers[new Types(type, sqlDbType)] = serializer;
		}
	}

	public struct ParameterDefaults
	{
		public int MaxLength;
		public byte Precision;
		public byte Scale;
	}

	//internal class SqlDbTypeProvider
	//{
	//	private static readonly Dictionary<Type, SqlDbType> Map = new Dictionary<Type, SqlDbType>
	//		{
	//			{typeof (bool), SqlDbType.Bit},
	//			{typeof (byte), SqlDbType.TinyInt},
	//			{typeof (byte[]), SqlDbType.VarBinary},
	//			{typeof (DateTime), SqlDbType.DateTime2},
	//			{typeof (DateTimeOffset), SqlDbType.DateTimeOffset},
	//			{typeof (decimal), SqlDbType.Decimal},
	//			{typeof (double), SqlDbType.Float},
	//			{typeof (float), SqlDbType.Real},
	//			{typeof (Guid), SqlDbType.UniqueIdentifier},
	//			{typeof (int), SqlDbType.Int},
	//			{typeof (long), SqlDbType.BigInt},
	//			{typeof (short), SqlDbType.SmallInt},
	//			{typeof (string), SqlDbType.NVarChar},
	//			{typeof (TimeSpan), SqlDbType.Time},
	//			{typeof (XElement), SqlDbType.Xml}
	//		};

	//	public static SqlDbType GetSqlDbType(Type type)
	//	{
	//		SqlDbType sqlDbType;
	//		if (TryGetSqlDbType(type, out sqlDbType))
	//			return sqlDbType;

	//		throw new ArgumentException();
	//	}

	//	public static bool TryGetSqlDbType(Type type, out SqlDbType sqlDbType)
	//	{
	//		if (Map.TryGetValue(type, out sqlDbType))
	//			return true;

	//		if (type.IsEnum)
	//			return TryGetSqlDbType(type.GetEnumUnderlyingType(), out sqlDbType);

	//		if (type.IsValueType && type.IsGenericType && typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
	//			return TryGetSqlDbType(type.GetGenericArguments()[0], out sqlDbType);

	//		return false;
	//	}

	//	public static int GetDefaultScale(SqlDbType sqlDbType)
	//	{
	//		switch (sqlDbType)
	//		{
	//			case SqlDbType.DateTime2:
	//			case SqlDbType.DateTimeOffset:
	//			case SqlDbType.Time:
	//				return 7;

	//			case SqlDbType.Decimal:
	//				return 38;

	//			default:
	//				return 0;
	//		}
	//	}

	//	public static int GetDefaultPrecision(SqlDbType sqlDbType)
	//	{
	//		switch (sqlDbType)
	//		{
	//			case SqlDbType.BigInt:
	//				return 19;
	//			case SqlDbType.Bit:
	//				return 1;
	//			case SqlDbType.DateTime2:
	//				return 27;
	//			case SqlDbType.DateTimeOffset:
	//				return 34;
	//			case SqlDbType.Decimal:
	//				return 38;
	//			case SqlDbType.Float:
	//				return 53;
	//			case SqlDbType.Int:
	//				return 10;
	//			case SqlDbType.Real:
	//				return 24;
	//			case SqlDbType.Time:
	//				return 16;
	//			default:
	//				return 0;
	//		}
	//	}

	//	public static int GetDefaultMax(SqlDbType sqlDbType)
	//	{
	//		throw new NotImplementedException();
	//	}
	//}

	//internal static class ValueSerializerProvider
	//{
	//	private static readonly ConcurrentDictionary<Type, IValueSerializer> EnumSerializers = new ConcurrentDictionary<Type, IValueSerializer>();
	//	private static readonly XmlValueSerializer XmlValueSerializer = new XmlValueSerializer();

	//	public static IValueSerializer GetSerializerOrNull(Type type)
	//	{
	//		if (type == typeof(XElement))
	//			return XmlValueSerializer;

	//		if (type.IsEnum)
	//			return EnumSerializers.GetOrAdd(type, CreateEnumValueSerializer);

	//		if (type.IsValueType && type.IsGenericType && typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
	//			return GetSerializerOrNull(type.GetGenericArguments()[0]);

	//		return null;
	//	}

	//	public static bool TryGetSerializer(Type type, out IValueSerializer serializer)
	//	{
	//		serializer = GetSerializerOrNull(type);
	//		return serializer != null;
	//	}

	//	private static IValueSerializer CreateEnumValueSerializer(Type type)
	//	{
	//		var serializerType = typeof(EnumValueSerializer<>).MakeGenericType(type);
	//		var instance = Activator.CreateInstance(serializerType);
	//		return (IValueSerializer)instance;
	//	}
	//}
}