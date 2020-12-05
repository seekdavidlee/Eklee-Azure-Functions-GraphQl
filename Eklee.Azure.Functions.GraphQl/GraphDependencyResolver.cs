using System;
using Autofac;

namespace Eklee.Azure.Functions.GraphQl
{
	public class GraphDependencyResolver : IServiceProvider
    {
        private readonly IComponentContext _componentContext;

        public GraphDependencyResolver(IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

		public object GetService(Type serviceType)
		{
			return _componentContext.Resolve(serviceType);
		}
	}
}
