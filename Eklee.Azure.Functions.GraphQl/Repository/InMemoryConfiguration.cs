namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class InMemoryConfiguration<TSource>
	{
		private readonly ModelConventionInputBuilder<TSource> _modelConventionInputBuilder;

		public InMemoryConfiguration(ModelConventionInputBuilder<TSource> modelConventionInputBuilder)
		{
			_modelConventionInputBuilder = modelConventionInputBuilder;
		}

		public ModelConventionInputBuilder<TSource> BuildInMemory()
		{
			return _modelConventionInputBuilder;
		}
	}
}
