namespace Eklee.Azure.Functions.GraphQl.Repository.Search
{
	public class SearchResultModel
	{
		public object Value { get; set; }

		public double Score { get; set; }

		public T Get<T>()
		{
			return (T)Value;
		}
	}
}
