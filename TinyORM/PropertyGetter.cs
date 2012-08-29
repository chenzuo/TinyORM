using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TinyORM
{
	internal class PropertyGetter
	{
		private static readonly ConcurrentDictionary<int, PropertyGetter> Cache = new ConcurrentDictionary<int, PropertyGetter>();
		private volatile Func<object, object> _getter;

		public static PropertyGetter Create(Type sourceType, string propertyName)
		{
			var key = (sourceType.FullName + propertyName).GetHashCode();
			return Cache.GetOrAdd(key, _ => CreateInstance(sourceType, propertyName));
		}

		private static PropertyGetter CreateInstance(Type sourceType, string propertyName)
		{
			if (!propertyName.Contains('.'))
				return new PropertyGetter(sourceType, propertyName);

			var getters = new List<PropertyGetter>();
			foreach (var name in propertyName.Split('.'))
			{
				getters.Add(new PropertyGetter(sourceType, name));
				sourceType = sourceType.GetProperty(name).PropertyType;
			}

			return new PropertyChainGetter(sourceType, propertyName, getters);
		}

		public Type SourceType { get; private set; }
		public string PropertyName { get; private set; }

		private PropertyGetter(Type sourceType, string propertyName)
		{
			SourceType = sourceType;
			PropertyName = propertyName;
		}

		public virtual object Get(object source)
		{
			if (_getter == null)
				lock (this)
					if (_getter == null)
						_getter = CreateGetter();

			return _getter(source);
		}

		private Func<object, object> CreateGetter()
		{
			var xObjectSource = Expression.Parameter(typeof(object), "source");
			var xSource = Expression.Convert(xObjectSource, SourceType);
			var xProperty = Expression.Property(xSource, PropertyName);
			var xReturn = Expression.Convert(xProperty, typeof(object));
			var xLambda = Expression.Lambda<Func<object, object>>(xReturn, xObjectSource);
			return xLambda.Compile();
		}

		private class PropertyChainGetter : PropertyGetter
		{
			private readonly PropertyGetter[] _getters;

			internal PropertyChainGetter(Type sourceType, string propertyName, IEnumerable<PropertyGetter> getters)
				: base(sourceType, propertyName)
			{
				_getters = getters.ToArray();
			}

			public override object Get(object source)
			{
				return _getters.Aggregate(source, (current, getter) => getter.Get(current));
			}
		}
	}
}