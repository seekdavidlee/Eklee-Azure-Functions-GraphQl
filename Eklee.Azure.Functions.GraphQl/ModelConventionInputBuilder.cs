using System;
using System.Collections.Generic;
using System.Linq;
using Eklee.Azure.Functions.GraphQl.Repository;
using Eklee.Azure.Functions.GraphQl.Repository.DocumentDb;
using Eklee.Azure.Functions.GraphQl.Repository.Http;
using Eklee.Azure.Functions.GraphQl.Repository.InMemory;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using GraphQL.Types;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl
{
	public class ModelConventionInputBuilder<TSource> : IModelConventionInputBuilder<TSource> where TSource : class
	{
		private readonly ObjectGraphType _objectGraphType;
		private readonly IGraphQlRepositoryProvider _graphQlRepositoryProvider;
		private readonly ILogger _logger;
		private readonly string _sourceName;
		private Action _deleteSetupAction;
		private readonly ISearchMappedModels _searchMappedModels;

		internal ModelConventionInputBuilder(
			ObjectGraphType objectGraphType,
			IGraphQlRepositoryProvider graphQlRepositoryProviderProvider,
			ILogger logger,
			ISearchMappedModels searchMappedModels)
		{
			_objectGraphType = objectGraphType;
			_graphQlRepositoryProvider = graphQlRepositoryProviderProvider;
			_logger = logger;
			_searchMappedModels = searchMappedModels;
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

		public InMemoryConfiguration<TSource> ConfigureInMemory<TType>()
		{
			_graphQlRepository = _graphQlRepositoryProvider.Use<TType, InMemoryRepository>();
			_typeSource = typeof(TType);
			return new InMemoryConfiguration<TSource>(this);
		}

		public HttpConfiguration<TSource> ConfigureHttp<TType>()
		{
			_graphQlRepository = _graphQlRepositoryProvider.Use<TType, HttpRepository>();
			_typeSource = typeof(TType);
			return new HttpConfiguration<TSource>(this, _graphQlRepository, _typeSource);
		}

		public DocumentDbConfiguration<TSource> ConfigureDocumentDb<TType>()
		{
			_graphQlRepository = _graphQlRepositoryProvider.Use<TType, DocumentDbRepository>();
			_typeSource = typeof(TType);
			return new DocumentDbConfiguration<TSource>(this, _graphQlRepository, _typeSource);
		}

		public SearchConfiguration<TSource> ConfigureSearch<TSearchModel>()
		{
			_graphQlRepositoryProvider.Use<SearchModel, SearchRepository>();

			_graphQlRepository = _graphQlRepositoryProvider.Use<TSearchModel, SearchRepository>();
			_typeSource = typeof(TSearchModel);
			return new SearchConfiguration<TSource>(this, _graphQlRepository);
		}

		public SearchConfiguration<TSource> ConfigureSearchWith<TSearchModel, TModel>()
		{
			_searchMappedModels.Map<TSearchModel, TModel>();
			return ConfigureSearch<TSearchModel>();
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

							Type mappedSearchType;
							if (_searchMappedModels.TryGetMappedSearchType<TSource>(out mappedSearchType))
							{
								var mappedInstance = _searchMappedModels.CreateInstanceFromMap(item);
								await _graphQlRepositoryProvider.GetRepository(mappedSearchType)
									.DeleteAsync(mappedSearchType, mappedInstance);
							}
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

		private Action _deleteAllAction;

		public ModelConventionInputBuilder<TSource> DeleteAll<TDeleteOutput>(Func<TDeleteOutput> getOutput)
		{
			_deleteAllAction = () =>
			{
				_objectGraphType.FieldAsync<ModelConventionType<TDeleteOutput>>($"deleteAll{typeof(TSource).Name}",
					resolve: async context =>
					{
						try
						{
							await _graphQlRepositoryProvider.GetRepository<TSource>().DeleteAllAsync<TSource>();

							Type mappedSearchType;
							if (_searchMappedModels.TryGetMappedSearchType<TSource>(out mappedSearchType))
							{
								await _graphQlRepositoryProvider.GetRepository(mappedSearchType).DeleteAllAsync(mappedSearchType);
							}
						}
						catch (Exception e)
						{
							_logger.LogError(e, "An error has occured while performing a delete-all operation.");
							throw;
						}

						return getOutput();
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
					var items = context.GetArgument<IEnumerable<TSource>>(_sourceName).ToList();
					try
					{
						await _graphQlRepositoryProvider.GetRepository<TSource>().BatchAddAsync(items);

						Type mappedSearchType;
						if (_searchMappedModels.TryGetMappedSearchType<TSource>(out mappedSearchType))
						{
							var mappedInstances = items.Select(item => Convert.ChangeType(_searchMappedModels.CreateInstanceFromMap(item), mappedSearchType)).ToList();

							await _graphQlRepositoryProvider.GetRepository(mappedSearchType)
								.BatchAddAsync(mappedSearchType, mappedInstances);
						}

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

						Type mappedSearchType;
						if (_searchMappedModels.TryGetMappedSearchType<TSource>(out mappedSearchType))
						{
							var mappedInstance = _searchMappedModels.CreateInstanceFromMap(item);
							await _graphQlRepositoryProvider.GetRepository(mappedSearchType)
								.AddAsync(mappedSearchType, mappedInstance);
						}
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

						Type mappedSearchType;
						if (_searchMappedModels.TryGetMappedSearchType<TSource>(out mappedSearchType))
						{
							var mappedInstance = _searchMappedModels.CreateInstanceFromMap(item);
							await _graphQlRepositoryProvider.GetRepository(mappedSearchType)
								.UpdateAsync(mappedSearchType, mappedInstance);
						}
					}
					catch (Exception e)
					{
						_logger.LogError(e, "An error has occured while performing an update operation.");
						throw;
					}
					return item;
				});

			_deleteSetupAction();

			_deleteAllAction?.Invoke();
		}
	}
}