namespace Eklee.Azure.Functions.GraphQl
{
    public class GraphQlDomainRequest
    {
        public string OperationName { get; set; }

        public string Query { get; set; }

		public object Variables { get; set; }

	}
}
