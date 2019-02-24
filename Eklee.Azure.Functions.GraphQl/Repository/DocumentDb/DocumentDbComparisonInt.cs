using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public class DocumentDbComparisonInt : IDocumentDbComparison
	{
		private QueryParameter _queryParameter;

		private int? _value;
		private int[] _values;

		public bool CanHandle(QueryParameter queryParameter)
		{
			_queryParameter = queryParameter;
			_value = null;

			if (queryParameter.ContextValue.GetFirstValue() is int value && value != 0)
			{
				if (_queryParameter.ContextValue.IsSingleValue())
					_value = value;
				else
					_values = _queryParameter.ContextValue.Values.Select(x => (int)x).ToArray();
				return true;
			}

			return false;
		}

		private string GetJoinValues()
		{
			return string.Join(",", _values);
		}

		public string Generate()
		{
			if (_queryParameter.ContextValue.Comparison == Comparisons.Equal)
			{
				if (_queryParameter.ContextValue.IsSingleValue())
					return $" x.{_queryParameter.MemberModel.Member.Name} = {_value}";

				return $" x.{_queryParameter.MemberModel.Member.Name} in ({GetJoinValues()})";
			}


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
