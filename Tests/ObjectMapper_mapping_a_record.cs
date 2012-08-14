using System;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace TinyORM.Tests
{
	[TestFixture]
	public class ObjectMapper_mapping_a_record
	{
		[Test]
		public void should_map_ids()
		{
			var value = 1;
			using (var table = CreateTable("Id", value))
			{
				var mapper = CreateMapper(table, x => x.Id(y => y.Id));
				using (var reader = new DataTableReader(table))
				{
					reader.Read();
					var person = (Foobar)mapper.Map(reader, typeof(Foobar));

					Assert.That(person.Id, Is.EqualTo(value));
				}
			}
		}

		[Test]
		public void should_map_values()
		{
			var value = 1;
			using (var table = CreateTable("Value", value))
			{
				var mapper = CreateMapper(table, x => x.Value(y => y.Value));
				using (var reader = new DataTableReader(table))
				{
					reader.Read();
					var person = (Foobar)mapper.Map(reader, typeof(Foobar));

					Assert.That(person.Value, Is.EqualTo(value));
				}
			}
		}

		[Test]
		public void should_map_values_where_property_and_column_name_differs_by_casing()
		{
			var value = 1;
			using (var table = CreateTable("vALUE", value))
			{
				var mapper = CreateMapper(table, x => x.Value(y => y.Value));
				using (var reader = new DataTableReader(table))
				{
					reader.Read();
					var person = (Foobar)mapper.Map(reader, typeof(Foobar));

					Assert.That(person.Value, Is.EqualTo(value));
				}
			}
		}

		[Test]
		public void should_map_values_where_property_and_column_has_diffrent_names()
		{
			var value = 1;
			var columnName = "SpecialValue";
			using (var table = CreateTable(columnName, value))
			{
				var mapper = CreateMapper(table, x => x.Value(y => y.Value, new Column { Name = columnName }));
				using (var reader = new DataTableReader(table))
				{
					reader.Read();
					var person = (Foobar)mapper.Map(reader, typeof(Foobar));

					Assert.That(person.Value, Is.EqualTo(value));
				}
			}
		}

		[Test]
		public void should_map_nullable_type_with_value()
		{
			var value = 1;
			using (var table = CreateTable("NullableValueDefaultsToZero", value))
			{
				var mapper = CreateMapper(table, x => x.Value(y => y.NullableValueDefaultsToZero));
				using (var reader = new DataTableReader(table))
				{
					reader.Read();
					var person = (Foobar)mapper.Map(reader, typeof(Foobar));

					Assert.That(person.NullableValueDefaultsToZero, Is.EqualTo(value));
				}
			}
		}

		[Test]
		public void should_map_nullable_type_with_null()
		{
			int? value = null;
			using (var table = CreateTable("NullableValueDefaultsToZero", value, typeof(int)))
			{
				var mapper = CreateMapper(table, x => x.Value(y => y.NullableValueDefaultsToZero));
				using (var reader = new DataTableReader(table))
				{
					reader.Read();
					var person = (Foobar)mapper.Map(reader, typeof(Foobar));

					Assert.That(person.NullableValueDefaultsToZero, Is.EqualTo(value));
				}
			}
		}

		[Test]
		public void should_map_enums()
		{
			var value = FoobarEnum.Bar;
			using (var table = CreateTable("Enum", value, typeof(int)))
			{
				var mapper = CreateMapper(table, x => x.Value(y => y.Enum));
				using (var reader = new DataTableReader(table))
				{
					reader.Read();
					var person = (Foobar)mapper.Map(reader, typeof(Foobar));

					Assert.That(person.Enum, Is.EqualTo(value));
				}
			}
		}

		[Test]
		public void should_throw_when_mapping_an_invalid_value_to_enum()
		{
			using (var table = CreateTable("Enum", 10, typeof(int)))
			{
				var mapper = CreateMapper(table, x => x.Value(y => y.Enum));
				using (var reader = new DataTableReader(table))
				{
					reader.Read();

					Assert.That(() => mapper.Map(reader, typeof(Foobar)), Throws.Exception);
				}
			}
		}

		[Test]
		public void should_map_xml()
		{
			var xml = "<root><parent version=\"1\"><child /></parent></root>";
			using (var table = CreateTable("Xml", xml))
			{
				var mapper = CreateMapper(table, x => x.Value(y => y.Xml));
				using (var reader = new DataTableReader(table))
				{
					reader.Read();
					var person = (Foobar)mapper.Map(reader, typeof(Foobar));

					Assert.That(person.Xml.ToString(SaveOptions.DisableFormatting), Is.EqualTo(xml));
				}
			}
		}

		[Test]
		public void should_map_values_with_custom_value_mapper()
		{
			Assert.Inconclusive();
		}

		private static ObjectMapper CreateMapper(DataTable table, Action<TypeMapBuilder<Foobar>> configure)
		{
			var typeMapBuilder = new TypeMapBuilder<Foobar>();
			configure(typeMapBuilder);
			var typeMap = typeMapBuilder.GetTypeMap();
			var dataColumnInfos = table.Columns.Cast<DataColumn>().Select(x => new DataColumnInfo { Index = x.Ordinal, Name = x.ColumnName });
			var mapper = new ObjectMapper(new DefaultInstanceFactory());
			mapper.Initialize(typeMap, dataColumnInfos);
			return mapper;
		}

		private static DataTable CreateTable(string columnName, object value, Type type = null)
		{
			type = type ?? value.GetType();
			var table = new DataTable();
			table.Columns.Add(columnName, type);
			table.Rows.Add(value);
			return table;
		}

		private class Foobar
		{
			public Foobar()
			{
				NullableValueDefaultsToZero = 0;
			}

			public int Id { get; set; }
			public int Value { get; set; }
			public int? NullableValueDefaultsToZero { get; set; }
			public FoobarEnum Enum { get; set; }
			public XElement Xml { get; set; }
			public string NumberAsString { get; set; }
		}

		public enum FoobarEnum
		{
			Foo = 0,
			Bar = 1
		}
	}
}