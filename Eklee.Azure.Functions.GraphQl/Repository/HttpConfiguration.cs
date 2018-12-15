using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class HttpResource
	{
		public string AppendUrl { get; set; }
		public HttpMethod Method { get; set; }
		public bool? ContainsBody { get; set; }
	}

	public enum HttpQueryTypes
	{
		AppendToUrl
	}

	public class HttpQueryResource
	{
		public string ForQueryName { get; set; }
		public HttpQueryTypes QueryType { get; set; }
		public string AppendUrl { get; set; }
		public bool IsListResult { get; set; }
	}

	public class HttpConfiguration<TSource>
	{
		private readonly ModelConventionInputBuilder<TSource> _modelConventionInputBuilder;

		public HttpConfiguration(ModelConventionInputBuilder<TSource> modelConventionInputBuilder)
		{
			_modelConventionInputBuilder = modelConventionInputBuilder;
		}

		public HttpConfiguration<TSource> AddBaseUrl(string value)
		{
			_modelConventionInputBuilder.AddConfiguration(HttpConstants.BaseUrl, value);
			return this;
		}

		public Func<object, HttpResource> AddTransform { get; set; }
		public Func<object, HttpResource> UpdateTransform { get; set; }
		public Func<object, HttpResource> DeleteTransform { get; set; }

		public HttpConfiguration<TSource> AddResource(Func<TSource, HttpResource> transform)
		{
			AddTransform = item => transform((TSource)item);
			return this;
		}

		public HttpConfiguration<TSource> UpdateResource(Func<TSource, HttpResource> transform)
		{
			UpdateTransform = item => transform((TSource)item);
			return this;
		}

		public HttpConfiguration<TSource> DeleteResource(Func<TSource, HttpResource> transform)
		{
			DeleteTransform = item => transform((TSource)item);
			return this;
		}

		public Func<Dictionary<string, string>, HttpQueryResource> QueryTransform { get; set; }

		public HttpConfiguration<TSource> QueryResource(Func<Dictionary<string, string>, HttpQueryResource> transform)
		{
			QueryTransform = transform;
			return this;
		}

		public ModelConventionInputBuilder<TSource> Build()
		{
			return _modelConventionInputBuilder;
		}
	}
}
