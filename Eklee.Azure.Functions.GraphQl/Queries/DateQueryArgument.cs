using Eklee.Azure.Functions.GraphQl.Filters;
using GraphQL.Builders;
using GraphQL.Types;
using System.Collections.Generic;

namespace Eklee.Azure.Functions.GraphQl.Queries
{
	public class DateQueryArgument : IModelMemberQueryArgument
	{
		public bool CanHandle(ModelMember modelMember)
		{
			return modelMember.IsDate;
		}

		private QueryArgument GetArgument(ModelMember modelMember)
		{
			return modelMember.IsOptional ?
				(QueryArgument)new QueryArgument<ModelConventionInputType<DateFilter>>
				{
					Name = modelMember.Name,
					Description = modelMember.Description
				} : new QueryArgument<NonNullGraphType<ModelConventionInputType<DateFilter>>>
				{
					Name = modelMember.Name,
					Description = modelMember.Description
				};
		}

		private ConnectionBuilder<object> GetConnectionBuilderArgument(ModelMember modelMember, ConnectionBuilder<object> connectionBuilder)
		{
			return modelMember.IsOptional ?
						connectionBuilder.Argument<ModelConventionInputType<DateFilter>>(modelMember.Name, modelMember.Description) :
						connectionBuilder.Argument<NonNullGraphType<ModelConventionInputType<DateFilter>>>(modelMember.Name, modelMember.Description);
		}

		public IEnumerable<QueryArgument> GetArguments(ModelMember modelMember)
		{
			return new List<QueryArgument> { GetArgument(modelMember) };
		}

		public IEnumerable<ConnectionBuilder<object>> GetConnectionBuilderArguments(
			ModelMember modelMember,
			ConnectionBuilder<object> connectionBuilder)
		{
			return new List<ConnectionBuilder<object>> { GetConnectionBuilderArgument(modelMember, connectionBuilder) };
		}
	}
}
