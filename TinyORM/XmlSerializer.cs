using System.Xml.Linq;

namespace TinyORM
{
	public class XmlSerializer : IPropertySerializer
	{
		public object Deserialize(object data)
		{
			return data == null ? null : XElement.Parse((string)data);
		}

		public object Serialize(object value)
		{
			return value == null ? null : ((XElement)value).ToString(SaveOptions.DisableFormatting);
		}
	}
}