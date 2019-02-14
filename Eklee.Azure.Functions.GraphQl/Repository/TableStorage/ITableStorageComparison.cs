namespace Eklee.Azure.Functions.GraphQl.Repository.TableStorage
{
	public interface  ITableStorageComparison
	{
		bool CanHandle(QueryParameter queryParameter);
		string Generate();
	}
}
