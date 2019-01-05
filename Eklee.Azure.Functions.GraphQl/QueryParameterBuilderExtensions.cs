using System;
using System.Collections.Generic;
using Eklee.Azure.Functions.GraphQl.Filters;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using GraphQL.Builders;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl
{
	public static class QueryParameterBuilderExtensions
	{
		public static QueryArguments GetQueryArguments<TSource>(this QueryParameterBuilder<TSource> queryParameterBuilder)
		{
			var queryArguments = new List<QueryArgument>();

			queryParameterBuilder.ForEach(modelMember =>
			{
				if (modelMember.SourceType == typeof(SearchModel) && modelMember.IsString)
				{
					if (modelMember.IsOptional)
					{
						queryArguments.Add(new QueryArgument<ModelConventionInputType<SearchFilter>>
						{
							Name = modelMember.Name,
							Description = modelMember.Description
						});
					}
					else
					{
						queryArguments.Add(new QueryArgument<NonNullGraphType<ModelConventionInputType<SearchFilter>>>
						{
							Name = modelMember.Name,
							Description = modelMember.Description
						});
					}

					return;
				}

				if (modelMember.IsString)
				{
					if (modelMember.IsOptional)
					{
						queryArguments.Add(new QueryArgument<ModelConventionInputType<StringFilter>>
						{
							Name = modelMember.Name,
							Description = modelMember.Description
						});
					}
					else
					{
						queryArguments.Add(new QueryArgument<NonNullGraphType<ModelConventionInputType<StringFilter>>>
						{
							Name = modelMember.Name,
							Description = modelMember.Description
						});
					}

					return;
				}

				if (modelMember.IsInt)
				{
					if (modelMember.IsOptional)
					{
						queryArguments.Add(new QueryArgument<ModelConventionInputType<IntFilter>>
						{
							Name = modelMember.Name,
							Description = modelMember.Description
						});
					}
					else
					{
						queryArguments.Add(new QueryArgument<NonNullGraphType<ModelConventionInputType<IntFilter>>>
						{
							Name = modelMember.Name,
							Description = modelMember.Description
						});
					}

					return;
				}

				if (modelMember.IsDate)
				{
					if (modelMember.IsOptional)
					{
						queryArguments.Add(new QueryArgument<ModelConventionInputType<DateFilter>>
						{
							Name = modelMember.Name,
							Description = modelMember.Description
						});
					}
					else
					{
						queryArguments.Add(new QueryArgument<NonNullGraphType<ModelConventionInputType<DateFilter>>>
						{
							Name = modelMember.Name,
							Description = modelMember.Description
						});
					}

					return;
				}

				if (modelMember.IsBool)
				{
					if (modelMember.IsOptional)
					{
						queryArguments.Add(new QueryArgument<ModelConventionInputType<BoolFilter>>
						{
							Name = modelMember.Name,
							Description = modelMember.Description
						});
					}
					else
					{
						queryArguments.Add(new QueryArgument<NonNullGraphType<ModelConventionInputType<BoolFilter>>>
						{
							Name = modelMember.Name,
							Description = modelMember.Description
						});
					}

					return;
				}

				if (modelMember.IsGuid)
				{
					if (modelMember.IsOptional)
					{
						queryArguments.Add(new QueryArgument<ModelConventionInputType<GuidFilter>>
						{
							Name = modelMember.Name,
							Description = modelMember.Description
						});
					}
					else
					{
						queryArguments.Add(new QueryArgument<NonNullGraphType<ModelConventionInputType<GuidFilter>>>
						{
							Name = modelMember.Name,
							Description = modelMember.Description
						});
					}

					return;
				}

				throw new NotImplementedException($"QueryArgument type is not yet implemented for {modelMember.Name}");
			});


			return new QueryArguments(queryArguments);
		}

		public static void PopulateWithArguments<TSource>(this QueryParameterBuilder<TSource> queryParameterBuilder,
			ConnectionBuilder<ModelConventionType<TSource>, object> connectionBuilder)
		{
			queryParameterBuilder.ForEach(modelMember =>
			{
				if (modelMember.SourceType == typeof(SearchModel) && modelMember.IsString)
				{
					connectionBuilder = modelMember.IsOptional ?
						connectionBuilder.Argument<ModelConventionInputType<SearchFilter>>(modelMember.Name, modelMember.Description) :
						connectionBuilder.Argument<NonNullGraphType<ModelConventionInputType<SearchFilter>>>(modelMember.Name, modelMember.Description);

					return;
				}

				if (modelMember.IsString)
				{
					connectionBuilder = modelMember.IsOptional ?
						connectionBuilder.Argument<ModelConventionInputType<StringFilter>>(modelMember.Name, modelMember.Description) :
						connectionBuilder.Argument<NonNullGraphType<ModelConventionInputType<StringFilter>>>(modelMember.Name, modelMember.Description);

					return;
				}

				if (modelMember.IsInt)
				{
					connectionBuilder = modelMember.IsOptional ?
						connectionBuilder.Argument<ModelConventionInputType<IntFilter>>(modelMember.Name, modelMember.Description) :
						connectionBuilder.Argument<NonNullGraphType<ModelConventionInputType<IntFilter>>>(modelMember.Name, modelMember.Description);

					return;
				}

				if (modelMember.IsDate)
				{
					connectionBuilder = modelMember.IsOptional ?
						connectionBuilder.Argument<ModelConventionInputType<DateFilter>>(modelMember.Name, modelMember.Description) :
						connectionBuilder.Argument<NonNullGraphType<ModelConventionInputType<DateFilter>>>(modelMember.Name, modelMember.Description);

					return;
				}

				if (modelMember.IsBool)
				{
					connectionBuilder = modelMember.IsOptional ?
						connectionBuilder.Argument<ModelConventionInputType<BoolFilter>>(modelMember.Name, modelMember.Description) :
						connectionBuilder.Argument<NonNullGraphType<ModelConventionInputType<BoolFilter>>>(modelMember.Name, modelMember.Description);

					return;
				}

				if (modelMember.IsGuid)
				{
					connectionBuilder = modelMember.IsOptional ?
						connectionBuilder.Argument<ModelConventionInputType<GuidFilter>>(modelMember.Name, modelMember.Description) :
						connectionBuilder.Argument<NonNullGraphType<ModelConventionInputType<GuidFilter>>>(modelMember.Name, modelMember.Description);

					return;
				}

				throw new NotImplementedException($"QueryArgument type is not yet implemented for {modelMember.Name}");
			});
		}
	}
}
