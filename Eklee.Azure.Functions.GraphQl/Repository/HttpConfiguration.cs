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
		public HttpQueryTypes QueryType { get; set; }
		public string AppendUrl { get; set; }
		public bool IsListResult { get; set; }
	}

	public class HttpConfiguration<TSource>
	{
		private readonly ModelConventionInputBuilder<TSource> _modelConventionInputBuilder;
		private readonly IGraphQlRepository _graphQlRepository;
		private readonly Type _typeSource;
		private readonly Dictionary<string, object> _configurations = new Dictionary<string, object>();

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
		private Func<HttpResource> DeleteAllTransform { get; set; }
		private readonly Dictionary<string, Func<Dictionary<string, string>, HttpQueryResource>> _queryTransforms = new Dictionary<string, Func<Dictionary<string, string>, HttpQueryResource>>();

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

		public HttpConfiguration<TSource> DeleteAllResource(Func<HttpResource> transform)
		{
			DeleteAllTransform = transform;
			return this;
		}

		public HttpConfiguration<TSource> QueryResource(string queryName, Func<Dictionary<string, string>, HttpQueryResource> transform)
		{
			_queryTransforms.Add(queryName, transform);
			return this;
		}

		private void AddConfiguration(string key, string value)
		{
			_configurations.Add(key, value);
		}

		public ModelConventionInputBuilder<TSource> BuildHttp()
		{
			_graphQlRepository.Configure(_typeSource, _configurations);

			if (_graphQlRepository is HttpRepository repo)
			{
				if (AddTransform != null) repo.SetAddTransform(_typeSource, AddTransform);

				if (UpdateTransform != null) repo.SetUpdateTransform(_typeSource, UpdateTransform);

				if (DeleteTransform != null) repo.SetDeleteTransform(_typeSource, DeleteTransform);

				if (_queryTransforms.Count > 0) repo.SetQueryTransforms(_typeSource, _queryTransforms);

				if (DeleteAllTransform != null) repo.SetDeleteAllTransform(_typeSource, DeleteAllTransform);
			}
			return _modelConventionInputBuilder;
		}
	}
}
