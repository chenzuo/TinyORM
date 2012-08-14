using System.Collections.Generic;

namespace TinyORM
{
	public class TypeConfiguration
	{
		public TypeConfiguration()
		{
			TypeMaps = new TypeMapDictionary();
			InstanceFactoryProviders = new List<InstanceFactoryProvider>();
		}

		public TypeMapDictionary TypeMaps { get; private set; }
		public List<InstanceFactoryProvider> InstanceFactoryProviders { get; private set; }
	}
}