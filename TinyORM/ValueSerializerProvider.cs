using System;
using System.Collections.Concurrent;
using System.Xml.Linq;

namespace TinyORM
{
	internal static class ValueSerializerProvider
	{
		private static readonly ConcurrentDictionary<Type, IValueSerializer> EnumSerializers = new ConcurrentDictionary<Type, IValueSerializer>();
		private static readonly XmlValueSerializer XmlValueSerializer = new XmlValueSerializer();

		public static bool TryGetSerializer(Type type, out IValueSerializer valueSerializer)
		{
			if (type == typeof(XElement))
			{
				valueSerializer = XmlValueSerializer;
				return true;
			}

			if (type.IsEnum)
			{
				valueSerializer = EnumSerializers.GetOrAdd(type, CreateEnumValueSerializer);
				return true;
			}

			if (type.IsValueType && type.IsGenericType && typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
				return TryGetSerializer(type.GetGenericArguments()[0], out valueSerializer);

			valueSerializer = null;
			return false;
		}

		private static IValueSerializer CreateEnumValueSerializer(Type type)
		{
			var serializerType = typeof(EnumValueSerializer<>).MakeGenericType(type);
			var instance = Activator.CreateInstance(serializerType);
			return (IValueSerializer)instance;
		}
	}
}