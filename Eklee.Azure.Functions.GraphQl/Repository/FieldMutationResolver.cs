using Eklee.Azure.Functions.GraphQl.Actions;
using Eklee.Azure.Functions.GraphQl.Connections;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
		private readonly ILogger _logger;
		private readonly IConnectionEdgeResolver _connectionEdgeResolver;
		private readonly IMutationActionsProvider _mutationActionsProvider;
		private readonly IModelTransformerProvider _modelTransformerProvider;

		public FieldMutationResolver(
			IGraphQlRepositoryProvider graphQlRepositoryProvider,
			ILogger logger,
			IConnectionEdgeResolver connectionEdgeResolver,
			IMutationActionsProvider mutationActionsProvider,
			IModelTransformerProvider modelTransformerProvider)
		{
			_graphQlRepositoryProvider = graphQlRepositoryProvider;
			_logger = logger;
			_connectionEdgeResolver = connectionEdgeResolver;
			_mutationActionsProvider = mutationActionsProvider;
			_modelTransformerProvider = modelTransformerProvider;
		}

		public async Task<List<TSource>> BatchAddAsync<TSource>(ResolveFieldContext<object> context, string sourceName, Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion) where TSource : class
		{
			return await InternalBatchAsync<TSource>(context, sourceName, AssertAction.BatchCreate, claimsPrincipalAssertion);
		}

		private void AssertWithClaimsPrincipal(AssertAction assertAction, ResolveFieldContext<object> context, Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion)
		{
			if (claimsPrincipalAssertion != null)
			{
				var graphRequestContext = context.UserContext as IGraphRequestContext;
				if (graphRequestContext == null ||
					graphRequestContext.HttpRequest.Security.ClaimsPrincipal == null ||
					!claimsPrincipalAssertion(graphRequestContext.HttpRequest.Security.ClaimsPrincipal, assertAction))
				{
					var message = $"{assertAction} execution has been denied due to insufficient permissions.";
					throw new ExecutionError(message, new SecurityException(message));
				}
			}
		}

		public async Task<TSource> DeleteAsync<TSource>(ResolveFieldContext<object> context, string sourceName, Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion) where TSource : class
		{
			AssertWithClaimsPrincipal(AssertAction.Delete, context, claimsPrincipalAssertion);

			var item = context.GetArgument<TSource>(sourceName);

			return await InternalDeleteAsync(context, item);
		}

		private async Task<TSource> InternalDeleteAsync<TSource>(ResolveFieldContext<object> context, TSource item) where TSource : class
		{
			try
			{
				IGraphRequestContext ctx = context.UserContext as IGraphRequestContext;

				await TransformObject(item, ctx, MutationActions.Delete);

				var mutationActionItem = new MutationActionItem<TSource>
				{
					Action = MutationActions.Delete,
					RequestContext = ctx,
					Item = item,
					RepositoryProvider = _graphQlRepositoryProvider
				};

				var cloned = item.Clone();

				await _mutationActionsProvider.HandlePreActions(mutationActionItem);

				await _graphQlRepositoryProvider.GetRepository<TSource>().DeleteAsync(item, ctx);

				await _mutationActionsProvider.HandlePostActions(mutationActionItem);

				return cloned;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "An error has occured while performing a delete operation.");
				throw;
			}
		}

		public async Task<TDeleteOutput> DeleteAsync<TSource, TDeleteInput, TDeleteOutput>(ResolveFieldContext<object> context, string sourceName,
		Func<TDeleteInput, TSource> mapDelete,
		Func<TSource, TDeleteOutput> transform,
		Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion) where TSource : class
		{
			AssertWithClaimsPrincipal(AssertAction.Delete, context, claimsPrincipalAssertion);

			var arg = context.GetArgument<TDeleteInput>(sourceName);
			var item = mapDelete(arg);

			await InternalDeleteAsync(context, item);

			return transform(item);
		}

		public async Task<TDeleteOutput> DeleteAllAsync<TSource, TDeleteOutput>(ResolveFieldContext<object> context, string sourceName,
			Func<TDeleteOutput> getOutput,
			Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion) where TSource : class
		{
			AssertWithClaimsPrincipal(AssertAction.DeleteAll, context, claimsPrincipalAssertion);
			try
			{
				var ctx = context.UserContext as IGraphRequestContext;

				var mutationActionItem = new MutationActionItem<TSource>
				{
					Action = MutationActions.DeleteAll,
					RequestContext = ctx,
					RepositoryProvider = _graphQlRepositoryProvider
				};
				await _mutationActionsProvider.HandlePreActions(mutationActionItem);

				await _graphQlRepositoryProvider.GetRepository<TSource>().DeleteAllAsync<TSource>(ctx);

				await _mutationActionsProvider.HandlePostActions(mutationActionItem);

			}
			catch (Exception e)
			{
				_logger.LogError(e, "An error has occured while performing a delete-all operation.");
				throw;
			}

			return getOutput();
		}

		private async Task TransformObject(object item, IGraphRequestContext context, MutationActions mutationAction)
		{
			await TransformObjects(new List<object> { item }, context, mutationAction);
		}

		private async Task TransformObjects(List<object> items, IGraphRequestContext context, MutationActions mutationAction)
		{
			await _modelTransformerProvider.TransformAsync(new ModelTransformArguments
			{
				Models = items,
				Action = mutationAction,
				RequestContext = context
			});
		}

		public async Task<TSource> AddAsync<TSource>(ResolveFieldContext<object> context, string sourceName,
			Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion) where TSource : class
		{
			AssertWithClaimsPrincipal(AssertAction.Create, context, claimsPrincipalAssertion);

			var item = context.GetArgument<TSource>(sourceName);

			var ctx = context.UserContext as IGraphRequestContext;

			await TransformObject(item, ctx, MutationActions.Create);

			var cloned = item.Clone();

			try
			{
				var edges = _connectionEdgeResolver.HandleConnectionEdges(item, async (model) =>
				{
					var mutationActionItem = new MutationActionItem<TSource>
					{
						Action = MutationActions.Create,
						RequestContext = ctx,
						ObjectItem = model,
						RepositoryProvider = _graphQlRepositoryProvider
					};

					await _mutationActionsProvider.HandlePreActions(mutationActionItem);

					await _graphQlRepositoryProvider.GetRepository(model.GetType())
						.AddAsync(model.GetType(), model, ctx);

					await _mutationActionsProvider.HandlePostActions(mutationActionItem);
				});

				if (edges.Count > 0)
					await _graphQlRepositoryProvider.GetRepository(typeof(ConnectionEdge)).BatchAddAsync(edges, ctx);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "An error has occured while performing an add operation.");
				throw;
			}
			return cloned;
		}

		public async Task<TSource> AddOrUpdateAsync<TSource>(ResolveFieldContext<object> context, string sourceName, Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion) where TSource : class
		{
			AssertWithClaimsPrincipal(AssertAction.CreateOrUpdate, context, claimsPrincipalAssertion);
			var item = context.GetArgument<TSource>(sourceName);
			var ctx = context.UserContext as IGraphRequestContext;

			await TransformObject(item, ctx, MutationActions.CreateOrUpdate);

			var cloned = item.Clone();

			try
			{
				var edges = _connectionEdgeResolver.HandleConnectionEdges(item, async (model) =>
				{
					var mutationActionItem = new MutationActionItem<TSource>
					{
						Action = MutationActions.CreateOrUpdate,
						RequestContext = ctx,
						ObjectItem = model,
						RepositoryProvider = _graphQlRepositoryProvider
					};

					await _mutationActionsProvider.HandlePreActions(mutationActionItem);

					await _graphQlRepositoryProvider.GetRepository(model.GetType())
						.AddOrUpdateAsync(model.GetType(), model, ctx);

					await _mutationActionsProvider.HandlePostActions(mutationActionItem);
				});

				if (edges.Count > 0)
					await _graphQlRepositoryProvider.GetRepository(typeof(ConnectionEdge))
						.BatchAddOrUpdateAsync(edges, ctx);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "An error has occured while performing an add operation.");
				throw;
			}
			return cloned;
		}

		public async Task<TSource> UpdateAsync<TSource>(ResolveFieldContext<object> context, string sourceName,
			Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion) where TSource : class
		{
			AssertWithClaimsPrincipal(AssertAction.Update, context, claimsPrincipalAssertion);
			var item = context.GetArgument<TSource>(sourceName);
			var ctx = context.UserContext as IGraphRequestContext;

			await TransformObject(item, ctx, MutationActions.Update);

			var cloned = item.Clone();

			try
			{
				var mutationActionItem = new MutationActionItem<TSource>
				{
					Action = MutationActions.Update,
					RequestContext = ctx,
					Item = item,
					RepositoryProvider = _graphQlRepositoryProvider
				};

				await _mutationActionsProvider.HandlePreActions(mutationActionItem);

				await _graphQlRepositoryProvider.GetRepository<TSource>().UpdateAsync(item, context.UserContext as IGraphRequestContext);

				await _mutationActionsProvider.HandlePostActions(mutationActionItem);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "An error has occured while performing an update operation.");
				throw;
			}
			return cloned;
		}

		public async Task<List<TSource>> BatchAddOrUpdateAsync<TSource>(ResolveFieldContext<object> context, string sourceName,
			Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion) where TSource : class
		{
			return await InternalBatchAsync<TSource>(context, sourceName, AssertAction.BatchCreateOrUpdate, claimsPrincipalAssertion);
		}

		private async Task<List<TSource>> InternalBatchAsync<TSource>(
			ResolveFieldContext<object> context, string sourceName,
			AssertAction assertAction,
			Func<ClaimsPrincipal, AssertAction, bool> claimsPrincipalAssertion) where TSource : class
		{
			AssertWithClaimsPrincipal(assertAction, context, claimsPrincipalAssertion);

			var items = context.GetArgument<IEnumerable<TSource>>(sourceName).ToList();
			var ctx = context.UserContext as IGraphRequestContext;

			if (assertAction == AssertAction.BatchCreate)
			{
				await TransformObjects(items.Select(x => (object)x).ToList(), ctx, MutationActions.BatchCreate);
			}

			if (assertAction == AssertAction.BatchCreateOrUpdate)
			{
				await TransformObjects(items.Select(x => (object)x).ToList(), ctx, MutationActions.BatchCreateOrUpdate);
			}

			var cloned = items.Clone();

			var batchItems = new Dictionary<Type, BatchModelList>();

			try
			{
				var edges = _connectionEdgeResolver.HandleConnectionEdges(items, (model) =>
				{
					var key = model.GetType();
					BatchModelList itemsList;

					if (batchItems.ContainsKey(key))
					{
						itemsList = batchItems[key];
					}
					else
					{
						itemsList = new BatchModelList(key);
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
							var mutationActionItem = new MutationActionItem<TSource>
							{
								Action = MutationActions.BatchCreate,
								RequestContext = ctx,
								ObjectItems = batchItem.Value.Items,
								RepositoryProvider = _graphQlRepositoryProvider
							};

							await _mutationActionsProvider.HandlePreActions(mutationActionItem);

							await _graphQlRepositoryProvider.GetRepository(batchItem.Key).BatchAddAsync(batchItem.Key, batchItem.Value.Items, ctx);

							await _mutationActionsProvider.HandlePostActions(mutationActionItem);
						}

						break;

					case AssertAction.BatchCreateOrUpdate:
						if (edges.Count > 0)
							await _graphQlRepositoryProvider.GetRepository(typeof(ConnectionEdge)).BatchAddOrUpdateAsync(edges, ctx);

						foreach (var batchItem in batchItems)
						{
							var mutationActionItem = new MutationActionItem<TSource>
							{
								Action = MutationActions.BatchCreateOrUpdate,
								RequestContext = ctx,
								ObjectItems = batchItem.Value.Items,
								RepositoryProvider = _graphQlRepositoryProvider
							};
							await _mutationActionsProvider.HandlePreActions(mutationActionItem);

							await _graphQlRepositoryProvider.GetRepository(batchItem.Key).BatchAddOrUpdateAsync(batchItem.Key, batchItem.Value.Items, ctx);

							await _mutationActionsProvider.HandlePostActions(mutationActionItem);
						}
						break;

					default:
						throw new InvalidOperationException("This internal method is only for batch operations.");
				}

				return cloned;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "An error has occured while performing a batch add operation.");
				throw;
			}
		}
	}
}
