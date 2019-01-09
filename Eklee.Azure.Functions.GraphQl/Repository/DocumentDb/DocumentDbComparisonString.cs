namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public class DocumentDbComparisonString : IDocumentDbComparison
	{
		private QueryParameter _queryParameter;

		private string _value;
		public bool CanHandle(QueryParameter queryParameter)
		{
			_queryParameter = queryParameter;
			_value = null;

			if (queryParameter.ContextValue.Value is string value && !string.IsNullOrEmpty(value))
			{
				_value = value;
				return true;
			}

			return false;
		}

		public string Generate()
		{
			if (_queryParameter.ContextValue.Comparison == Comparisons.Equal)
				return $"x.{_queryParameter.MemberModel.Member.Name} = '{_value}'";

			string comparison;

			switch (_queryParameter.ContextValue.Comparison)
			{
				case Comparisons.StringContains:
					comparison = "CONTAINS";
					break;

				case Comparisons.StringStartsWith:
					comparison = "STARTSWITH";
					break;

				case Comparisons.StringEndsWith:
					comparison = "ENDSWITH";
					break;

				default:
					return null;
			}

			return $" {comparison}(x.{_queryParameter.MemberModel.Member.Name}, '{_value}')";
		}
	}
}
