using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using Eklee.Azure.Functions.GraphQl.Repository;
using Eklee.Azure.Functions.GraphQl.Repository.DocumentDb;
using Eklee.Azure.Functions.GraphQl.Repository.Http;
using Eklee.Azure.Functions.GraphQl.Repository.InMemory;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using Eklee.Azure.Functions.GraphQl.Repository.TableStorage;
using GraphQL;
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
		private readonly IQueryArgumentsBuilder _queryArgumentsBuilder;

		internal ModelConventionInputBuilder(
			ObjectGraphType objectGraphType,
			IGraphQlRepositoryProvider graphQlRepositoryProviderProvider,
			ILogger logger,
			ISearchMappedModels searchMappedModels,
			IQueryArgumentsBuilder queryArgumentsBuilder)
		{
			_objectGraphType = objectGraphType;
			_graphQlRepositoryProvider = graphQlRepositoryProviderProvider;
			_logger = logger;
			_searchMappedModels = searchMappedModels;
			_queryArgumentsBuilder = queryArgumentsBuilder;
			_sourceName = typeof(TSource).Name.ToLower();

			// Default setup for delete.
			_deleteSetupAction = () =>
			{
				var fieldName = $"delete{GetTypeName()}";

				if (_objectGraphType.HasField(fieldName)) return;

				_objectGraphType.FieldAsync<ModelConventionType<TSource>>(fieldName,
					description: $"Deletes a single {GetTypeName()} instance.",
					arguments: _queryArgumentsBuilder.BuildNonNull<TSource>(_sourceName),
					resolve: async context =>
					{
						AssertWithClaimsPrincipal(AssertAction.Delete, context);
						var item = context.GetArgument<TSource>(_sourceName);

						try
						{
							await _graphQlRepositoryProvider.GetRepository<TSource>().DeleteAsync(item, context.UserContext as IGraphRequestContext);
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

		private Func<ClaimsPrincipal, AssertAction, bool> _claimsPrincipalAssertion;
		public ModelConventionInputBuilder<TSource> AssertWithClaimsPrincipal(Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion)
		{
			_claimsPrincipalAssertion = claimsPrincipalAssertion;
			return this;
		}

		private void AssertWithClaimsPrincipal(AssertAction assertAction, ResolveFieldContext<object> context)
		{
			if (_claimsPrincipalAssertion != null)
			{
				var graphRequestContext = context.UserContext as IGraphRequestContext;
				if (graphRequestContext == null ||
					!_claimsPrincipalAssertion(graphRequestContext.HttpRequest.Security.ClaimsPrincipal, assertAction))
				{
					var message = $"{assertAction} execution has been denied due to insufficient permissions.";
					throw new ExecutionError(message, new SecurityException(message));
				}
			}
		}

		private IGraphQlRepository _graphQlRepository;
		private Type _typeSource;

		public InMemoryConfiguration<TSource> ConfigureInMemory<TType>()
		{
			_graphQlRepository = _graphQlRepositoryProvider.Use<TType, InMemoryRepository>();
			_typeSource = typeof(TType);
			return new InMemoryConfiguration<TSource>(this);
		}

		public TableStorageConfiguration<TSource> ConfigureTableStorage<TType>()
		{
			_graphQlRepository = _graphQlRepositoryProvider.Use<TType, TableStorageRepository>();
			_typeSource = typeof(TType);
			return new TableStorageConfiguration<TSource>(this, _graphQlRepository, _typeSource);
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
				var fieldName = $"delete{GetTypeName()}";

				if (_objectGraphType.HasField(fieldName)) return;

				_objectGraphType.FieldAsync<ModelConventionType<TDeleteOutput>>(fieldName,
					description: $"Deletes a single {GetTypeName()} instance.",
					arguments: _queryArgumentsBuilder.BuildNonNull<TDeleteInput>(_sourceName),
					resolve: async context =>
					{
						AssertWithClaimsPrincipal(AssertAction.Delete, context);
						var arg = context.GetArgument<TDeleteInput>(_sourceName);
						var item = mapDelete(arg);

						try
						{
							await _graphQlRepositoryProvider.GetRepository<TSource>().DeleteAsync(item, context.UserContext as IGraphRequestContext);

							if (_searchMappedModels.TryGetMappedSearchType<TSource>(out var mappedSearchType))
							{
								var mappedInstance = _searchMappedModels.CreateInstanceFromMap(item);
								await _graphQlRepositoryProvider.GetRepository(mappedSearchType)
									.DeleteAsync(mappedSearchType, mappedInstance, context.UserContext as IGraphRequestContext);
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
				var fieldName = $"deleteAll{GetTypeName()}";

				if (_objectGraphType.HasField(fieldName)) return;

				_objectGraphType.FieldAsync<ModelConventionType<TDeleteOutput>>(fieldName,
					description: $"Deletes all {GetTypeName()} instances.",
					resolve: async context =>
					{
						AssertWithClaimsPrincipal(AssertAction.DeleteAll, context);
						try
						{
							await _graphQlRepositoryProvider.GetRepository<TSource>().DeleteAllAsync<TSource>(context.UserContext as IGraphRequestContext);

							if (_searchMappedModels.TryGetMappedSearchType<TSource>(out var mappedSearchType))
							{
								await _graphQlRepositoryProvider.GetRepository(mappedSearchType).DeleteAllAsync(mappedSearchType, context.UserContext as IGraphRequestContext);
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

		private string GetTypeName()
		{
			return typeof(TSource).Name;
		}

		private void AddBatchCreateField()
		{
			var fieldName = $"batchCreate{GetTypeName()}";

			if (_objectGraphType.HasField(fieldName)) return;

			_objectGraphType.FieldAsync<ListGraphType<ModelConventionType<TSource>>>(fieldName,
				description: $"Batch create {GetTypeName()} instances.",
				arguments: _queryArgumentsBuilder.BuildList<TSource>(_sourceName),
				resolve: async context =>
				{
					AssertWithClaimsPrincipal(AssertAction.BatchCreate, context);
					var items = context.GetArgument<IEnumerable<TSource>>(_sourceName).ToList();
					try
					{
						await _graphQlRepositoryProvider.GetRepository<TSource>().BatchAddAsync(items, context.UserContext as IGraphRequestContext);

						if (_searchMappedModels.TryGetMappedSearchType<TSource>(out var mappedSearchType))
						{
							var mappedInstances = items.Select(item => Convert.ChangeType(_searchMappedModels.CreateInstanceFromMap(item), mappedSearchType)).ToList();

							await _graphQlRepositoryProvider.GetRepository(mappedSearchType)
								.BatchAddAsync(mappedSearchType, mappedInstances, context.UserContext as IGraphRequestContext);
						}

						return items;
					}
					catch (Exception e)
					{
						_logger.LogError(e, "An error has occured while performing a batch add operation.");
						throw;
					}
				});
		}

		private void AddCreateField()
		{
			var fieldName = $"create{typeof(TSource).Name}";

			if (_objectGraphType.HasField(fieldName)) return;

			_objectGraphType.FieldAsync<ModelConventionType<TSource>>(fieldName, 
				description: $"Creates a single {GetTypeName()} instance.", 
				arguments: _queryArgumentsBuilder.BuildNonNull<TSource>(_sourceName),
				resolve: async context =>
				{
					AssertWithClaimsPrincipal(AssertAction.Create, context);
					var item = context.GetArgument<TSource>(_sourceName);

					try
					{
						await _graphQlRepositoryProvider.GetRepository<TSource>().AddAsync(item, context.UserContext as IGraphRequestContext);

						if (_searchMappedModels.TryGetMappedSearchType<TSource>(out var mappedSearchType))
						{
							var mappedInstance = _searchMappedModels.CreateInstanceFromMap(item);
							await _graphQlRepositoryProvider.GetRepository(mappedSearchType)
						.AddAsync(mappedSearchType, mappedInstance, context.UserContext as IGraphRequestContext);
						}
					}
					catch (Exception e)
					{
						_logger.LogError(e, "An error has occured while performing an add operation.");
						throw;
					}
					return item;
				});
		}

		private void AddUpdateField()
		{
			var fieldName = $"update{GetTypeName()}";

			if (_objectGraphType.HasField(fieldName)) return;

			_objectGraphType.FieldAsync<ModelConventionType<TSource>>(fieldName, 
				description: $"Updates a single {GetTypeName()} instance.", 
				arguments:_queryArgumentsBuilder.BuildNonNull<TSource>(_sourceName),
				resolve: async context =>
				{
					AssertWithClaimsPrincipal(AssertAction.Update, context);
					var item = context.GetArgument<TSource>(_sourceName);
					try
					{
						await _graphQlRepositoryProvider.GetRepository<TSource>().UpdateAsync(item, context.UserContext as IGraphRequestContext);

						if (_searchMappedModels.TryGetMappedSearchType<TSource>(out var mappedSearchType))
						{
							var mappedInstance = _searchMappedModels.CreateInstanceFromMap(item);
							await _graphQlRepositoryProvider.GetRepository(mappedSearchType)
								.UpdateAsync(mappedSearchType, mappedInstance, context.UserContext as IGraphRequestContext);
						}
					}
					catch (Exception e)
					{
						_logger.LogError(e, "An error has occured while performing an update operation.");
						throw;
					}
					return item;
				});
		}

		public void Build()
		{
			AddBatchCreateField();

			AddCreateField();

			AddUpdateField();

			_deleteSetupAction();

			_deleteAllAction?.Invoke();
		}
	}
}