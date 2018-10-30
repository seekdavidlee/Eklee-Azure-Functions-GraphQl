using System;
using Autofac;
using GraphQL;

namespace Eklee.Azure.Functions.GraphQl
{
    public class GraphDependencyResolver : IDependencyResolver
    {
        private readonly IComponentContext _componentContext;

        public GraphDependencyResolver(IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

        public T Resolve<T>()
        {
            return _componentContext.Resolve<T>();
        }

        public object Resolve(Type type)
        {
            return _componentContext.Resolve(type);
        }
    }
}
