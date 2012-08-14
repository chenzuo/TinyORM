using System;
using System.Data;

namespace TinyORM
{
	public interface IInstanceMapper
	{
		object Map(IDataRecord record, Type type);
	}
}