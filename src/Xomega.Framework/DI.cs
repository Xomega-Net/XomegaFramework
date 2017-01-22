using Microsoft.Extensions.DependencyInjection;
using System;

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
        public static IServiceProvider DefaultServiceProvider { get; set; }

        /// <summary>
        /// Configures specified service container with implementation of an error list and a default error parser
        /// </summary>
        /// <param name="container">Service container to configure</param>
        public static void AddErrors(this IServiceCollection container)
        {
            container.AddScoped<ErrorList>();
            container.AddSingleton<ErrorParser>();
        }
    }
}
