using NUnit.Framework;

namespace TinyORM.Tests
{
	[TestFixture]
	public class PropertySetter_assigning_a_property
	{
		[Test]
		public void property_should_have_the_assigned_value()
		{
			var value = 123;
			var foobar = new Foobar();
			var setter = PropertySetter.Create(foobar.GetType(), "Number");
			setter.Set(foobar, value);
			Assert.That(foobar.Number, Is.EqualTo(value));
		}

		private class Foobar
		{
			public int Number { get; set; }
		}
	}
}