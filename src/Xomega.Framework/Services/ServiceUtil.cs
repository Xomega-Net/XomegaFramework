// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Configuration;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Static utility methods for working with services
    /// </summary>
    public static class ServiceUtil
    {
        /// <summary>
        /// The default (root) service provider for the application
        /// </summary>
        public static IServiceProvider DefaultServiceProvider { get; set; }

        #region Auto-mapping

        /// <summary>
        /// Copies the values of all properties from the source object to the
        /// corresponding properties in the destination object.
        /// </summary>
        /// <param name="src">The source object.</param>
        /// <param name="dest">The destination object.</param>
        public static void CopyProperties(object src, object dest)
        {
            if (src == null || dest == null) return;
            CopyProperties(src, dest, src.GetType().GetProperties());
        }

        /// <summary>
        /// Copies the values of the specified properties from the source object
        /// to the corresponding properties in the destination object.
        /// </summary>
        /// <param name="src">The source object.</param>
        /// <param name="dest">The destination object.</param>
        /// <param name="props">The list of fields to copy.</param>
        public static void CopyProperties(object src, object dest, IEnumerable<PropertyInfo> props)
        {
            if (src == null || dest == null) return;
            foreach (PropertyInfo srcProp in props)
            {
                object val = srcProp.GetValue(src, null);
                PropertyInfo destProp = dest.GetType().GetProperty(srcProp.Name, srcProp.PropertyType);
                if (destProp != null && !destProp.GetCustomAttributes(typeof(EdmScalarPropertyAttribute), false)
                        .Cast<EdmScalarPropertyAttribute>().Any(a => a.EntityKeyProperty))
                    destProp.SetValue(dest, val, null);
            }
        }

        #endregion

        #region WCF configuration

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

        /// <summary>
        /// Release WCF service to prepare it for proper disposal
        /// </summary>
        /// <param name="service">Service instance to release</param>
        public static void ReleaseService(object service)
        {
            ICommunicationObject co = service as ICommunicationObject;
            if (co != null && co.State == CommunicationState.Faulted)
                co.Abort();
        }

        #endregion
    }
}
