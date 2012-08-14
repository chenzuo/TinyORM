namespace TinyORM
{
	public interface IConnectionStringProvider
	{
		string GetConnectionString();
		string[] GetConnectionStrings();
	}
}