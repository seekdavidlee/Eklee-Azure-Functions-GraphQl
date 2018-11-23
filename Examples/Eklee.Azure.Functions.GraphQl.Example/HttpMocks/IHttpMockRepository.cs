using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Example.HttpMocks
{
	public interface IHttpMockRepository<T>
	{
		void Add(T item);
		T Update(T item);
		void Delete(string id);
		IQueryable<T> Search();
	}
}
