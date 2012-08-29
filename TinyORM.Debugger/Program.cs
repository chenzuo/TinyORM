using System;
using System.Linq;

namespace TinyORM.Debugger
{
	public class Program
	{
		public static void Main()
		{
			var db = new Database();
			db.Configure(x => x.ConnectionStringProvider(new ConnectionStringProvider("sqldev02", "Sandbox")));

			var ids = new[] { 1, 2 };
			var enumerable = db.Query("select * from item");
			var items1 = db.Query<Item>("select * from item");

			db.Configure(x => x.AddMapsFromAssemblyContaining<ItemMap>());

			var items2 = db.Query<Item>(new { number = Is.In(ids.Cast<object>()) });
		}
	}

	public class ItemMap : Map<Item>
	{
		public ItemMap()
		{
			Key(x => x.Number);
			Value(x => x.Id);
			Value(x => x.Info, new Column { Name = "Text" });
			Value(x => x.Timestamp);
		}
	}

	public class ConnectionStringProvider : IConnectionStringProvider
	{
		private readonly string _server;
		private readonly string _database;

		public ConnectionStringProvider(string server, string database)
		{
			_server = server;
			_database = database;
		}

		public string GetConnectionString()
		{
			return GetConnectionStrings().First();
		}

		public string[] GetConnectionStrings()
		{
			return new[] { string.Format("Data Source={0};Initial Catalog={1};Integrated Security=True", _server, _database) };
		}
	}

	public class Item
	{
		public Guid Id { get; set; }
		public int Number { get; set; }
		public DateTimeOffset Timestamp { get; set; }
		public string Info { get; set; }
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
