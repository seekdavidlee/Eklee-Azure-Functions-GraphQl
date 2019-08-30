namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public interface ISearchMappedModels
	{
		void Map<TSearchModel, TModel>();
	}
}
