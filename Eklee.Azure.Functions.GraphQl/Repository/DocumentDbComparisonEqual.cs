using System;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class DocumentDbComparisonEqual
	{
		private readonly QueryParameter _queryParameter;

		public DocumentDbComparisonEqual(QueryParameter queryParameter)
		{
			_queryParameter = queryParameter;
		}

		public override string ToString()
		{
			var fieldName = _queryParameter.MemberModel.Member.Name;
			var fieldValue = _queryParameter.ContextValue.Value;

			if (fieldValue is string strFieldValue)
			{
				return $" x.{fieldName} = '{strFieldValue}'";
			}

			if (fieldValue is int intFieldValue)
			{
				return $" x.{fieldName} = {intFieldValue}";
			}

			throw new NotImplementedException(DocumentClientProvider.SyntaxNotYetSupported);
		}
	}
}
