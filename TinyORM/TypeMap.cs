using System;
using System.Collections.Generic;

namespace TinyORM
{
	public interface ITypeMap
	{
		Type Type { get; }
		string Table { get; }
		string Namespace { get; }
		IEnumerable<PropertyMap> Properties { get; }
	}

	public class TypeMap : ITypeMap
	{
		public TypeMap()
		{
			Properties = new List<PropertyMap>();
		}

		public Type Type { get; set; }
		public string Table { get; set; }
		public string Namespace { get; set; }
		public List<PropertyMap> Properties { get; private set; }

		IEnumerable<PropertyMap> ITypeMap.Properties
		{
			get { return Properties; }
		}
	}
}