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
						x => x.ContextValue?.Value != null && x.ContextValue.Comparison.HasValue).Select(
						x => $"{x.ContextValue.Comparison.Value}{ x.ContextValue.Value}")));
			});
			return all.ToString();
		}
	}
}
