using System;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class DocumentDbComparisonDate : IDocumentDbComparison
	{
		private QueryParameter _queryParameter;

		private DateTime? _value;

		public bool CanHandle(QueryParameter queryParameter)
		{
			_queryParameter = queryParameter;
			_value = null;

			if (queryParameter.ContextValue.Value is DateTime value && value != DateTime.MinValue)
			{
				_value = value;
				return true;
			}

			return false;
		}

		private string GetDateTimeString()
		{
			if (!_value.HasValue) throw new InvalidOperationException("DateTime comparison is not valid for a null value.");

			return _value.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");
		}

		public string Generate()
		{
			if (_queryParameter.ContextValue.Comparison == Comparisons.Equal)
				return $" x.{_queryParameter.MemberModel.Member.Name} = '{GetDateTimeString()}'";

			if (_queryParameter.ContextValue.Comparison == Comparisons.NotEqual)
				return $" x.{_queryParameter.MemberModel.Member.Name} != '{GetDateTimeString()}'";

			if (_queryParameter.ContextValue.Comparison == Comparisons.GreaterThan)
				return $" x.{_queryParameter.MemberModel.Member.Name} > '{GetDateTimeString()}'";

			if (_queryParameter.ContextValue.Comparison == Comparisons.GreaterEqualThan)
				return $" x.{_queryParameter.MemberModel.Member.Name} >= '{GetDateTimeString()}'";

			if (_queryParameter.ContextValue.Comparison == Comparisons.LessThan)
				return $" x.{_queryParameter.MemberModel.Member.Name} < '{GetDateTimeString()}'";

			if (_queryParameter.ContextValue.Comparison == Comparisons.LessEqualThan)
				return $" x.{_queryParameter.MemberModel.Member.Name} <= '{GetDateTimeString()}'";

			return null;
		}
	}
}
