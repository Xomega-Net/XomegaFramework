// Copyright (c) 2010-2016 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System.Web;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// Dependency injection utilities for web projects
    /// </summary>
    public class WebDI
    {
        /// <summary>
        /// The service scope for the current request
        /// </summary>
        public static IServiceScope CurrentServiceScope
        {
            get { return HttpContext.Current == null ? null : HttpContext.Current.Items["IServiceScope"] as IServiceScope; }
            set { if (HttpContext.Current != null) HttpContext.Current.Items["IServiceScope"] = value; }
        }

    }
}
