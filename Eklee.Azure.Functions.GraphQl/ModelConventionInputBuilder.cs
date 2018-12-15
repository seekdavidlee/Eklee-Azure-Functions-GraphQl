using System;
using System.Collections.Generic;
using Eklee.Azure.Functions.GraphQl.Repository;
using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelConventionInputBuilder<TSource>
	{
		private readonly ObjectGraphType _objectGraphType;
		private readonly IGraphQlRepositoryProvider _graphQlRepositoryProvider;
		private readonly ILogger _logger;
		private readonly string _sourceName;
		private Action _deleteSetupAction;

		internal ModelConventionInputBuilder(
			ObjectGraphType objectGraphType,
			IGraphQlRepositoryProvider graphQlRepositoryProviderProvider,
			ILogger logger)
		{
			_objectGraphType = objectGraphType;
			_graphQlRepositoryProvider = graphQlRepositoryProviderProvider;
			_logger = logger;
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
		private Type _typeSource;

		public ModelConventionInputBuilder<TSource> Use<TType, TRepository>() where TRepository : IGraphQlRepository
		{
			_graphQlRepository = _graphQlRepositoryProvider.Use<TType, TRepository>();
			_typeSource = typeof(TType);
			return this;
		}

		private HttpConfiguration<TSource> _httpRepositoryConfiguration;

		public HttpConfiguration<TSource> ConfigureHttp()
		{
			_httpRepositoryConfiguration = new HttpConfiguration<TSource>(this, _graphQlRepository, _typeSource);
			return _httpRepositoryConfiguration;
		}

		private DocumentDbConfiguration<TSource> _documentDbConfiguration;

		public DocumentDbConfiguration<TSource> ConfigureDocumentDb()
		{
			_documentDbConfiguration = new DocumentDbConfiguration<TSource>(this);
			return _documentDbConfiguration;
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
			_objectGraphType.FieldAsync<ListGraphType<ModelConventionType<TSource>>>($"batchCreate{typeof(TSource).Name}", arguments: new QueryArguments(
					new QueryArgument<ListGraphType<ModelConventionInputType<TSource>>> { Name = _sourceName }
				),
				resolve: async context =>
				{
					var items = context.GetArgument<IEnumerable<TSource>>(_sourceName);
					try
					{
						await _graphQlRepositoryProvider.GetRepository<TSource>().BatchAddAsync(items);
						return items;
					}
					catch (Exception e)
					{
						_logger.LogError(e, "An error has occured while performing a batch add operation.");
						throw;
					}
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