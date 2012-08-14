using NUnit.Framework;

namespace TinyORM.Tests
{
	[TestFixture]
	public class PropertyGetter_accessing_a_nested_property
	{
		[Test]
		public void should_return_the_nested_property_value()
		{
			var foo = new { Text = "Hello world" };
			var getter = PropertyGetter.Create(foo.GetType(), "Text.Length");
			Assert.That(getter.Get(foo), Is.EqualTo(foo.Text.Length));
		}
	}
}