using System;
using System.Collections.Generic;
using Eklee.Azure.Functions.GraphQl.Repository;
using Eklee.Azure.Functions.Http;
using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelConventionInputBuilder<TSource>
	{
		private readonly ObjectGraphType _objectGraphType;
		private readonly IGraphQlRepositoryProvider _graphQlRepositoryProvider;
		private readonly ILogger _logger;
		private readonly IHttpRequestContext _httpRequestContext;
		private readonly string _sourceName;
		private Action _deleteSetupAction;

		internal ModelConventionInputBuilder(
			ObjectGraphType objectGraphType,
			IGraphQlRepositoryProvider graphQlRepositoryProviderProvider,
			ILogger logger,
			IHttpRequestContext httpRequestContext)
		{
			_objectGraphType = objectGraphType;
			_graphQlRepositoryProvider = graphQlRepositoryProviderProvider;
			_logger = logger;
			_httpRequestContext = httpRequestContext;
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

						try
						{
							await _graphQlRepositoryProvider.GetRepository<TSource>().DeleteAsync(item);
						}
						catch (Exception e)
						{
							_logger.LogError(e, "An error has occured while performing a delete operation.");
							throw;
						}
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

		public HttpConfiguration<TSource> ConfigureHttp()
		{
			return new HttpConfiguration<TSource>(this, _graphQlRepository, _typeSource);
		}

		public DocumentDbConfiguration<TSource> ConfigureDocumentDb()
		{
			return new DocumentDbConfiguration<TSource>(this, _graphQlRepository, _typeSource, _httpRequestContext);
		}

		public ModelConventionInputBuilder<TSource> Delete<TDeleteInput, TDeleteOutput>(
			Func<TDeleteInput, TSource> mapDelete,
			Func<TSource, TDeleteOutput> transform)
		{
			_deleteSetupAction = () =>
			{
				_objectGraphType.FieldAsync<ModelConventionType<TDeleteOutput>>($"delete{typeof(TSource).Name}", arguments: new QueryArguments(
						new QueryArgument<NonNullGraphType<ModelConventionInputType<TDeleteInput>>> { Name = _sourceName }
					),
					resolve: async context =>
					{
						var arg = context.GetArgument<TDeleteInput>(_sourceName);
						var item = mapDelete(arg);

						try
						{
							await _graphQlRepositoryProvider.GetRepository<TSource>().DeleteAsync(item);
						}
						catch (Exception e)
						{
							_logger.LogError(e, "An error has occured while performing a delete operation.");
							throw;
						}
						
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

					try
					{
						await _graphQlRepositoryProvider.GetRepository<TSource>().AddAsync(item);
					}
					catch (Exception e)
					{
						_logger.LogError(e, "An error has occured while performing an add operation.");
						throw;
					}
					return item;
				});

			_objectGraphType.FieldAsync<ModelConventionType<TSource>>($"update{typeof(TSource).Name}", arguments: new QueryArguments(
					new QueryArgument<NonNullGraphType<ModelConventionInputType<TSource>>> { Name = _sourceName }
				),
				resolve: async context =>
				{
					var item = context.GetArgument<TSource>(_sourceName);
					try
					{
						await _graphQlRepositoryProvider.GetRepository<TSource>().UpdateAsync(item);
					}
					catch (Exception e)
					{
						_logger.LogError(e, "An error has occured while performing an update operation.");
						throw;
					}
					return item;
				});

			_deleteSetupAction();
		}
	}
}