using System;

namespace TinyORM
{
	internal class DefaultConnectionStringProvider : IConnectionStringProvider
	{
		public DefaultConnectionStringProvider()
		{
			;
		}
		public string GetConnectionString()
		{
			return NoOp();
		}

		public string[] GetConnectionStrings()
		{
			return new[] { NoOp() };
		}

		private static string NoOp()
		{
			throw new InvalidOperationException("No connection string provider defined.");
		}
	}
}