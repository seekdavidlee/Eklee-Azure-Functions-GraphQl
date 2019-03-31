using System;
using Eklee.Azure.Functions.GraphQl.Example.Models;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;

namespace Eklee.Azure.Functions.GraphQl.Example.TestDocumentDb
{
	public class TestDocumentDbMutation : ObjectGraphType
	{
		public TestDocumentDbMutation(InputBuilderFactory inputBuilderFactory, IConfiguration configuration)
		{
			Name = "mutation";

			var key = configuration["DocumentDb:Key"];
			var url = configuration["DocumentDb:Url"];
			var requestUnits = Convert.ToInt32(configuration["DocumentDb:RequestUnits"]);
			const string db = "docDb";

			inputBuilderFactory.Create<Model1>(this)
				.ConfigureDocumentDb<Model1>()
				.AddKey(key)
				.AddUrl(url)
				.AddRequestUnit(requestUnits)
				.AddDatabase(db)
				.AddPartition(x => x.Field)
				.BuildDocumentDb()
				.DeleteAll(() => new Status { Message = "All Model1 have been removed." })
				.Build();

			inputBuilderFactory.Create<Model2>(this)
				.ConfigureDocumentDb<Model2>()
				.AddKey(key)
				.AddUrl(url)
				.AddRequestUnit(requestUnits)
				.AddDatabase(db)
				.AddPartition(x => x.Field)
				.BuildDocumentDb()
				.DeleteAll(() => new Status { Message = "All Model2 have been removed." })
				.Build();

			inputBuilderFactory.Create<Model3>(this)
				.ConfigureDocumentDb<Model3>()
				.AddKey(key)
				.AddUrl(url)
				.AddRequestUnit(requestUnits)
				.AddDatabase(db)
				.AddPartition(x => x.Field)
				.BuildDocumentDb()
				.DeleteAll(() => new Status { Message = "All Model3 have been removed." })
				.Build();

			inputBuilderFactory.Create<Model4>(this)
				.ConfigureDocumentDb<Model4>()
				.AddKey(key)
				.AddUrl(url)
				.AddRequestUnit(requestUnits)
				.AddDatabase(db)
				.AddPartition(x => x.Field)
				.BuildDocumentDb()
				.DeleteAll(() => new Status { Message = "All Model3 have been removed." })
				.Build();
		}
	}
}
