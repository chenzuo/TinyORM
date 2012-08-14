using NUnit.Framework;

namespace TinyORM.Tests
{
	[TestFixture]
	public class DefaultInstanceFactory_creating_an_instance
	{
		[Test]
		public void instance_should_not_be_null()
		{
			var instance = new DefaultInstanceFactory().CreateInstance(typeof(Foobar));

			Assert.That(instance, Is.Not.Null);
		}

		[Test]
		public void instance_should_be_of_the_provided_type()
		{
			var instance = new DefaultInstanceFactory().CreateInstance(typeof(Foobar));

			Assert.That(instance, Is.InstanceOf<Foobar>());
		}

		[Test]
		public void should_always_return_a_new_instance()
		{
			var factory = new DefaultInstanceFactory();
			var instance1 = factory.CreateInstance(typeof(Foobar));
			var instance2 = factory.CreateInstance(typeof(Foobar));

			Assert.That(instance1, Is.Not.EqualTo(instance2));
		}

		private class Foobar { }
	}
}