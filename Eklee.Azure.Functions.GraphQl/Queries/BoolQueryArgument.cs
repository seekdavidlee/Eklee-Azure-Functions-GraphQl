using System.Collections.Generic;
using Eklee.Azure.Functions.GraphQl.Filters;
using GraphQL.Builders;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Queries
{
	public class BoolQueryArgument : IModelMemberQueryArgument
	{
		public bool CanHandle(ModelMember modelMember)
		{
			return modelMember.IsBool;
		}

		private QueryArgument GetArgument(ModelMember modelMember)
		{
			return modelMember.IsOptional ?
				(QueryArgument)new QueryArgument<ModelConventionInputType<BoolFilter>>
				{
					Name = modelMember.Name,
					Description = modelMember.Description
				} :
				new QueryArgument<NonNullGraphType<ModelConventionInputType<BoolFilter>>>
				{
					Name = modelMember.Name,
					Description = modelMember.Description
				};
		}

		private ConnectionBuilder<ModelConventionType<TSource>> GetConnectionBuilderArgument<TSource>(ModelMember modelMember, ConnectionBuilder<ModelConventionType<TSource>> connectionBuilder)
		{
			return modelMember.IsOptional ?
						connectionBuilder.Argument<ModelConventionInputType<BoolFilter>>(modelMember.Name, modelMember.Description) :
						connectionBuilder.Argument<NonNullGraphType<ModelConventionInputType<BoolFilter>>>(modelMember.Name, modelMember.Description);
		}

		public IEnumerable<QueryArgument> GetArguments(ModelMember modelMember)
		{
			return new List<QueryArgument> { GetArgument(modelMember) };
		}

		public IEnumerable<ConnectionBuilder<ModelConventionType<TSource>>> GetConnectionBuilderArguments<TSource>(
			ModelMember modelMember,
			ConnectionBuilder<ModelConventionType<TSource>> connectionBuilder)
		{
			return new List<ConnectionBuilder<ModelConventionType<TSource>>> { GetConnectionBuilderArgument(modelMember, connectionBuilder) };
		}
	}
}
