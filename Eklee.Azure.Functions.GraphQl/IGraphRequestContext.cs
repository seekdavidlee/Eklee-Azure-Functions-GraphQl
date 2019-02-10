using Eklee.Azure.Functions.Http;

namespace Eklee.Azure.Functions.GraphQl
{
	public interface IGraphRequestContext
	{
		IHttpRequestContext HttpRequest { get; set; }
	}
}