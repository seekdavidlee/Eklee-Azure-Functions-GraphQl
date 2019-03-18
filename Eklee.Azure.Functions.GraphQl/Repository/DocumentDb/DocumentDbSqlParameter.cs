using Microsoft.Azure.Documents;

namespace Eklee.Azure.Functions.GraphQl.Repository.DocumentDb
{
	public class DocumentDbSqlParameter
	{
		public SqlParameter[] SqlParameters { get; set; }
		public string Comparison { get; set; }
	}
}