// Copyright (c) 2023 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Configuration;

namespace Xomega.Framework.Wcf
{
    /// <summary>
    /// Utilities to support DI configuration of WCF services
    /// </summary>
    public static class WcfDI
    {
        /// <summary>
        /// Registers WCF fault error parser with the service container
        /// </summary>
        /// <param name="container">Service container to configure</param>
        /// <param name="fullException">True to make error parser include full details of unhandled exceptions. False otherwise.</param>
        public static void AddWcfErrorParser(this IServiceCollection container, bool fullException)
        {
            container.AddSingleton<ErrorParser>(sp => new FaultErrorParser(sp, fullException));
        }

        /// <summary>
        /// Registers WCF client type mappings from the given configuration excluding specified endpoints if needed.
        /// </summary>
        public static IServiceCollection AddWcfClientServices(this IServiceCollection container, Func<SecurityToken> tokenProvider,
            ContextInformation ctx, Func<ChannelEndpointElement, bool> exclEndpoints)
        {
            string clientPath = "system.serviceModel/client";
            ClientSection client = (ClientSection)(ctx != null ? ctx.GetSection(clientPath) : ConfigurationManager.GetSection(clientPath));
            foreach (ChannelEndpointElement endpoint in client.Endpoints)
            {
                if (endpoint.Name == null || endpoint.Contract == null || exclEndpoints != null && exclEndpoints(endpoint)) continue;
                Type contractType = AppInitializer.GetType(endpoint.Contract);
                Type factoryType = ctx != null ?
                    typeof(ConfigurationChannelFactory<>).MakeGenericType(contractType) :
                    typeof(ChannelFactory<>).MakeGenericType(contractType);
                container.TryAddSingleton(factoryType, sp => ctx == null ? Activator.CreateInstance(factoryType, endpoint.Name) :
                    Activator.CreateInstance(factoryType, endpoint.Name, ctx, null));
                container.TryAddScoped(contractType, sp => tokenProvider != null ? factoryType.InvokeMember("CreateChannelWithIssuedToken",
                        BindingFlags.InvokeMethod, null, sp.GetService(factoryType), new object[] { tokenProvider() }) :
                    factoryType.InvokeMember("CreateChannel", BindingFlags.InvokeMethod, null, sp.GetService(factoryType), new object[] { }));
            }
            return container;
        }

        /// <summary>
        /// Registers WCF services type mappings from the given configuration excluding specified endpoints if needed.
        /// </summary>
        public static IServiceCollection AddWcfServices(this IServiceCollection container, ContextInformation ctx, Func<ServiceEndpointElement, bool> exclEndpoints)
        {
            string servicesPath = "system.serviceModel/services";
            ServicesSection services = (ServicesSection)(ctx != null ? ctx.GetSection(servicesPath) : ConfigurationManager.GetSection(servicesPath));
            foreach (ServiceElement service in services.Services)
            {
                Type serviceType = AppInitializer.GetType(service.Name);
                if (serviceType == null) continue;
                foreach (ServiceEndpointElement endpoint in service.Endpoints)
                {
                    Type contractType = AppInitializer.GetType(endpoint.Contract);
                    if (contractType == null || endpoint.IsSystemEndpoint || exclEndpoints != null && exclEndpoints(endpoint)) continue;
                    container.TryAddScoped(contractType, serviceType);
                }
            }
            return container;
        }
    }
}
