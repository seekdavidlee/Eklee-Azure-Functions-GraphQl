using System;

namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public interface ISearchMappedModels
	{
		void Map<TSearchModel, TModel>();
		bool TryGetMappedSearchType<TModel>(out Type mappedSearchType);
		object CreateInstanceFromMap<TModel>(TModel map);
	}
}
