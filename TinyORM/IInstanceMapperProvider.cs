using System;
using System.Collections.Generic;

namespace TinyORM
{
	internal interface IInstanceMapperProvider
	{
		IInstanceMapper GetMapper(Type targetType, IEnumerable<DataColumnInfo> schema);
	}
}