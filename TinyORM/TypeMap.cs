using System;
using System.Collections.Generic;

namespace TinyORM
{
	public class TypeMap
	{
		public TypeMap()
		{
			Properties = new List<PropertyMap>();
		}

		public Type Type { get; set; }
		public string Table { get; set; }
		public string Namespace { get; set; }
		public List<PropertyMap> Properties { get; private set; }
	}
}