using System;
using System.Collections.Generic;
using Eklee.Azure.Functions.GraphQl.Repository;
using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelConventionInputBuilder<TSource>
	{
		private readonly ObjectGraphType _objectGraphType;
		private readonly IGraphQlRepositoryProvider _graphQlRepositoryProvider;
		private readonly string _sourceName;
		private Action _deleteSetupAction;
		private readonly Dictionary<string, string> _configurations = new Dictionary<string, string>();

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
						await _graphQlRepositoryProvider.GetRepository<TSource>().DeleteAsync(item);
						return item;
					});
			};
		}

		private IGraphQlRepository _graphQlRepository;
		public ModelConventionInputBuilder<TSource> Use<TType, TRepository>() where TRepository : IGraphQlRepository
		{
			_graphQlRepository = _graphQlRepositoryProvider.Use<TType, TRepository>();
			return this;
		}

		public HttpRepositoryConfiguration<TSource> ConfigureHttp()
		{
			return new HttpRepositoryConfiguration<TSource>(this);
		}

		public ModelConventionInputBuilder<TSource> AddConfiguration(string key, string value)
		{
			_configurations.Add(key, value);
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
						await _graphQlRepositoryProvider.GetRepository<TSource>().DeleteAsync(item);
						return transform(item);
					});
			};

			return this;
		}

		public void Build()
		{
			_graphQlRepository.Configure(_configurations);

			_objectGraphType.FieldAsync<ListGraphType<ModelConventionType<TSource>>>($"batchCreate{typeof(TSource).Name}", arguments: new QueryArguments(
					new QueryArgument<ListGraphType<ModelConventionInputType<TSource>>> { Name = _sourceName }
				),
				resolve: async context =>
				{
					var items = context.GetArgument<IEnumerable<TSource>>(_sourceName);
					await _graphQlRepositoryProvider.GetRepository<TSource>().BatchAddAsync(items);
					return items;
				});

			_objectGraphType.FieldAsync<ModelConventionType<TSource>>($"create{typeof(TSource).Name}", arguments: new QueryArguments(
					new QueryArgument<NonNullGraphType<ModelConventionInputType<TSource>>> { Name = _sourceName }
				),
				resolve: async context =>
				{
					var item = context.GetArgument<TSource>(_sourceName);
					await _graphQlRepositoryProvider.GetRepository<TSource>().AddAsync(item);
					return item;
				});

			_objectGraphType.FieldAsync<ModelConventionType<TSource>>($"update{typeof(TSource).Name}", arguments: new QueryArguments(
					new QueryArgument<NonNullGraphType<ModelConventionInputType<TSource>>> { Name = _sourceName }
				),
				resolve: async context =>
				{
					var item = context.GetArgument<TSource>(_sourceName);
					await _graphQlRepositoryProvider.GetRepository<TSource>().UpdateAsync(item);
					return item;
				});

			_deleteSetupAction();
		}
	}
}