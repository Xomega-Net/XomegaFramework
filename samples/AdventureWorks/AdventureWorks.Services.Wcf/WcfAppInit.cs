using AdventureWorks.Services.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xomega.Framework;
using Xomega.Framework.Wcf;

namespace AdventureWorks.Services.Wcf
{
    public class WcfAppInit : AppInitializer
    {
        public override IServiceProvider ConfigureServices()
        {
            IServiceCollection container = new ServiceCollection();

            // framework services configuration
            container.AddErrors();
            container.AddWcfErrorList();
            container.AddSingletonLookupCacheProvider();

            // app services configuration
            // NOTE: make sure to build the Xomega model project first for the code below to compile
            container.AddServices();
            container.AddSingleton<AppStsConfig>();
            container.AddLookupCacheLoaders();
            container.AddXmlResourceCacheLoader(typeof(Enumerations.Operators).Assembly, ".enumerations.xml", true);

            // TODO: configure container with other services as needed

            return container.BuildServiceProvider();
        }
    }
}
