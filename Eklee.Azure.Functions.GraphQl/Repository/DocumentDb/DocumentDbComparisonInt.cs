namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public class DocumentDbComparisonInt : IDocumentDbComparison
	{
		private QueryParameter _queryParameter;

		private int? _value;

		public bool CanHandle(QueryParameter queryParameter)
		{
			_queryParameter = queryParameter;
			_value = null;

			if (queryParameter.ContextValue.Value is int value && value != 0)
			{
				_value = value;
				return true;
			}

			return false;
		}

		public string Generate()
		{
			if (_queryParameter.ContextValue.Comparison == Comparisons.Equal)
				return $" x.{_queryParameter.MemberModel.Member.Name} = {_value}";

			if (_queryParameter.ContextValue.Comparison == Comparisons.NotEqual)
				return $" x.{_queryParameter.MemberModel.Member.Name} != {_value}";

			if (_queryParameter.ContextValue.Comparison == Comparisons.GreaterThan)
				return $" x.{_queryParameter.MemberModel.Member.Name} > {_value}";

			if (_queryParameter.ContextValue.Comparison == Comparisons.GreaterEqualThan)
				return $" x.{_queryParameter.MemberModel.Member.Name} >= {_value}";

			if (_queryParameter.ContextValue.Comparison == Comparisons.LessThan)
				return $" x.{_queryParameter.MemberModel.Member.Name} < {_value}";

			if (_queryParameter.ContextValue.Comparison == Comparisons.LessEqualThan)
				return $" x.{_queryParameter.MemberModel.Member.Name} <= {_value}";

			return null;
		}
	}
}
