using System;
using System.Linq;

namespace TinyORM.Debugger
{
	public class Program
	{
		public static void Main()
		{
			var db = new Database();
			db.Configure(x =>
				{
					x.ConnectionStringProvider(new ConnectionStringProvider("sqldev02", "Sandbox"));
					x.AddMapsFromAssemblyContaining<ItemMap>();
				});

			db.Execute("update item set number = 2 where id = 'F2608C87-CAF8-4B9F-97A0-83CECA9ED5D6'");
		}
	}

	public class ItemMap : Map<Item>
	{
		public ItemMap()
		{
			Id(x => x.Number);
			Value(x => x.Id);
			Value(x => x.Text);
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
		public string Text { get; set; }
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
