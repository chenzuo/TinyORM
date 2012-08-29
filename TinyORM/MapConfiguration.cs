using System.Collections.Generic;

namespace TinyORM
{
	public class MapConfiguration
	{
		public MapConfiguration()
		{
			Defaults = new Defaults();
			Tables = new TableMapDictionary();
			InstanceFactoryProviders = new List<IInstanceFactory>();
			SaveOperationProviders = new List<ISaveOperationProvider>();
		}

		public IDefaults Defaults { get; private set; }
		public TableMapDictionary Tables { get; private set; }
		public List<IInstanceFactory> InstanceFactoryProviders { get; private set; }
		public List<ISaveOperationProvider> SaveOperationProviders { get; private set; }
	}
}