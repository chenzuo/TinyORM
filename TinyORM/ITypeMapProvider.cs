namespace TinyORM
{
	public interface ITypeMapProvider
	{
		TableMap GetTypeMap(IDefaultsGetter defaults);
	}
}