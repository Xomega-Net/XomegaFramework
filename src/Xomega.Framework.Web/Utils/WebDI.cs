// Copyright (c) 2025 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System.Web;
using Xomega.Framework.Lookup;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// Dependency injection utilities for web projects
    /// </summary>
    public static class WebDI
    {
        /// <summary>
        /// The service scope for the current request
        /// </summary>
        public static IServiceScope CurrentServiceScope
        {
            get { return HttpContext.Current == null ? null : HttpContext.Current.Items["IServiceScope"] as IServiceScope; }
            set { if (HttpContext.Current != null) HttpContext.Current.Items["IServiceScope"] = value; }
        }

        /// <summary>
        /// Registers WebLookupCacheProvider with the container
        /// </summary>
        /// <param name="services">Service container</param>
        public static void AddWebLookupCacheProvider(this IServiceCollection services)
        {
            services.AddSingleton<ILookupCacheProvider, WebLookupCacheProvider>();
        }
    }
}
