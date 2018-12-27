namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public interface IDocumentDbComparison
	{
		bool CanHandle(QueryParameter queryParameter);
		string Generate();
	}
}