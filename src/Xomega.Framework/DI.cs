// Copyright (c) 2024 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using Xomega.Framework.Lookup;
using Xomega.Framework.Operators;

namespace Xomega.Framework
{
    /// <summary>
    /// Dependency injection support
    /// </summary>
    public static class DI
    {
        /// <summary>
        /// The default (root) service provider for the application
        /// </summary>
        public static IServiceProvider DefaultServiceProvider { get; internal set; }

        /// <summary>
        /// Configures specified service container with implementation of an error list and a default error parser.
        /// </summary>
        /// <param name="container">Service container to configure.</param>
        /// <param name="fullException">True to make error parser include full details of unhandled exceptions. False otherwise.</param>
        public static void AddErrors(this IServiceCollection container, bool fullException)
        {
            container.AddScoped<ErrorList>();
            container.AddSingleton(sp => new ErrorParser(sp, fullException));
        }

        /// <summary>
        /// Registers WebLookupCacheProvider with the container
        /// </summary>
        /// <param name="container">Service container to configure</param>
        public static void AddSingletonLookupCacheProvider(this IServiceCollection container)
        {
            container.AddSingleton<ILookupCacheProvider, DefaultLookupCacheProvider>();
        }

        /// <summary>
        /// Configures specified service container with implementation of an operator registry
        /// </summary>
        /// <param name="container">Service container to configure</param>
        public static void AddOperators(this IServiceCollection container)
        {
            container.AddSingleton<OperatorRegistry>();
        }

        /// <summary>
        /// Registers an XmlLookupCacheLoader constructed from an XML resource embedded in the specified assembly.
        /// </summary>
        /// <param name="container">Service container to configure</param>
        /// <param name="asm">Assembly with the resource</param>
        /// <param name="enumResource">Resource name</param>
        /// <param name="caseSensitive">True to build case-sensitive cache, which improves performance. False otherwise.</param>
        public static void AddXmlResourceCacheLoader(this IServiceCollection container, Assembly asm, string enumResource, bool caseSensitive)
        {
            foreach (string resource in asm.GetManifestResourceNames())
            {
                if (resource.EndsWith(enumResource))
                {
                    container.AddSingleton<ILookupCacheLoader>(new XmlLookupCacheLoader(asm.GetManifestResourceStream(resource), caseSensitive));
                    break;
                }
            }
        }
    }
}
