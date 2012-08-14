using NUnit.Framework;

namespace TinyORM.Tests
{
	[TestFixture]
	public class Database_accessing_the_static_instance
	{
		[Test]
		public void should_always_get_the_same_instance()
		{
			var instance1 = Database.Instance;
			var instance2 = Database.Instance;

			Assert.That(instance1, Is.EqualTo(instance2));
		}
	}
}