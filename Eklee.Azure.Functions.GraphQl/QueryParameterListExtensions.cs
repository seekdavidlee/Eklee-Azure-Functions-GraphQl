using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eklee.Azure.Functions.GraphQl
{
	public static class QueryParameterListExtensions
	{
		public static string GetCacheKey<TSource>(this List<QueryStep> steps)
		{
			var all = new StringBuilder(typeof(TSource).FullName);
			steps.ForEach(list =>
			{
				all.Append(string.Join("_",
					list.QueryParameters.Where(
						x => x.ContextValue?.Values != null && x.ContextValue.Comparison.HasValue).Select(
						x => $"{x.ContextValue.Comparison.Value}{ GetValue(x.ContextValue.Values)}")));
			});
			return all.ToString();
		}

		private static string GetValue(List<object> list)
		{
			return string.Join("_", list.Select(x => x.ToString()));
		}
	}
}
