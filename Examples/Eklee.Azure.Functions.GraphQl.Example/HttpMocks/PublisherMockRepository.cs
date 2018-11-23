using System.Collections.Generic;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Example.HttpMocks
{
	public class PublisherMockRepository : IHttpMockRepository<Publisher>
	{
		private readonly List<Publisher> _publishers = new List<Publisher>();
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
	}
}