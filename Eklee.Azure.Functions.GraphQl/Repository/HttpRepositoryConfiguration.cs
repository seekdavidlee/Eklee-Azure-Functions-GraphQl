namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class HttpRepositoryConfiguration<TSource>
	{
		private readonly ModelConventionInputBuilder<TSource> _modelConventionInputBuilder;

		public HttpRepositoryConfiguration(ModelConventionInputBuilder<TSource> modelConventionInputBuilder)
		{
			_modelConventionInputBuilder = modelConventionInputBuilder;
		}

		public HttpRepositoryConfiguration<TSource> AddBaseUrl(string value)
		{
			_modelConventionInputBuilder.AddConfiguration(Constants.BaseUrl, value);
			return this;
		}

		public HttpRepositoryConfiguration<TSource> AddResource(string value, string verb)
		{
			_modelConventionInputBuilder.AddConfiguration(Constants.AddResource, value);
			_modelConventionInputBuilder.AddConfiguration(Constants.AddResourceVerb, verb);
			return this;
		}

		public HttpRepositoryConfiguration<TSource> UpdateResource(string value, string verb)
		{
			_modelConventionInputBuilder.AddConfiguration(Constants.UpdateResource, value);
			_modelConventionInputBuilder.AddConfiguration(Constants.UpdateResourceVerb, verb);
			return this;
		}

		public HttpRepositoryConfiguration<TSource> DeleteResource(string value, string verb)
		{
			_modelConventionInputBuilder.AddConfiguration(Constants.DeleteResource, value);
			_modelConventionInputBuilder.AddConfiguration(Constants.DeleteResourceVerb, verb);
			return this;
		}

		public ModelConventionInputBuilder<TSource> Build()
		{
			return _modelConventionInputBuilder;
		}
	}
}
