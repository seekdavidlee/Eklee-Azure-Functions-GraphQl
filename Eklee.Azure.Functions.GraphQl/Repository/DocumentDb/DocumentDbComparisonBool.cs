namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public class DocumentDbComparisonBool : BaseDocumentDbComparison<bool>
	{
		protected override string GetComprisonString(Comparisons comparison, string[] names)
		{
			if (names.Length == 1 && comparison == Comparisons.Equal)
			{
				return $" {GetPropertyName()} = {names[0]}";
			}

			return null;
		}
	}
}