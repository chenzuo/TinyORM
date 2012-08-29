using System;
using System.Data;

namespace TinyORM
{
	internal interface IInstanceMapper
	{
		object Map(IDataRecord record, Type type);
	}
}