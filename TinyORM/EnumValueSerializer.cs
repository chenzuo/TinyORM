using System;

namespace TinyORM
{
	public class EnumValueSerializer<T> : IValueSerializer
		where T : struct
	{
		public Type DataType
		{
			get { return Enum.GetUnderlyingType(typeof(T)); }
		}

		public Type ValueType
		{
			get { return typeof(T); }
		}

		public object Deserialize(object data)
		{
			if (data == null)
				return null;

			var type = typeof(T);
			var dataString = data.ToString();
			var value = Enum.Parse(type, dataString);
			var valueString = value.ToString();
			if (dataString == valueString)
				throw new InvalidCastException(string.Format("'{0}' is not a valid value for {1}", valueString, type));
			return value;
		}

		public object Serialize(object value)
		{
			return value == null ? null : Convert.ChangeType(value, DataType);
		}
	}
}