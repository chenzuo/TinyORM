using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace TinyORM
{
	internal class MultiParameterFactory : ParameterFactory
	{
		private readonly string _operator;

		public MultiParameterFactory(string @operator, IEnumerable<object> values, string parameterName = null)
			: base(values as ICollection<object> ?? values.ToList(), parameterName)
		{
			_operator = @operator;
		}

		private IEnumerable<object> Values
		{
			get { return (IEnumerable<object>)Value; }
		}

		public override IEnumerable<IDataParameter> GetParameters(IColumnMap columnMap)
		{
			var index = 0;
			var copy = new ColumnMap
				{
					ColumnName = columnMap.ColumnName,
					ColumnType = columnMap.ColumnType,
					IsPrimaryKey = columnMap.IsPrimaryKey,
					PropertyName = columnMap.PropertyName,
					PropertyType = columnMap.PropertyType,
					Serializer = columnMap.Serializer
				};

			foreach (var value in Values)
			{
				copy.ColumnName = string.Format("{0}_{1}", columnMap.ColumnName, index++);
				yield return CreateParameter(copy, value);
			}
		}

		public override string GetSql(string columnName)
		{
			return string.Format("{0} {1} ({2})", columnName, _operator, GetParameterNames(columnName));
		}

		public override string UpdateSql(string sql, string columnName)
		{
			var pattern = Regex.Escape(GetParameterName(columnName));
			var replacement = GetParameterNames(columnName);
			return Regex.Replace(sql, pattern, replacement, RegexOptions.IgnoreCase);
		}

		private string GetParameterNames(string name)
		{
			var parameterNames = from index in Enumerable.Range(0, Values.Count())
										select string.Format("{0}_{1}", GetParameterName(name), index);
			return string.Join(", ", parameterNames);
		}
	}
}