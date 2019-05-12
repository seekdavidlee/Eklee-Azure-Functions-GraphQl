using Eklee.Azure.Functions.GraphQl.Connections;
using Eklee.Azure.Functions.GraphQl.Repository.Search;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class FieldMutationResolver : IFieldMutationResolver
	{
		private readonly IGraphQlRepositoryProvider _graphQlRepositoryProvider;
		private readonly ISearchMappedModels _searchMappedModels;
		private readonly ILogger _logger;
		private readonly IConnectionEdgeResolver _connectionEdgeResolver;
		private readonly IConnectionEdgeHandler _connectionEdgeHandler;

		public FieldMutationResolver(
			IGraphQlRepositoryProvider graphQlRepositoryProvider,
			ISearchMappedModels searchMappedModels,
			ILogger logger,
			IConnectionEdgeResolver connectionEdgeResolver,
			IConnectionEdgeHandler connectionEdgeHandler)
		{
			_graphQlRepositoryProvider = graphQlRepositoryProvider;
			_searchMappedModels = searchMappedModels;
			_logger = logger;
			_connectionEdgeResolver = connectionEdgeResolver;
			_connectionEdgeHandler = connectionEdgeHandler;
		}

		public Func<ClaimsPrincipal, AssertAction, bool> ClaimsPrincipalAssertion { get; set; }

		public async Task<List<TSource>> BatchAddAsync<TSource>(ResolveFieldContext<object> context, string sourceName) where TSource : class
		{
			return await InternalBatchAsync<TSource>(context, sourceName, AssertAction.BatchCreate);
		}

		private void AssertWithClaimsPrincipal(AssertAction assertAction, ResolveFieldContext<object> context)
		{
			if (ClaimsPrincipalAssertion != null)
			{
				var graphRequestContext = context.UserContext as IGraphRequestContext;
				if (graphRequestContext == null ||
					!ClaimsPrincipalAssertion(graphRequestContext.HttpRequest.Security.ClaimsPrincipal, assertAction))
				{
					var message = $"{assertAction} execution has been denied due to insufficient permissions.";
					throw new ExecutionError(message, new SecurityException(message));
				}
			}
		}

		public async Task<TSource> DeleteAsync<TSource>(ResolveFieldContext<object> context, string sourceName) where TSource : class
		{
			AssertWithClaimsPrincipal(AssertAction.Delete, context);
			var item = context.GetArgument<TSource>(sourceName);

			await InternalDeleteAsync(context, item);

			return item;
		}

		private async Task InternalDeleteAsync<TSource>(ResolveFieldContext<object> context, TSource item) where TSource : class
		{
			try
			{
				IGraphRequestContext ctx = context.UserContext as IGraphRequestContext;

				await _connectionEdgeHandler.RemoveEdgeConnections(item, ctx);

				await _graphQlRepositoryProvider.GetRepository<TSource>().DeleteAsync(item, ctx);

				if (_searchMappedModels.TryGetMappedSearchType<TSource>(out var mappedSearchType))
				{
					var mappedInstance = _searchMappedModels.CreateInstanceFromMap(item);
					await _graphQlRepositoryProvider.GetRepository(mappedSearchType)
						.DeleteAsync(mappedSearchType, mappedInstance, ctx);
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "An error has occured while performing a delete operation.");
				throw;
			}
		}

		public async Task<TDeleteOutput> DeleteAsync<TSource, TDeleteInput, TDeleteOutput>(ResolveFieldContext<object> context, string sourceName,
		Func<TDeleteInput, TSource> mapDelete,
		Func<TSource, TDeleteOutput> transform) where TSource : class
		{
			AssertWithClaimsPrincipal(AssertAction.Delete, context);

			var arg = context.GetArgument<TDeleteInput>(sourceName);
			var item = mapDelete(arg);

			await InternalDeleteAsync(context, item);

			return transform(item);
		}

		public async Task<TDeleteOutput> DeleteAllAsync<TSource, TDeleteOutput>(ResolveFieldContext<object> context, string sourceName, Func<TDeleteOutput> getOutput) where TSource : class
		{
			AssertWithClaimsPrincipal(AssertAction.DeleteAll, context);
			try
			{
				var ctx = context.UserContext as IGraphRequestContext;

				await _connectionEdgeHandler.DeleteAllEdgeConnectionsOfType<TSource>(ctx);

				await _graphQlRepositoryProvider.GetRepository<TSource>().DeleteAllAsync<TSource>(ctx);

				if (_searchMappedModels.TryGetMappedSearchType<TSource>(out var mappedSearchType))
				{
					await _graphQlRepositoryProvider.GetRepository(mappedSearchType)
						.DeleteAllAsync(mappedSearchType, ctx);
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "An error has occured while performing a delete-all operation.");
				throw;
			}

			return getOutput();
		}

		public async Task<TSource> AddAsync<TSource>(ResolveFieldContext<object> context, string sourceName) where TSource : class
		{
			AssertWithClaimsPrincipal(AssertAction.Create, context);
			var item = context.GetArgument<TSource>(sourceName);

			var ctx = context.UserContext as IGraphRequestContext;
			try
			{
				var edges = _connectionEdgeResolver.HandleConnectionEdges(item, async (model) =>
				{
					await _graphQlRepositoryProvider.GetRepository(model.GetType())
						.AddAsync(model.GetType(), model, ctx);
				});

				if (edges.Count > 0)
					await _graphQlRepositoryProvider.GetRepository(typeof(ConnectionEdge)).BatchAddAsync(edges, ctx);

				if (_searchMappedModels.TryGetMappedSearchType<TSource>(out var mappedSearchType))
				{
					var mappedInstance = _searchMappedModels.CreateInstanceFromMap(item);
					await _graphQlRepositoryProvider.GetRepository(mappedSearchType)
						.AddAsync(mappedSearchType, mappedInstance, ctx);
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "An error has occured while performing an add operation.");
				throw;
			}
			return item;
		}

		public async Task<TSource> AddOrUpdateAsync<TSource>(ResolveFieldContext<object> context, string sourceName) where TSource : class
		{
			AssertWithClaimsPrincipal(AssertAction.Create, context);
			var item = context.GetArgument<TSource>(sourceName);
			var ctx = context.UserContext as IGraphRequestContext;
			try
			{
				var edges = _connectionEdgeResolver.HandleConnectionEdges(item, async (model) =>
				{
					await _graphQlRepositoryProvider.GetRepository(model.GetType())
						.AddOrUpdateAsync(model.GetType(), model, ctx);
				});

				if (edges.Count > 0)
					await _graphQlRepositoryProvider.GetRepository(typeof(ConnectionEdge))
						.BatchAddOrUpdateAsync(edges, ctx);

				if (_searchMappedModels.TryGetMappedSearchType<TSource>(out var mappedSearchType))
				{
					var mappedInstance = _searchMappedModels.CreateInstanceFromMap(item);
					await _graphQlRepositoryProvider.GetRepository(mappedSearchType)
						.AddAsync(mappedSearchType, mappedInstance, ctx);
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "An error has occured while performing an add operation.");
				throw;
			}
			return item;
		}

		public async Task<TSource> UpdateAsync<TSource>(ResolveFieldContext<object> context, string sourceName) where TSource : class
		{
			AssertWithClaimsPrincipal(AssertAction.Update, context);
			var item = context.GetArgument<TSource>(sourceName);
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
		}

		public async Task<List<TSource>> BatchAddOrUpdateAsync<TSource>(ResolveFieldContext<object> context, string sourceName) where TSource : class
		{
			return await InternalBatchAsync<TSource>(context, sourceName, AssertAction.BatchCreateOrUpdate);
		}

		private async Task<List<TSource>> InternalBatchAsync<TSource>(
			ResolveFieldContext<object> context, string sourceName,
			AssertAction assertAction) where TSource : class
		{
			AssertWithClaimsPrincipal(assertAction, context);

			var items = context.GetArgument<IEnumerable<TSource>>(sourceName).ToList();
			var ctx = context.UserContext as IGraphRequestContext;

			var batchItems = new Dictionary<Type, List<object>>();

			try
			{
				var edges = _connectionEdgeResolver.HandleConnectionEdges(items, (model) =>
				{
					var key = model.GetType();
					List<object> itemsList;

					if (batchItems.ContainsKey(key))
					{
						itemsList = batchItems[key];
					}
					else
					{
						itemsList = new List<object>();
						batchItems[key] = itemsList;
					}

					itemsList.Add(model);
				});

				switch (assertAction)
				{
					case AssertAction.BatchCreate:
						if (edges.Count > 0)
							await _graphQlRepositoryProvider.GetRepository(typeof(ConnectionEdge)).BatchAddAsync(edges, ctx);

						foreach (var batchItem in batchItems)
						{
							await _graphQlRepositoryProvider.GetRepository(batchItem.Key).BatchAddAsync(batchItem.Key, batchItem.Value, ctx);
						}

						break;

					case AssertAction.BatchCreateOrUpdate:
						if (edges.Count > 0)
							await _graphQlRepositoryProvider.GetRepository(typeof(ConnectionEdge)).BatchAddOrUpdateAsync(edges, ctx);

						foreach (var batchItem in batchItems)
						{
							await _graphQlRepositoryProvider.GetRepository(batchItem.Key).BatchAddOrUpdateAsync(batchItem.Key, batchItem.Value, ctx);
						}
						break;

					default:
						throw new InvalidOperationException("This internal method is only for batch operations.");
				}

				if (_searchMappedModels.TryGetMappedSearchType<TSource>(out var mappedSearchType))
				{
					var mappedInstances = items.Select(item => Convert.ChangeType(_searchMappedModels.CreateInstanceFromMap(item), mappedSearchType)).ToList();

					await _graphQlRepositoryProvider.GetRepository(mappedSearchType)
						.BatchAddAsync(mappedSearchType, mappedInstances, ctx);
				}

				return items;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "An error has occured while performing a batch add operation.");
				throw;
			}
		}
	}
}
