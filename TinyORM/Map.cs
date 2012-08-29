using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace TinyORM
{
	public abstract class Map<T> : ITypeMapProvider
	{
		private string _schema;
		private string _table;
		private readonly List<Values> _values = new List<Values>();

		public void Table(string table, string schema = null)
		{
			_schema = schema;
			_table = table;
		}

		public void Key<TValue>(string column)
		{
			Key<TValue>(new Column { Name = column });
		}

		public void Key<TValue>(Column column)
		{
			column.IsPrimaryKey = true;
			CreateColumnMap<TValue>(column.Name, column);
		}

		public void Key<TValue>(Expression<Func<T, TValue>> property, Column column = null)
		{
			column = column ?? new Column();
			column.IsPrimaryKey = true;
			CreateColumnMap(property, column);
		}

		public void Value<TValue>(string column, IValueSerializer serializer = null)
		{
			Value<TValue>(new Column { Name = column }, serializer);
		}

		public void Value<TValue>(Column column, IValueSerializer serializer = null)
		{
			CreateColumnMap<TValue>(column.Name, column, serializer);
		}

		public void Value<TValue>(Expression<Func<T, TValue>> property, Column column = null, IValueSerializer serializer = null)
		{
			column = column ?? new Column();
			column.IsPrimaryKey = false;
			CreateColumnMap(property, column, serializer);
		}

		private void CreateColumnMap<TProperty>(Expression<Func<T, TProperty>> property, Column column, IValueSerializer serializer = null)
		{
			var propertyName = GetPropertyName(property);
			CreateColumnMap<TProperty>(propertyName, column, serializer);
		}

		private void CreateColumnMap<TProperty>(string propertyName, Column column, IValueSerializer serializer = null)
		{
			_values.Add(new Values
				{
					ColumnMaxLength = column.MaxLength,
					ColumnName = column.Name,
					ColumnPrecision = column.Precision,
					ColumnScale = column.Scale,
					ColumnType = column.Type,
					IsGenerated = column.IsGenerated,
					IsNullable = column.IsNullable,
					IsPrimaryKey = column.IsPrimaryKey,
					PropertyName = propertyName,
					PropertyType = typeof(TProperty),
					Serializer = serializer,
				});

			//var columnMap = new Values
			//	{
			//		ColumnMaxLength = column.MaxLength ?? SqlDbTypeProvider.GetDefaultMax(sqlDbType),
			//		ColumnName = column.Name ?? propertyName.Replace(".", string.Empty),
			//		ColumnPrecision = column.Precision ?? SqlDbTypeProvider.GetDefaultPrecision(sqlDbType),
			//		ColumnScale = column.Scale ?? SqlDbTypeProvider.GetDefaultScale(sqlDbType),
			//		ColumnType = sqlDbType,
			//		IsGenerated = column.IsGenerated,
			//		IsNullable = column.IsNullable,
			//		IsPrimaryKey = column.IsPrimaryKey,
			//		PropertyName = propertyName,
			//		PropertyType = propertyType,
			//		Serializer = serializer ?? ValueSerializerProvider.GetSerializerOrNull(propertyType)
			//	};
		}

		private static string GetPropertyName(Expression property)
		{
			var lambda = property as LambdaExpression;

			if (lambda == null)
				throw new ArgumentException();

			var member = (MemberExpression)lambda.Body;
			var name = member.Member.Name;

			while (member.Expression.NodeType == ExpressionType.MemberAccess)
			{
				member = (MemberExpression)member.Expression;
				name = string.Format("{0}.{1}", member.Member.Name, name);
			}

			return name;
		}

		public TableMap GetTypeMap(IDefaultsGetter defaults)
		{
			var columns = from values in _values
							  let propertyType = values.PropertyType
							  let sqlDbType = values.ColumnType ?? defaults.GetSqlDbType(propertyType)
							  let parameterDefaults = defaults.GetParameterDefaults(sqlDbType)
							  select new ColumnMap
								  {
									  ColumnMaxLength = values.ColumnMaxLength ?? parameterDefaults.MaxLength,
									  ColumnName = values.ColumnName ?? values.PropertyName.Replace(".", ""),
									  ColumnPrecision = values.ColumnPrecision ?? parameterDefaults.Precision,
									  ColumnScale = values.ColumnScale ?? parameterDefaults.Scale,
									  ColumnType = values.ColumnType ?? defaults.GetSqlDbType(propertyType),
									  IsGenerated = values.IsGenerated,
									  IsNullable = values.IsNullable ?? defaults.IsNullable(propertyType, sqlDbType),
									  IsPrimaryKey = values.IsPrimaryKey,
									  PropertyName = values.PropertyName,
									  PropertyType = propertyType,
									  Serializer = values.Serializer ?? defaults.GetSerializerOrNull(propertyType, sqlDbType)
								  };

			return new TableMap
				{
					Columns = columns.ToList(),
					Schema = _schema ?? "dbo",
					Table = _table ?? typeof(T).Name,
					Type = typeof(T)
				};
		}

		private class Values
		{
			public int? ColumnMaxLength { get; set; }
			public string ColumnName { get; set; }
			public byte? ColumnPrecision { get; set; }
			public byte? ColumnScale { get; set; }
			public SqlDbType? ColumnType { get; set; }
			public bool IsGenerated { get; set; }
			public bool? IsNullable { get; set; }
			public bool IsPrimaryKey { get; set; }
			public string PropertyName { get; set; }
			public Type PropertyType { get; set; }
			public IValueSerializer Serializer { get; set; }
		}
	}
}