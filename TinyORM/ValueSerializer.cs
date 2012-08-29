using System;

namespace TinyORM
{
	public abstract class ValueSerializer<TValue, TData> : IValueSerializer
	{
		public Type DataType
		{
			get { return typeof(TData); }
		}

		public Type ValueType
		{
			get { return typeof(TValue); }
		}

		public object Deserialize(object data)
		{
			return Deserialize((TData)data);
		}

		public object Serialize(object value)
		{
			return Serialize((TValue)value);
		}

		protected abstract TValue Deserialize(TData data);
		protected abstract TData Serialize(TValue value);
	}
}