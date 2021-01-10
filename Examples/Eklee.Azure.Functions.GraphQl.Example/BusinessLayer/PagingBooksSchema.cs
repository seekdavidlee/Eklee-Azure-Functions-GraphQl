using GraphQL.Types;
using System;

namespace Eklee.Azure.Functions.GraphQl.Example.BusinessLayer
{
	public class PagingBooksSchema : Schema
	{
		public PagingBooksSchema(IServiceProvider resolver, PagingBooksQuery booksQuery, PagingBooksMutation booksMutation) : base(resolver)
		{
			Query = booksQuery;
			Mutation = booksMutation;
		}
	}
}
