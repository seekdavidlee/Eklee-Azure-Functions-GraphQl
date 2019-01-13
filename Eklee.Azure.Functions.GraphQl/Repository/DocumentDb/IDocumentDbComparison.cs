namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public interface IDocumentDbComparison
	{
		bool CanHandle(QueryParameter queryParameter);
		string Generate();
	}
}