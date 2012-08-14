using System;
using NUnit.Framework;

namespace TinyORM.Tests
{
	[TestFixture]
	public class EnumValueSerializer_deserializing
	{
		[Test]
		public void should_return_null_if_data_is_null()
		{
			var serializer = new EnumValueSerializer<Values>();
			var value = serializer.Deserialize(null);
			Assert.That(value, Is.Null);
		}

		[Test]
		public void should_deserialize_a_defined_value()
		{
			var input = Values.One;
			var serializer = new EnumValueSerializer<Values>();
			var output = serializer.Deserialize((int)input);
			Assert.That(output, Is.EqualTo(output));
		}

		[Test]
		public void should_deserialize_a_combined_value_if_it_is_a_flags_enum()
		{
			var input = Values.One | Values.Two;
			var serializer = new EnumValueSerializer<Values>();
			var output = serializer.Deserialize((int)input);
			Assert.That(output, Is.EqualTo(output));
		}

		[Test]
		public void should_throw_if_input_data_is_invalid()
		{
			var serializer = new EnumValueSerializer<Values>();
			Assert.That(() => serializer.Deserialize(4), Throws.TypeOf<InvalidCastException>());
		}

		[Flags]
		private enum Values
		{
			Zero = 0,
			One = 1,
			Two = 2
		}
	}
}