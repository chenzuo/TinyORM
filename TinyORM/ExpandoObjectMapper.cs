using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;

namespace TinyORM
{
	internal class ExpandoObjectMapper : IInstanceMapper
	{
		public object Map(IDataRecord record, Type type)
		{
			if (type != typeof(ExpandoObject))
				throw new ArgumentException();

			var expandoObject = new ExpandoObject();
			var dictionary = (IDictionary<string, object>)expandoObject;
			var noNameIndex = 0;

			for (var i = 0; i < record.FieldCount; i++)
			{
				var name = record.GetName(i) ?? "NoName" + noNameIndex++;
				dictionary.Add(name, record.IsDBNull(i) ? null : record.GetValue(i));
			}

			return expandoObject;
		}
	}
}