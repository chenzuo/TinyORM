using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace TinyORM
{
	public class DataParametersMapper
	{
		public IEnumerable<IDataParameter> Map(object input, TableMap tableMap)
		{
			if (input == null)
				return Enumerable.Empty<IDataParameter>();

			var dataParameters = input as IEnumerable<IDataParameter>;
			if (dataParameters != null)
				return dataParameters;

			throw new NotImplementedException();
		}
	}
}