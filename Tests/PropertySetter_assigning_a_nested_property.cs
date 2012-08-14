using NUnit.Framework;

namespace TinyORM.Tests
{
	[TestFixture]
	public class PropertySetter_assigning_a_nested_property
	{
		[Test]
		public void the_nested_property_should_have_the_assigned_value()
		{
			var value = 123;
			var foo = new Foo { Bar = new Bar { Number = 0 } };
			var setter = PropertySetter.Create(foo.GetType(), "Bar.Number");
			setter.Set(foo, value);
			Assert.That(foo.Bar.Number, Is.EqualTo(value));
		}

		private class Foo
		{
			public Bar Bar { get; set; }
		}

		private class Bar
		{
			public int Number { get; set; }
		}
	}
}