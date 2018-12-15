using System;
using System.Collections.Generic;
using System.Text;

namespace Eklee.Azure.Functions.GraphQl.Repository
{
	public class DocumentDbConfiguration<TSource>
	{
		private readonly ModelConventionInputBuilder<TSource> _modelConventionInputBuilder;

		public DocumentDbConfiguration(ModelConventionInputBuilder<TSource> modelConventionInputBuilder)
		{
			_modelConventionInputBuilder = modelConventionInputBuilder;
		}

		public ModelConventionInputBuilder<TSource> Build()
		{
			return _modelConventionInputBuilder;
		}
	}
}
