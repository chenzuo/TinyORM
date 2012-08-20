using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace TinyORM.Debugger
{
	interface IFoobar { }
	class Foo : IFoobar { }
	class Bar : IFoobar { }
	public class Program
	{
		private static int Value;

		public static void Main()
		{
			var assign = new List<long>();
			var cast = new List<long>();
			for (int a = 0; a < 10; a++)
			{
				var stopwatch = Stopwatch.StartNew();
				for (int i = 0; i < 1000000; i++)
					Value = i;
				assign.Add(stopwatch.ElapsedTicks);
				stopwatch = Stopwatch.StartNew();
				for (int i = 0; i < 1000000; i++)
					Value = (int)(object)i;
				cast.Add(stopwatch.ElapsedTicks);
			}
			Console.WriteLine("assign avg " + assign.Average());
			Console.WriteLine("assign max " + assign.Max());
			Console.WriteLine("assign min " + assign.Min());
			Console.WriteLine("cast avg   " + cast.Average());
			Console.WriteLine("cast max   " + cast.Max());
			Console.WriteLine("cast min   " + cast.Min());
			return;
			var objects = new object[]
				{
					new {foobar=(IFoobar)new Foo()}, new {foobar=(IFoobar)new Bar()}
				};
			foreach (var o in objects.GroupBy(x => x.GetType()))
			{
				Console.WriteLine(o.Count() + " " + o.Key.FullName);
			}
			return;
			var values = new object[]
				{
					default(bool),
					default(byte),
					new byte[0],
					default(char),
					default(DateTime),
					default(DateTimeOffset),
					default(decimal),
					default(double),
					default(float),
					default(Guid),
					default(int),
					default(long),
					default(short),
					string.Empty,
					default(TimeSpan)
				};
			var enumerable = from value in values
								  let parameter = new SqlParameter("foo", value)
								  orderby parameter.SqlDbType.ToString()
								  select new[]
				                 {
										  value.GetType().Name,
					                 parameter.SqlDbType.ToString(),
					                 "o: " + parameter.Offset,
					                 "sc: " + parameter.Scale,
					                 "si: " + parameter.Size,
					              //   "v: " + parameter.Value.ToString()
				                 };

			var list = enumerable.ToList();
			var lengths = list.First().Select(x => 0).ToArray();

			foreach (var item in list)
				for (int i = 0; i < lengths.Length; i++)
					if (item[i].Length > lengths[i])
						lengths[i] = item[i].Length;

			foreach (var item in list)
			{
				var line = string.Empty;
				for (int i = 0; i < lengths.Length; i++)
					line += item[i].PadRight(lengths[i] + 1);
				Console.WriteLine(line);
			}

			return;

			new Database().Execute("select @foo", new { foo = "hello world" });
			;
			new Database().Configure(x =>
				{
					x.Map<Foobar>(y =>
						{
							y.Id(z => z.Id);
							y.Value(z => z.Text);
							y.Value(z => z.Person.Name.First, new Column { Name = "PersonFirstName" });
							y.Value(z => z.Person.Name.Last);
						});
					;
				});
			//var foobars = new Database().Query<Foobar>().OrderBy(x => x.Id).ToList();
			//var foobars = new Database().Query<Foobar>().OrderBy(x => Table.Column("ParentId") == 6).ToList();
			//var foobars = new Database().Query<Foobar>().Where(x => x.Id == 1).ToList();
			//var foobars = new Database().Query<Foobar>().Where(x => x.Id < 1 || x.Text == string.Empty).ToList();
			//var foobars = new Database().Query<Foobar>("select * from foobar");
			//var foobars = new Database().Query<Foobar>(new { Id = Is.In(new[] { 1, 2 }) });
			var dynamics = new Database().Query("select * from foobar");
		}

		private void Test()
		{
			//new DataModelContainer().Entity1Set.Where()
		}
	}

	public class Foobar
	{
		public int Id { get; set; }
		public string Text { get; set; }
		public Person Person { get; set; }
	}

	public class Person
	{
		public int Id { get; set; }
		public PersonName Name { get; set; }

		public class PersonName
		{
			public string First { get; set; }
			public string Last { get; set; }
			public string Full
			{
				get { throw new NotImplementedException(); }
			}
		}
	}
}
