using Eklee.Azure.Functions.Http;

namespace Eklee.Azure.Functions.GraphQl
{
	public class GraphRequestContext : IGraphRequestContext
	{
		public IHttpRequestContext HttpRequest { get; set; }
	}
}
