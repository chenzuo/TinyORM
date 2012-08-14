using System.Xml.Linq;
using NUnit.Framework;

namespace TinyORM.Tests
{
	[TestFixture]
	public class XElementValueSerializer_deserializing
	{
		[Test]
		public void should_map_null_to_null()
		{
			var output = new XmlValueSerializer().Deserialize(null);
			Assert.That(output, Is.Null);
		}

		[Test]
		public void should_map_valid_xml()
		{
			var xml = "<root><parent version=\"1\"><child /></parent></root>";
			var output = (XElement)new XmlValueSerializer().Deserialize(xml);
			Assert.That(output.ToString(SaveOptions.DisableFormatting), Is.EqualTo(xml));
		}

		[Test]
		public void should_throw_on_invalid_xml()
		{
			Assert.That(() => new XmlValueSerializer().Deserialize("foobar"), Throws.Exception);
		}
	}
}