using System;
using System.Data;
using System.Xml.Linq;
using NUnit.Framework;

namespace TinyORM.Tests
{
	[TestFixture]
	public class SqlDbTypeProvider_get_SqlDbType
	{
		[Test]
		public void bool_should_map_to_Bit()
		{
			AssertTypeToSqlDbType<bool>(SqlDbType.Bit);
		}

		[Test]
		public void byte_should_map_to_TinyInt()
		{
			AssertTypeToSqlDbType<byte>(SqlDbType.TinyInt);
		}

		[Test]
		public void byte_array_should_map_to_VarBinary()
		{
			AssertTypeToSqlDbType<byte[]>(SqlDbType.VarBinary);
		}

		[Test]
		public void DateTime_should_map_to_DateTime()
		{
			AssertTypeToSqlDbType<DateTime>(SqlDbType.DateTime);
		}

		[Test]
		public void DateTimeOffset_should_map_to_DateTimeOffset()
		{
			AssertTypeToSqlDbType<DateTimeOffset>(SqlDbType.DateTimeOffset);
		}

		[Test]
		public void decimal_should_map_to_Decimal()
		{
			AssertTypeToSqlDbType<decimal>(SqlDbType.Decimal);
		}

		[Test]
		public void double_should_map_to_Float()
		{
			AssertTypeToSqlDbType<double>(SqlDbType.Float);
		}

		[Test]
		public void float_should_map_to_Real()
		{
			AssertTypeToSqlDbType<float>(SqlDbType.Real);
		}

		[Test]
		public void Guid_should_map_to_UniquIdentifier()
		{
			AssertTypeToSqlDbType<Guid>(SqlDbType.UniqueIdentifier);
		}

		[Test]
		public void int_should_map_to_Int()
		{
			AssertTypeToSqlDbType<int>(SqlDbType.Int);
		}

		[Test]
		public void long_should_map_to_BigInt()
		{
			AssertTypeToSqlDbType<long>(SqlDbType.BigInt);
		}

		[Test]
		public void short_should_map_to_SmallInt()
		{
			AssertTypeToSqlDbType<short>(SqlDbType.SmallInt);
		}

		[Test]
		public void string_should_map_to_NVarChar()
		{
			AssertTypeToSqlDbType<string>(SqlDbType.NVarChar);
		}

		[Test]
		public void TimeSpan_should_map_to_Time()
		{
			AssertTypeToSqlDbType<TimeSpan>(SqlDbType.Time);
		}

		[Test]
		public void XElement_should_map_to_Xml()
		{
			AssertTypeToSqlDbType<XElement>(SqlDbType.Xml);
		}

		[Test]
		public void Void_should_throw()
		{
			Assert.Throws<ArgumentException>(() => SqlDbTypeProvider.GetSqlDbType(typeof(void)));
		}

		private static void AssertTypeToSqlDbType<T>(SqlDbType sqlDbType)
		{
			Assert.That(SqlDbTypeProvider.GetSqlDbType(typeof(T)), Is.EqualTo(sqlDbType));
		}
	}
}