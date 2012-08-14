namespace TinyORM
{
	public class DatabaseConfiguration : TypeConfiguration
	{
		public IConnectionStringProvider ConnectionStringProvider { get; set; }
	}
}