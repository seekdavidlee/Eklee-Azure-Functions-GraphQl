using System;
using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public static class DocumentDbConfigurationExtensions
	{
		public static void Add<TSource>(this Dictionary<string, object> configurations, string name, object value)
		{
			configurations[$"{typeof(TSource).Name}{name}"] = value;
		}

		public static T GetValue<T>(this Dictionary<string, object> configurations, string key, Type sourceType)
		{
			return (T)configurations[GetKey(key, sourceType)];
		}

		public static bool ContainsKey<TSource>(this Dictionary<string, object> configurations, string key)
		{
			return configurations.ContainsKey(GetKey(key, typeof(TSource)));
		}


		public static string GetStringValue(this Dictionary<string, object> configurations, string key, Type sourceType)
		{
			string configKey = GetKey(key, sourceType);
			return configurations.ContainsKey(configKey) ? (string)configurations[configKey] : null;
		}

		public static string GetKey(string key, Type sourceType)
		{
			return $"{sourceType.Name}{key}";
		}
	}
}