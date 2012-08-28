using System;
using System.Collections.Generic;
using System.Data;

namespace TinyORM
{
	public class CommandBuilder_old
	{
		private readonly IDbCommand _command;
		private readonly MapConfiguration _configuration;
		private IDictionary<string, List<PropertyGetter>> _cache;

		public CommandBuilder_old(IDbCommand command, MapConfiguration configuration)
		{
			_command = command;
			_configuration = configuration;
			_cache = new Dictionary<string, List<PropertyGetter>>();
		}

		public void CreateCommand(string sql, object @params) { throw new NotImplementedException(); }
		public void CreateSelectCommand(object @params) { throw new NotImplementedException(); }
		public void CreateInsertCommand(object @params) { throw new NotImplementedException(); }
		public void CreateUpdateCommand(object @params) { throw new NotImplementedException(); }
		public void CreateUpdateCommand(object @setParams, object @whereParams) { throw new NotImplementedException(); }
		public void CreateDeleteCommand(object @params) { throw new NotImplementedException(); }


		//public void AddParameters(string sql, object @params = null, Type mapType = null)
		//{
		//   ResetCommand(sql);

		//   if (@params == null)
		//      return;

		//   mapType = mapType ?? @params.GetType();
		//   var key = @params.GetType().FullName + mapType.FullName;

		//   List<PropertyGetter> propertyGetters;
		//   if (!_cache.TryGetValue(key, out propertyGetters))
		//   {
		//      var typeMap = _configuration.Types.GetOrCreate(mapType ?? @params.GetType());
		//      var propertyMaps = typeMap.Properties.ToDictionary(x => x.PropertyName.ToLower());
		//      foreach (var pair in propertyMaps.Where(pair => pair.Key.Contains(".")))
		//         propertyMaps.Add(pair.Key.Replace(".", ""), pair.Value);

		//      var type = @params.GetType();
		//      propertyGetters = GetPropertyGetters(type, propertyMaps, mapType).ToList();
		//      _cache.Add(key, propertyGetters);
		//   }
		//}

		//private void ResetCommand(string sql = null)
		//{
		//   _command.CommandText = sql ?? string.Empty;
		//   _command.Parameters.Clear();
		//}

		//private static IEnumerable<PropertyGetter> GetPropertyGetters(Type type, IDictionary<string, PropertyMap> propertyMaps, MemberInfo mapType)
		//{
		//   throw new NotImplementedException();
		//   //var properties = type.GetProperties();
		//   //foreach (var property in properties)
		//   //{
		//   //   if (property.PropertyType == mapType)
		//   //      foreach (var getter in GetPropertyGetters(property.PropertyType, propertyMaps, null))
		//   //         yield return new PropertyGetter
		//   //            {
		//   //               PropertyName = string.Format("{0}.{1}", property.Name, getter.PropertyName),
		//   //               ValueSerializer = getter.ValueSerializer
		//   //            };

		//   //   PropertyMap propertyMap;
		//   //   if (propertyMaps.TryGetValue(property.Name.ToLower(), out propertyMap))
		//   //      yield return new PropertyGetter
		//   //         {
		//   //            PropertyName = property.Name,
		//   //            ValueSerializer = propertyMap.ValueSerializer
		//   //         };
		//   //}
		//}
	}
}