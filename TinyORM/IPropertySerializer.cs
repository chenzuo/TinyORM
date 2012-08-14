namespace TinyORM
{
	public interface IPropertySerializer
	{
		object Deserialize(object data);
		object Serialize(object value);
	}
}