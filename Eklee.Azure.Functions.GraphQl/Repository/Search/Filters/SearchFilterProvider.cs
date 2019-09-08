using FastMember;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search.Filters
{
	public class SearchFilterProvider : ISearchFilterProvider
	{
		private readonly IEnumerable<ISearchFilter> _searchFilters;

		public SearchFilterProvider(IEnumerable<ISearchFilter> searchFilters)
		{
			_searchFilters = searchFilters;
		}

		public string GenerateStringFilter(IEnumerable<QueryParameter> queryParameters, MemberSet members)
		{
			var filterQp = queryParameters.SingleOrDefault(x => x.MemberModel.Name == "filters");
			if (filterQp != null && filterQp.ContextValue != null &&
				filterQp.ContextValue.Values != null)
			{
				var sb = new StringBuilder();
				filterQp.ContextValue.Values.ForEach(value =>
				{
					var searchFilterModel = (SearchFilterModel)value;

					var member = members.SingleOrDefault(x => x.Name.ToLower() == searchFilterModel.FieldName.ToLower());

					if (member == null)
					{
						throw new ArgumentException($"{searchFilterModel.FieldName} is not valid.");
					}

					var filter = _searchFilters.SingleOrDefault(x => x.CanHandle(searchFilterModel.Comprison, member));

					if (filter != null)
					{
						sb.Append($"{filter.GetFilter(searchFilterModel, member)} and ");
					}
				});

				return sb.ToString().TrimEnd("and ".ToCharArray());
			}

			return null;
		}
	}
}
