using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FastMember;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public class SearchMappedModels : ISearchMappedModels, IMutationPostAction
	{
		private readonly Dictionary<string, SearchMappedModel> _mappedTypes = new Dictionary<string, SearchMappedModel>();

		public void Map<TSearchModel, TModel>()
		{
			var typeName = typeof(TModel).Name;
			if (!_mappedTypes.ContainsKey(typeName))
			{
				_mappedTypes.Add(typeName, new SearchMappedModel
				{
					ModelType = typeof(TModel),
					SearchModelType = typeof(TSearchModel)
				});
			}
		}

		public bool TryGetMappedSearchType<TModel>(out Type mappedSearchType)
		{
			return InternalTryGetMappedSearchType(typeof(TModel), out mappedSearchType);
		}

		private bool InternalTryGetMappedSearchType(Type modelType, out Type mappedSearchType)
		{
			if (_mappedTypes.ContainsKey(modelType.Name))
			{
				mappedSearchType = _mappedTypes[modelType.Name].SearchModelType;
				return true;
			}

			mappedSearchType = null;
			return false;
		}

		public object CreateInstanceFromMap(object map, string typeName)
		{
			var searchMappedModel = _mappedTypes[typeName];
			var searchModelAccessor = TypeAccessor.Create(searchMappedModel.SearchModelType);
			var modelAccessor = TypeAccessor.Create(searchMappedModel.ModelType);

			var o = searchModelAccessor.CreateNew();

			var availableMemberNames = searchModelAccessor.GetMembers().Select(x => x.Name).ToList();

			modelAccessor.GetMembers().ToList().ForEach(m =>
			{
				if (availableMemberNames.Contains(m.Name))
				{
					var searchMember = searchModelAccessor.GetMembers().Single(x => x.Name == m.Name);
					var modelMember = modelAccessor.GetMembers().Single(x => x.Name == m.Name);

					if (searchMember.Type == modelMember.Type)
						searchModelAccessor[o, m.Name] = modelAccessor[map, m.Name];
					else
					{
						var mappedValue = modelAccessor[map, m.Name];
						if (searchMember.Type == typeof(string) && mappedValue != null)
						{
							searchModelAccessor[o, m.Name] = mappedValue.ToString();
						}
					}
				}
			});

			return o;
		}

		public async Task TryHandlePostItem<TSource>(MutationActionItem<TSource> mutationActionItem)
		{
			Type mappedSearchType;
			bool shouldHandle = false;
			if (mutationActionItem.ObjectItem != null)
			{
				shouldHandle = InternalTryGetMappedSearchType(mutationActionItem.ObjectItem.GetType(),
					out mappedSearchType);
			}
			else
			{
				if (mutationActionItem.ObjectItems != null &&
					mutationActionItem.ObjectItems.Count > 0)
				{
					shouldHandle = InternalTryGetMappedSearchType(mutationActionItem.ObjectItems.First().GetType(),
						out mappedSearchType);
				}
				else
				{
					shouldHandle = TryGetMappedSearchType<TSource>(out mappedSearchType);
				}

			}

			if (!shouldHandle) return;

			if (mutationActionItem.Action == MutationActions.DeleteAll)
			{
				await mutationActionItem.RepositoryProvider.GetRepository(mappedSearchType)
					.DeleteAllAsync(mappedSearchType, mutationActionItem.RequestContext);
				return;
			}

			if (mutationActionItem.Action == MutationActions.BatchCreateOrUpdate ||
				mutationActionItem.Action == MutationActions.BatchCreate)
			{
				List<object> mappedInstances;
				if (mutationActionItem.Items != null)
				{
					var type = mutationActionItem.Items.First().GetType();
					mappedInstances = mutationActionItem.Items.Select(item => Convert.ChangeType(CreateInstanceFromMap(item,
						type.Name), mappedSearchType)).ToList();
				}
				else
				{
					var type = mutationActionItem.ObjectItems.First().GetType();
					mappedInstances = mutationActionItem.ObjectItems.Select(item => Convert.ChangeType(CreateInstanceFromMap(item,
						type.Name), mappedSearchType)).ToList();
				}

				await mutationActionItem.RepositoryProvider.GetRepository(mappedSearchType)
						.BatchAddAsync(mappedSearchType, mappedInstances, mutationActionItem.RequestContext);

				return;
			}

			object oMap = mutationActionItem.ObjectItem != null ?
				mutationActionItem.ObjectItem :
				mutationActionItem.Item;

			if (oMap == null) return;

			var mappedInstance = CreateInstanceFromMap(oMap, oMap.GetType().Name);

			switch (mutationActionItem.Action)
			{
				case MutationActions.Create:
				case MutationActions.CreateOrUpdate:    // Search repository does not support createOrUpdate, so we are just using Create.

					await mutationActionItem.RepositoryProvider.GetRepository(mappedSearchType)
						.AddAsync(mappedSearchType, mappedInstance, mutationActionItem.RequestContext);
					break;

				case MutationActions.Update:

					await mutationActionItem.RepositoryProvider.GetRepository(mappedSearchType)
						.UpdateAsync(mappedSearchType, mappedInstance, mutationActionItem.RequestContext);
					break;

				case MutationActions.Delete:

					await mutationActionItem.RepositoryProvider.GetRepository(mappedSearchType)
						.DeleteAsync(mappedSearchType, mappedInstance, mutationActionItem.RequestContext);
					break;

				default:
					// Do nothing.
					break;
			}
		}
	}
}
