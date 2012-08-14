using System;
using System.Linq.Expressions;

namespace TinyORM
{
	public abstract class Map<T> : ITypeMapProvider
	{
		private readonly TypeMap _map = new TypeMap();

		public void Table(string table, string @namespace = null)
		{
			_map.Namespace = @namespace;
			_map.Table = table;
		}

		public void Id<TValue>(string column)
		{
			Id<TValue>(new Column { Name = column });
		}

		public void Id<TValue>(Column column)
		{
			column.IsPrimaryKey = true;
			CreatePropertyMap<TValue>(column.Name, column);
		}

		public void Id<TProperty>(Expression<Func<T, TProperty>> property, Column column = null)
		{
			column = column ?? new Column();
			column.IsPrimaryKey = true;
			CreatePropertyMap(property, column);
		}

		public void Value<TProperty>(Expression<Func<T, TProperty>> property, Column column = null)
		{
			column = column ?? new Column();
			column.IsPrimaryKey = false;
			CreatePropertyMap(property, column);
		}

		private void CreatePropertyMap<TProperty>(Expression<Func<T, TProperty>> property, Column column)
		{
			var propertyName = GetPropertyName(property);
			CreatePropertyMap<TProperty>(propertyName, column);
		}

		private void CreatePropertyMap<TProperty>(string propertyName, Column column)
		{
			var propertyType = typeof(TProperty);

			IValueSerializer valueSerializer = null;
			var map = new PropertyMap
				{
					ColumnName = column.Name ?? propertyName.Replace(".", string.Empty),
					ColumnType = SqlDbTypeProvider.GetSqlDbType(propertyType),
					IsPrimaryKey = column.IsPrimaryKey,
					PropertyName = propertyName,
					PropertyType = propertyType,
					ValueSerializer = ValueSerializerProvider.TryGetSerializer(propertyType, out valueSerializer)
												? valueSerializer
												: null
				};

			_map.Properties.Add(map);
		}

		private string GetPropertyName(Expression property)
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

		public TypeMap GetTypeMap()
		{
			_map.Type = typeof(T);
			_map.Table = _map.Table ?? typeof(T).Name;
			_map.Namespace = _map.Namespace ?? "dbo";
			return _map;
		}
	}
}