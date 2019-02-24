using System;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public class DocumentDbComparisonGuid : IDocumentDbComparison
	{
		private QueryParameter _queryParameter;

		private Guid? _value;
		private Guid[] _values;

		public bool CanHandle(QueryParameter queryParameter)
		{
			_queryParameter = queryParameter;
			_value = null;

			if (_queryParameter.ContextValue.GetFirstValue() is Guid value)
			{
				if (_queryParameter.ContextValue.IsSingleValue())
					_value = value;
				else
					_values = _queryParameter.ContextValue.Values.Select(x => (Guid)x).ToArray();

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
				return _queryParameter.ContextValue.IsSingleValue() ?
					$" x.{_queryParameter.MemberModel.Member.Name} = '{_value.ToString()}'" :
					$" x.{_queryParameter.MemberModel.Member.Name} in ({GetJoinValues()})";

			return null;
		}
	}
}
