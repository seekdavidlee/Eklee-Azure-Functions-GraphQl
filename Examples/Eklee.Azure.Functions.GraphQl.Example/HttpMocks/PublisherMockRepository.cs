using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Example.HttpMocks
{
	public class PublisherMockRepository : IHttpMockRepository<Publisher>
	{
		private readonly List<Publisher> _publishers = new List<Publisher>();

		public PublisherMockRepository(ILogger logger)
		{
			logger.LogDebug("Instantiated PublisherMockRepository.");

			_publishers.Add(new Publisher { Id = "1", Name = "West House Publishing" });
			_publishers.Add(new Publisher { Id = "2", Name = "Northwest Inc" });
			_publishers.Add(new Publisher { Id = "3", Name = "Texas Publishers" });
			_publishers.Add(new Publisher { Id = "4", Name = "ACME" });
			_publishers.Add(new Publisher { Id = "5", Name = "Jane Publishing" });
			_publishers.Add(new Publisher { Id = "6", Name = "App Pub" });
		}

		public void Add(Publisher item)
		{
			_publishers.Add(item);
		}

		public Publisher Update(Publisher item)
		{
			var p = _publishers.Single(x => x.Id == item.Id);
			p.Name = item.Name;

			return p;
		}

		public void Delete(string id)
		{
			_publishers.Remove(_publishers.Single(x => x.Id == id));
		}

		public IQueryable<Publisher> Search()
		{
			return _publishers.AsQueryable();
		}

		public void ClearAll()
		{
			_publishers.Clear();
		}
	}
}