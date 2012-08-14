using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;

namespace TinyORM
{
	public class ExpandoObjectMapper : IInstanceMapper
	{
		public object Map(IDataRecord record, Type type)
		{
			var expandoObject = new ExpandoObject();
			var dictionary = (IDictionary<string, object>)expandoObject;

			for (var i = 0; i < record.FieldCount; i++)
			{
				var name = record.GetName(i);
				dictionary.Add(name, record.IsDBNull(i) ? null : record.GetValue(i));
			}

			return expandoObject;
		}
	}
}