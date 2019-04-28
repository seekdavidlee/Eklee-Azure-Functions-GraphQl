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
			AssertWithClaimsPrincipal(AssertAction.BatchCreate, context);
			var items = context.GetArgument<IEnumerable<TSource>>(sourceName).ToList();
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
				await _graphQlRepositoryProvider.GetRepository<TSource>().DeleteAllAsync<TSource>(context.UserContext as IGraphRequestContext);

				if (_searchMappedModels.TryGetMappedSearchType<TSource>(out var mappedSearchType))
				{
					await _graphQlRepositoryProvider.GetRepository(mappedSearchType)
						.DeleteAllAsync(mappedSearchType, context.UserContext as IGraphRequestContext);
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

			try
			{
				var edges = _connectionEdgeResolver.HandleConnectionEdges(item, async (model) =>
				{
					await _graphQlRepositoryProvider.GetRepository(model.GetType())
						.AddAsync(model.GetType(), model, context.UserContext as IGraphRequestContext);
				});

				foreach (var edge in edges)
				{
					await _graphQlRepositoryProvider.GetRepository(typeof(ConnectionEdge))
						.AddAsync(edge, context.UserContext as IGraphRequestContext);
				}

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
		}

		public async Task<TSource> AddOrUpdateAsync<TSource>(ResolveFieldContext<object> context, string sourceName) where TSource : class
		{
			AssertWithClaimsPrincipal(AssertAction.Create, context);
			var item = context.GetArgument<TSource>(sourceName);

			try
			{
				var edges = _connectionEdgeResolver.HandleConnectionEdges(item, async (model) =>
				{
					await _graphQlRepositoryProvider.GetRepository(model.GetType())
						.AddOrUpdateAsync(model.GetType(), model, context.UserContext as IGraphRequestContext);
				});

				foreach (var edge in edges)
				{
					await _graphQlRepositoryProvider.GetRepository(typeof(ConnectionEdge))
						.AddOrUpdateAsync(edge, context.UserContext as IGraphRequestContext);
				}

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
	}
}
