namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class DocumentDbConfiguration<TSource>
	{
		private readonly ModelConventionInputBuilder<TSource> _modelConventionInputBuilder;

		public DocumentDbConfiguration(ModelConventionInputBuilder<TSource> modelConventionInputBuilder)
		{
			_modelConventionInputBuilder = modelConventionInputBuilder;
		}

		public DocumentDbConfiguration<TSource> AddUrl(string url)
		{
			return this;
		}

		public DocumentDbConfiguration<TSource> AddKey(string key)
		{
			return this;
		}

		public ModelConventionInputBuilder<TSource> Build()
		{
			return _modelConventionInputBuilder;
		}
	}
}
