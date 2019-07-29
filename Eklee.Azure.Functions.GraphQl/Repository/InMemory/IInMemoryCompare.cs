namespace Eklee.Azure.Functions.GraphQl.Repository.InMemory
{
	public interface IInMemoryCompare
	{
		bool CanHandle(object obj, QueryParameter queryParameter);
		bool MeetsCondition(object obj, QueryParameter queryParameter);
	}
}
