using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;

namespace AdventureWorks.Services.Rest
{
    public class DependencyResolver : IDependencyResolver
    {
        private IServiceScope scope;
        private IServiceProvider root;

        public DependencyResolver(IServiceScope svcScope)
        {
            scope = svcScope;
        }

        public DependencyResolver(IServiceProvider rootProvider)
        {
            root = rootProvider;
        }

        private IServiceProvider ServiceProvider { get { return scope != null ? scope.ServiceProvider : root; } }

        public IDependencyScope BeginScope()
        {
            return new DependencyResolver(ServiceProvider.CreateScope());
        }

        public void Dispose()
        {
            if (scope != null) scope.Dispose();
        }

        public object GetService(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return ServiceProvider.GetServices(serviceType);
        }
    }
}