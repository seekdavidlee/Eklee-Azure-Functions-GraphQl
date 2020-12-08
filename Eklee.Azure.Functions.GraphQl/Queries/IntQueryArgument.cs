using System.Collections.Generic;
using Eklee.Azure.Functions.GraphQl.Filters;
using GraphQL.Builders;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl.Queries
{
	public class IntQueryArgument : IModelMemberQueryArgument
	{
		public bool CanHandle(ModelMember modelMember)
		{
			return modelMember.IsInt;
		}

		private QueryArgument GetArgument(ModelMember modelMember)
		{
			return modelMember.IsOptional ?
				(QueryArgument)new QueryArgument<ModelConventionInputType<IntFilter>>
				{
					Name = modelMember.Name,
					Description = modelMember.Description
				} :
				new QueryArgument<NonNullGraphType<ModelConventionInputType<IntFilter>>>
				{
					Name = modelMember.Name,
					Description = modelMember.Description
				};
		}

		public IEnumerable<QueryArgument> GetArguments(ModelMember modelMember)
		{
			return new List<QueryArgument> { GetArgument(modelMember) };
		}

		public ConnectionBuilder<ModelConventionType<TSource>> GetConnectionBuilderArgument<TSource>(ModelMember modelMember, ConnectionBuilder<ModelConventionType<TSource>> connectionBuilder)
		{
			return modelMember.IsOptional ?
						connectionBuilder.Argument<ModelConventionInputType<IntFilter>>(modelMember.Name, modelMember.Description) :
						connectionBuilder.Argument<NonNullGraphType<ModelConventionInputType<IntFilter>>>(modelMember.Name, modelMember.Description);
		}

		public IEnumerable<ConnectionBuilder<ModelConventionType<TSource>>> GetConnectionBuilderArguments<TSource>(
			ModelMember modelMember,
			ConnectionBuilder<ModelConventionType<TSource>> connectionBuilder)
		{
			return new List<ConnectionBuilder<ModelConventionType<TSource>>> { GetConnectionBuilderArgument(modelMember, connectionBuilder) };
		}
	}
}
