using System.Data;

namespace TinyORM
{
	public class Column
	{
		public bool IsGenerated { get; set; }
		public bool? IsNullable { get; set; }
		internal bool IsPrimaryKey { get; set; }
		public int? MaxLength { get; set; }
		public string Name { get; set; }
		public byte? Precision { get; set; }
		public byte? Scale { get; set; }
		public SqlDbType? Type { get; set; }
	}
}