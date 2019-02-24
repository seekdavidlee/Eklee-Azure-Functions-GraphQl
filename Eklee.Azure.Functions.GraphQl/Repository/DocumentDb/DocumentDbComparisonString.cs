using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public class DocumentDbComparisonString : IDocumentDbComparison
	{
		private QueryParameter _queryParameter;

		private string _value;
		private string[] _values;
		public bool CanHandle(QueryParameter queryParameter)
		{
			_queryParameter = queryParameter;
			_value = null;

			if (_queryParameter.ContextValue.GetFirstValue() is string value && !string.IsNullOrEmpty(value))
			{
				if (_queryParameter.ContextValue.IsSingleValue())
					_value = value;
				else
					_values = _queryParameter.ContextValue.Values.Select(x => (string)x).ToArray();
				return true;
			}

			return false;
		}

		private string GetJoinValues()
		{
			return string.Join(",", _values.Select(x => $"'{x}'"));
		}

		public string Generate()
		{
			if (_queryParameter.ContextValue.Comparison == Comparisons.Equal)
			{
				return !string.IsNullOrEmpty(_value) ?
					$"x.{_queryParameter.MemberModel.Member.Name} = '{_value}'" :
					$"x.{_queryParameter.MemberModel.Member.Name} in ({GetJoinValues()})";
			}

			if (string.IsNullOrEmpty(_value)) return null;

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
