using System;

namespace TinyORM
{
	public interface IValueSerializer
	{
		Type DataType { get; }
		Type ValueType { get; }

		object Deserialize(object data);
	}
}