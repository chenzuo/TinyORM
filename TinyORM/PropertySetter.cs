using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TinyORM
{
	internal class PropertySetter
	{
		public static PropertySetter Create(Type targetType, string propertyName)
		{
			if (!propertyName.Contains('.'))
				return new PropertySetter(targetType, propertyName);

			var sourceType = targetType;
			var names = propertyName.Split('.');
			var getters = new List<PropertyGetter>();
			foreach (var name in names.Take(names.Length - 1))
			{
				getters.Add(PropertyGetter.Create(sourceType, name));
				sourceType = sourceType.GetProperty(name).PropertyType;
			}

			var setter = new PropertySetter(sourceType, names.Last());
			return new PropertyChainSetter(targetType, propertyName, getters, setter);
		}

		public Type TargetType { get; set; }
		public string PropertyName { get; set; }
		private volatile Action<object, object> _setter;

		private PropertySetter(Type targetType, string propertyName)
		{
			TargetType = targetType;
			PropertyName = propertyName;
		}

		public virtual void Set(object target, object value)
		{
			if (_setter == null)
				lock (this)
					if (_setter == null)
						_setter = CreateSetter();

			_setter(target, value);
		}

		private Action<object, object> CreateSetter()
		{
			var propertyInfo = TargetType.GetProperty(PropertyName);
			var xObjectTarget = Expression.Parameter(typeof(object), "target");
			var xObjectValue = Expression.Parameter(typeof(object), "value");
			var xTarget = Expression.Convert(xObjectTarget, TargetType);
			var xValue = Expression.Convert(xObjectValue, propertyInfo.PropertyType);
			var xProperty = Expression.Property(xTarget, PropertyName);
			var xAssign = Expression.Assign(xProperty, xValue);
			var xLambda = Expression.Lambda<Action<object, object>>(xAssign, xObjectTarget, xObjectValue);
			return xLambda.Compile();
		}

		private class PropertyChainSetter : PropertySetter
		{
			private readonly IEnumerable<PropertyGetter> _getters;
			private readonly PropertySetter _setter;

			internal PropertyChainSetter(Type targetType, string propertyName, IEnumerable<PropertyGetter> getters, PropertySetter setter)
				: base(targetType, propertyName)
			{
				_getters = getters;
				_setter = setter;
			}

			public override void Set(object target, object value)
			{
				target = _getters.Aggregate(target, (current, getter) => getter.Get(current));
				_setter.Set(target, value);
			}
		}
	}
}