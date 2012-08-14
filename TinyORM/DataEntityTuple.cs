namespace TinyORM
{
	public class DataEntityTuple<T>
	{
		public DataEntityTuple(dynamic data, T entity)
		{
			Data = data;
			Entity = entity;
		}

		public dynamic Data { get; private set; }
		public T Entity { get; private set; }
	}
}