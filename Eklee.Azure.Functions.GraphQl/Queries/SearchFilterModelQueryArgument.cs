using Eklee.Azure.Functions.GraphQl.Filters;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using GraphQL.Builders;
using GraphQL.Types;
using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl.Queries
{
	public class SearchFilterModelQueryArgument : IModelMemberQueryArgument
	{
		public bool CanHandle(ModelMember modelMember)
		{
			return modelMember.Member.Type == typeof(List<SearchFilterModel>);
		}

		public IEnumerable<QueryArgument> GetArguments(ModelMember modelMember)
		{
			return new List<QueryArgument>
			{
				new QueryArgument<ListGraphType<ModelConventionInputType<FieldNameValueFilter>>>
				{
					Name = modelMember.Name,
					Description = modelMember.Description
				}
			};
		}

		public IEnumerable<ConnectionBuilder<object>> GetConnectionBuilderArguments(ModelMember modelMember, ConnectionBuilder<object> connectionBuilder)
		{
			throw new System.NotImplementedException();
		}
	}
}
