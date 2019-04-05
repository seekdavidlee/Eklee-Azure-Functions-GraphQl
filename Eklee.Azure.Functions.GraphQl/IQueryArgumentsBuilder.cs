using GraphQL.Types;

namespace Eklee.Azure.Functions.GraphQl
{
	public interface IQueryArgumentsBuilder
	{
		QueryArguments BuildNonNull<T>(string sourceName);
		QueryArguments BuildList<T>(string sourceName);
	}
}
