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
		private readonly IGraphQlRepository _graphQlRepository;
		private readonly Type _typeSource;
		private readonly Dictionary<string, string> _configurations = new Dictionary<string, string>();

		public HttpConfiguration(
			ModelConventionInputBuilder<TSource> modelConventionInputBuilder,
			IGraphQlRepository graphQlRepository,
			Type typeSource)
		{
			_modelConventionInputBuilder = modelConventionInputBuilder;
			_graphQlRepository = graphQlRepository;
			_typeSource = typeSource;
		}

		public HttpConfiguration<TSource> AddBaseUrl(string value)
		{
			AddConfiguration(HttpConstants.BaseUrl, value);
			return this;
		}

		private Func<object, HttpResource> AddTransform { get; set; }
		private Func<object, HttpResource> UpdateTransform { get; set; }
		private Func<object, HttpResource> DeleteTransform { get; set; }
		private Func<Dictionary<string, string>, HttpQueryResource> QueryTransform { get; set; }

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

		public HttpConfiguration<TSource> QueryResource(Func<Dictionary<string, string>, HttpQueryResource> transform)
		{
			QueryTransform = transform;
			return this;
		}

		private void AddConfiguration(string key, string value)
		{
			_configurations.Add(key, value);
		}

		public ModelConventionInputBuilder<TSource> Build()
		{
			_graphQlRepository.Configure(_typeSource, _configurations);

			if (_graphQlRepository is HttpRepository repo)
			{
				repo.SetAddTransform(_typeSource, AddTransform);
				repo.SetUpdateTransform(_typeSource, UpdateTransform);
				repo.SetDeleteTransform(_typeSource, DeleteTransform);
				repo.SetQueryTransform(_typeSource, QueryTransform);
			}
			return _modelConventionInputBuilder;
		}
	}
}
