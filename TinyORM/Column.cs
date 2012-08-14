using System.Data;

namespace TinyORM
{
	public class Column
	{
		public bool IsNullable { get; set; }
		internal bool IsPrimaryKey { get; set; }
		public int? Max { get; set; }
		public string Name { get; set; }
		public int? Precision { get; set; }
		public int? Scale { get; set; }
		public SqlDbType? Type { get; set; }
	}
}