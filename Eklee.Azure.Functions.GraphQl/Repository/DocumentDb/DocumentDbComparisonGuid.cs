using System;

namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public class DocumentDbComparisonGuid : BaseDocumentDbComparison<Guid>
	{
		protected override string GetComprisonString(Comparisons comparison, string[] names)
		{
			if (comparison == Comparisons.Equal)
			{
				if (names.Length == 1)
				{
					return $" {GetPropertyName()} = {names[0]}";
				}

				if (names.Length > 1)
				{
					return $" {GetPropertyName()} in ({string.Join(",", names)})";
				}
			}
			return null;
		}
	}
}
