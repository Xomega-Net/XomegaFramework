// Copyright (c) 2010-2016 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
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
        public static void AddWcfErrorParser(this IServiceCollection container)
        {
            container.AddSingleton<ErrorParser, FaultErrorParser>();
        }

        /// <summary>
        /// Registers WCF client type mappings from the given configuration excluding specified endpoints if needed.
        /// </summary>
        public static IServiceCollection AddWcfClientServices(this IServiceCollection container, ContextInformation ctx, Func<ChannelEndpointElement, bool> exclEndpoints)
        {
            String clientPath = "system.serviceModel/client";
            ClientSection client = (ClientSection)(ctx != null ? ctx.GetSection(clientPath) : ConfigurationManager.GetSection(clientPath));
            foreach (ChannelEndpointElement endpoint in client.Endpoints)
            {
                if (endpoint.Name == null || endpoint.Contract == null || exclEndpoints != null && exclEndpoints(endpoint)) continue;
                Type contractType = GetType(endpoint.Contract);
                Type factoryType = ctx != null ?
                    typeof(ConfigurationChannelFactory<>).MakeGenericType(contractType) :
                    typeof(ChannelFactory<>).MakeGenericType(contractType);
                container.AddSingleton(factoryType, sp => ctx == null ? Activator.CreateInstance(factoryType, endpoint.Name) :
                    Activator.CreateInstance(factoryType, endpoint.Name, ctx, null));
                container.AddScoped(contractType, sp => factoryType.InvokeMember("CreateChannel", BindingFlags.InvokeMethod, null,
                    sp.GetService(factoryType), new object[] { }));
            }
            return container;
        }

        /// <summary>
        /// Registers WCF services type mappings from the given configuration excluding specified endpoints if needed.
        /// </summary>
        public static IServiceCollection AddWcfServices(this IServiceCollection container, ContextInformation ctx, Func<ServiceEndpointElement, bool> exclEndpoints)
        {
            String servicesPath = "system.serviceModel/services";
            ServicesSection services = (ServicesSection)(ctx != null ? ctx.GetSection(servicesPath) : ConfigurationManager.GetSection(servicesPath));
            foreach (ServiceElement service in services.Services)
            {
                Type serviceType = GetType(service.Name);
                if (serviceType == null) continue;
                foreach (ServiceEndpointElement endpoint in service.Endpoints)
                {
                    Type contractType = GetType(endpoint.Contract);
                    if (contractType == null || endpoint.IsSystemEndpoint || exclEndpoints != null && exclEndpoints(endpoint)) continue;
                    container.AddScoped(contractType, serviceType);
                }
            }
            return container;
        }

        /// <summary>
        /// Get type by name looking in all current assemblies if needed
        /// </summary>
        /// <param name="name">type name</param>
        /// <returns>Type object for the given type name</returns>
        public static Type GetType(String name)
        {
            if (name == null) return null;
            Type type = Type.GetType(name);
            if (type != null) return type;
            else
            {
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = asm.GetType(name, false);
                    if (type != null) return type;
                }
            }
            return null;
        }
    }
}
