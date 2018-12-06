using System;
using System.Collections.Generic;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelConventionInputBuilder<TSource>
	{
		private readonly ObjectGraphType _objectGraphType;
		private readonly IGraphQlRepositoryProvider _graphQlRepositoryProvider;
		private readonly string _sourceName;
		private Action _deleteSetupAction;

		internal ModelConventionInputBuilder(
			ObjectGraphType objectGraphType,
			IGraphQlRepositoryProvider graphQlRepositoryProviderProvider)
		{
			_objectGraphType = objectGraphType;
			_graphQlRepositoryProvider = graphQlRepositoryProviderProvider;
			_sourceName = typeof(TSource).Name.ToLower();

			// Default setup for delete.
			_deleteSetupAction = () =>
			{
				_objectGraphType.FieldAsync<ModelConventionType<TSource>>($"delete{typeof(TSource).Name}", arguments: new QueryArguments(
						new QueryArgument<NonNullGraphType<ModelConventionInputType<TSource>>> { Name = _sourceName }
					),
					resolve: async context =>
					{
						var item = context.GetArgument<TSource>(_sourceName);
						await _graphQlRepositoryProvider.DeleteAsync(item);
						return item;
					});
			};
		}

		public ModelConventionInputBuilder<TSource> Use<TType, TRepository>(Dictionary<string, string> configurations = null) where TRepository : IGraphQlRepository
		{
			_graphQlRepositoryProvider.Use<TType, TRepository>();
			return this;
		}

		public ModelConventionInputBuilder<TSource> Delete<TDeleteInput, TDeleteOutput>(Func<TSource, TDeleteOutput> transform)
		{
			_deleteSetupAction = () =>
			{
				_objectGraphType.FieldAsync<ModelConventionType<TDeleteInput>>($"delete{typeof(TSource).Name}", arguments: new QueryArguments(
						new QueryArgument<NonNullGraphType<ModelConventionInputType<TDeleteOutput>>> { Name = _sourceName }
					),
					resolve: async context =>
					{
						var item = context.GetArgument<TSource>(_sourceName);
						await _graphQlRepositoryProvider.DeleteAsync(item);
						return transform(item);
					});
			};

			return this;
		}

		public void Build()
		{
			_objectGraphType.FieldAsync<ModelConventionType<TSource>>($"create{typeof(TSource).Name}", arguments: new QueryArguments(
					new QueryArgument<NonNullGraphType<ModelConventionInputType<TSource>>> { Name = _sourceName }
				),
				resolve: async context =>
				{
					var item = context.GetArgument<TSource>(_sourceName);
					await _graphQlRepositoryProvider.AddAsync(item);
					return item;
				});

			_objectGraphType.FieldAsync<ModelConventionType<TSource>>($"update{typeof(TSource).Name}", arguments: new QueryArguments(
					new QueryArgument<NonNullGraphType<ModelConventionInputType<TSource>>> { Name = _sourceName }
				),
				resolve: async context =>
				{
					var item = context.GetArgument<TSource>(_sourceName);
					await _graphQlRepositoryProvider.UpdateAsync(item);
					return item;
				});

			_deleteSetupAction();
		}
	}
}