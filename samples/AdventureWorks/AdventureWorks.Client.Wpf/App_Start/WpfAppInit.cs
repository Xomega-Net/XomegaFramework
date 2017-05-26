using AdventureWorks.Client.Objects;
using AdventureWorks.Client.ViewModels;
using AdventureWorks.Services;
using AdventureWorks.Services.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xomega.Framework;
using Xomega.Framework.Views;

namespace AdventureWorks.Client.Wpf
{
    public class WpfAppInit : AppInitializer
    {
        public override IServiceProvider ConfigureServices()
        {
            IServiceCollection container = new ServiceCollection();

            // framework services configuration
            container.AddErrors();
            container.AddTransient<IErrorPresenter, ErrorsView>();
            container.AddSingletonLookupCacheProvider();

            // app services configuration
            // NOTE: make sure to build the Xomega model project first for the code below to compile
            container.AddDataObjects();
            container.AddViewModels();
            container.AddViews();
            container.AddLookupCacheLoaders();
            container.AddXmlResourceCacheLoader(typeof(Enumerations.Operators).Assembly, ".enumerations.xml", true);

            //container.AddServices();
            container.AddWcfServices();


            // TODO: configure container with other services as needed

            return container.BuildServiceProvider();
        }
    }
}
