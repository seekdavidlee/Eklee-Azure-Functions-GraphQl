using System.Collections.Generic;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl
{
	public static class QueryParameterListExtensions
	{
		public static string GetCacheKey(this List<QueryParameter> list)
		{
			return string.Join("_", list.Where(x => x.HasContextValue).Select(x => x.ContextValue.Value.ToString()));
		}
	}
}
