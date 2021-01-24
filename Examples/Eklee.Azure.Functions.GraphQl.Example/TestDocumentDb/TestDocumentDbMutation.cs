using System;
using Eklee.Azure.Functions.GraphQl.Connections;
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
				.DeleteAll(() => new Status { Message = "All Model4 have been removed." })
				.Build();

			inputBuilderFactory.Create<Model5>(this)
				.ConfigureDocumentDb<Model5>()
				.AddKey(key)
				.AddUrl(url)
				.AddRequestUnit(requestUnits)
				.AddDatabase(db)
				.AddPartition(x => x.Field)
				.BuildDocumentDb()
				.DeleteAll(() => new Status { Message = "All Model5 have been removed." })
				.DisableBatchCreate()   // Note it would be hard to use batch create in this context. Use batchCreateOrUpdate instead.
				.Build();

			inputBuilderFactory.Create<Model5Friend>(this)
				.ConfigureDocumentDb<Model5Friend>()
				.AddKey(key)
				.AddUrl(url)
				.AddRequestUnit(requestUnits)
				.AddDatabase(db)
				.AddPartition(x => x.Field)
				.BuildDocumentDb()
				.DeleteAll(() => new Status { Message = "All Model5Friend have been removed." })
				.Build();

			inputBuilderFactory.Create<Model6>(this)
				.ConfigureDocumentDb<Model6>()
				.AddKey(key)
				.AddUrl(url)
				.AddRequestUnit(requestUnits)
				.AddDatabase(db)
				.AddPartition(x => x.Field)
				.BuildDocumentDb()
				.DeleteAll(() => new Status { Message = "All Model6 have been removed." })
				.DisableBatchCreate()   // Note it would be hard to use batch create in this context. Use batchCreateOrUpdate instead.
				.Build();

			inputBuilderFactory.Create<Model6Friend>(this)
				.ConfigureDocumentDb<Model6Friend>()
				.AddKey(key)
				.AddUrl(url)
				.AddRequestUnit(requestUnits)
				.AddDatabase(db)
				.AddPartition(x => x.Field)
				.BuildDocumentDb()
				.DeleteAll(() => new Status { Message = "All Model6Friend have been removed." })
				.Build();

			inputBuilderFactory.Create<ConnectionEdge>(this)
				.ConfigureDocumentDb<ConnectionEdge>()
				.AddKey(key)
				.AddUrl(url)
				.AddRequestUnit(requestUnits)
				.AddDatabase(db)
				.AddPartition(x => x.SourceId)
				.BuildDocumentDb()
				.DeleteAll(() => new Status { Message = "All ConnectionEdges have been removed." })
				.Build();

			inputBuilderFactory.Create<Model7>(this)
				.ConfigureDocumentDb<Model7>()
				.AddKey(key)
				.AddUrl(url)
				.AddRequestUnit(requestUnits)
				.AddDatabase(db)
				.AddPartition(x => x.Field)
				.BuildDocumentDb()
				.DeleteAll(() => new Status { Message = "All Model7 have been removed." })
				.DisableBatchCreate()   // Note it would be hard to use batch create in this context. Use batchCreateOrUpdate instead.
				.Build();

			inputBuilderFactory.Create<Model8>(this)
				.ConfigureDocumentDb<Model8>()
				.AddKey(key)
				.AddUrl(url)
				.AddRequestUnit(requestUnits)
				.AddDatabase(db)
				.AddPartition(x => x.Field)
				.BuildDocumentDb()
				.DeleteAll(() => new Status { Message = "All Model8 have been removed." })
				.DisableBatchCreate()   // Note it would be hard to use batch create in this context. Use batchCreateOrUpdate instead.
				.Build();

			inputBuilderFactory.Create<Model15>(this)
				.ConfigureDocumentDb<Model15>()
				.AddKey(key)
				.AddUrl(url)
				.AddRequestUnit(requestUnits)
				.AddDatabase(db)
				.AddPartition(x => x.Field)
				.BuildDocumentDb()
				.DeleteAll(() => new Status { Message = "All Model8 have been removed." })
				.DisableBatchCreate()   // Note it would be hard to use batch create in this context. Use batchCreateOrUpdate instead.
				.Build();
		}
	}
}
