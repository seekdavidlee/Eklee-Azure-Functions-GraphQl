using System;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public static class Extensions
	{
		public static Uri GetSearchServiceUri(this string serviceName)
		{
			return new Uri($"https://{serviceName}.search.windows.net.");
		}
	}
}
