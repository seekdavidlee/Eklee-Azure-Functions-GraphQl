using System.Collections.Generic;
using System.Linq;
using Eklee.Azure.Functions.GraphQl.Repository.Search;

namespace Eklee.Azure.Functions.GraphQl
{
	public static class QueryExtensions
	{
		public static List<T> GetTypeList<T>(this List<SearchResultModel> searchResultModels) where T : class
		{
			return searchResultModels.Select(x => x.Value as T).Where(x => x != null).ToList();
		}
	}
}
