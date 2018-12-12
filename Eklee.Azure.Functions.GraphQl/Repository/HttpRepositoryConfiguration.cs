using System;
using System.Net.Http;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class HttpResource
	{
		public string AppendUrl { get; set; }
		public HttpMethod Method { get; set; }
		public bool? ContainsBody { get; set; }
	}

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

		public Func<object, HttpResource> AddTransform { get; set; }
		public Func<object, HttpResource> UpdateTransform { get; set; }
		public Func<object, HttpResource> DeleteTransform { get; set; }

		public HttpRepositoryConfiguration<TSource> AddResource(Func<TSource, HttpResource> transform)
		{
			AddTransform = item => transform((TSource)item);
			return this;
		}

		public HttpRepositoryConfiguration<TSource> UpdateResource(Func<TSource, HttpResource> transform)
		{
			UpdateTransform = item => transform((TSource)item);
			return this;
		}

		public HttpRepositoryConfiguration<TSource> DeleteResource(Func<TSource, HttpResource> transform)
		{
			DeleteTransform = item => transform((TSource)item);
			return this;
		}

		public ModelConventionInputBuilder<TSource> Build()
		{
			return _modelConventionInputBuilder;
		}
	}
}
