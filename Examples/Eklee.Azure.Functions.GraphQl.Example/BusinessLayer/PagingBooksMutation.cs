using Eklee.Azure.Functions.GraphQl.Example.Models;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
	public class PagingBooksMutation : ObjectGraphType
	{
		public PagingBooksMutation(InputBuilderFactory inputBuilderFactory, IConfiguration configuration)
		{
			Name = "mutations";

			inputBuilderFactory.Create<Book>(this).ConfigureInMemory<Book>().BuildInMemory().Build();
		}
	}
}
