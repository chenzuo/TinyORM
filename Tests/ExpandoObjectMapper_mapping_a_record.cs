using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using NUnit.Framework;

namespace TinyORM.Tests
{
	[TestFixture]
	public class ExpandoObjectMapper_mapping_a_record
	{
		[Test]
		public void should_map_all_columns()
		{
			using (var table = new DataTable())
			{
				table.Columns.Add("Id", typeof(int));
				table.Rows.Add(1);
				using (var reader = new DataTableReader(table))
				{
					reader.Read();
					var dynamic = new ExpandoObjectMapper().Map(reader, typeof(ExpandoObject));

					Assert.That(((IDictionary<string, object>)@dynamic).Count, Is.EqualTo(1));
				}
			}
		}

		[Test]
		public void should_map_columns_containing_a_value()
		{
			using (var table = new DataTable())
			{
				table.Columns.Add("Id", typeof(int));
				table.Rows.Add(1);
				using (var reader = new DataTableReader(table))
				{
					reader.Read();
					dynamic dynamic = new ExpandoObjectMapper().Map(reader, typeof(ExpandoObject));

					Assert.That((object)dynamic.Id, Is.EqualTo(1));
				}
			}
		}

		[Test]
		public void should_map_columns_containing_null()
		{
			using (var table = new DataTable())
			{
				table.Columns.Add("Id", typeof(int));
				table.Rows.Add(default(int?));
				using (var reader = new DataTableReader(table))
				{
					reader.Read();
					dynamic dynamic = new ExpandoObjectMapper().Map(reader, typeof(ExpandoObject));

					Assert.That((object)dynamic.Id, Is.Null);
				}
			}
		}
	}
}