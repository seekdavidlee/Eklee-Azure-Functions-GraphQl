namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public class DocumentDbComparisonBool : IDocumentDbComparison
	{
		private QueryParameter _queryParameter;

		private bool? _value;

		public bool CanHandle(QueryParameter queryParameter)
		{
			_queryParameter = queryParameter;
			_value = null;

			if (queryParameter.ContextValue.IsSingleValue() && 
			    queryParameter.ContextValue.GetFirstValue() is bool value)
			{
				_value = value;
				return true;
			}

			return false;
		}

		public string Generate()
		{
			if (_queryParameter.ContextValue.Comparison == Comparisons.Equal)
				return $" x.{_queryParameter.MemberModel.Member.Name} = {_value.ToString().ToLower()}";

			return null;
		}
	}
}