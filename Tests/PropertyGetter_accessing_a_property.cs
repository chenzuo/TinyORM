using NUnit.Framework;

namespace TinyORM.Tests
{
	[TestFixture]
	public class PropertyGetter_accessing_a_property
	{
		[Test]
		public void should_return_the_property_value()
		{
			var foo = new { Number = 123 };
			var getter = PropertyGetter.Create(foo.GetType(), "Number");
			Assert.That(getter.Get(foo), Is.EqualTo(foo.Number));
		}
	}
}