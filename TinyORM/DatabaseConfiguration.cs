namespace TinyORM
{
	public class DatabaseConfiguration : MapConfiguration
	{
		public IConnectionStringProvider ConnectionStringProvider { get; set; }
	}
}