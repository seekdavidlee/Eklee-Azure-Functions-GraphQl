using System.Collections.Generic;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Example.HttpMocks
{
	public class PublisherMockRepository : IHttpMockRepository<Publisher>
	{
		private readonly Dictionary<string, Publisher> _publishers = new Dictionary<string, Publisher>();

		public void Add(Publisher item)
		{
			_publishers.Add(item.Id, item);
		}

		public Publisher Update(Publisher item)
		{
			var p = _publishers[item.Id] = item;

			return p;
		}

		public void Delete(string id)
		{
			_publishers.Remove(id);
		}

		public IQueryable<Publisher> Search()
		{
			return _publishers.Values.AsQueryable();
		}

		public void ClearAll()
		{
			_publishers.Clear();
		}
	}
}