using System.Xml.Linq;

namespace TinyORM
{
	public class XmlValueSerializer : ValueSerializer<XElement, string>
	{
		protected override XElement Deserialize(string data)
		{
			return data == null ? null : XElement.Parse(data);
		}

		protected override string Serialize(XElement value)
		{
			return value == null ? null : value.ToString(SaveOptions.DisableFormatting);
		}
	}
}