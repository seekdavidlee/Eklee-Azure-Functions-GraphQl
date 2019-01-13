using System;

namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public class DocumentDbComparisonGuid : IDocumentDbComparison
	{
		private QueryParameter _queryParameter;

		private Guid? _value;

		public bool CanHandle(QueryParameter queryParameter)
		{
			_queryParameter = queryParameter;
			_value = null;

			if (queryParameter.ContextValue.Value is Guid value)
			{
				_value = value;
				return true;
			}

			return false;
		}

		public string Generate()
		{
			if (_queryParameter.ContextValue.Comparison == Comparisons.Equal)
				return $" x.{_queryParameter.MemberModel.Member.Name} = '{_value.ToString()}'";

			return null;
		}
	}
}
