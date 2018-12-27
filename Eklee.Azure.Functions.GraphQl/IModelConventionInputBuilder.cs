using System;
using Eklee.Azure.Functions.GraphQl.Repository;

namespace Eklee.Azure.Functions.GraphQl
{
	public interface IModelConventionInputBuilder<TSource>
	{
		InMemoryConfiguration<TSource> ConfigureInMemory<TType>();
		HttpConfiguration<TSource> ConfigureHttp<TType>();
		DocumentDbConfiguration<TSource> ConfigureDocumentDb<TType>();

		ModelConventionInputBuilder<TSource> Delete<TDeleteInput, TDeleteOutput>(
			Func<TDeleteInput, TSource> mapDelete,
			Func<TSource, TDeleteOutput> transform);

		ModelConventionInputBuilder<TSource> DeleteAll<TDeleteOutput>(Func<TDeleteOutput> getOutput);

		void Build();
	}
}