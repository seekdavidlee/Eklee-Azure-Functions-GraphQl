using System.Threading.Tasks;

namespace Eklee.Azure.Functions.GraphQl.Example.Operations
{
	public interface IOperations
	{
		Task DeleteSearchIndexes();
	}
}
