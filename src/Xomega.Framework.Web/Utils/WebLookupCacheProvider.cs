// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Web;

namespace Xomega.Framework.Lookup
{
    /// <summary>
    /// An implementation of the <c>ILookupCacheProvider</c> interface for the web.
    /// For a user cache type it returns an instance of the lookup cache from the user session.
    /// Otherwise it returns a global instance for the application.
    /// </summary>
    public class WebLookupCacheProvider : DefaultLookupCacheProvider
    {
        private const string SessionKey = "ILookupCacheProvider";

        /// <summary>
        /// Constructs a web lookup cache provider
        /// </summary>
        /// <param name="serviceProvider">Service provider to use</param>
        public WebLookupCacheProvider(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// For a user cache type returns an instance of the lookup cache from the user session.
        /// Otherwise it returns a global instance for the application.
        /// </summary>
        /// <param name="type">Cache type.</param>
        /// <returns>An instance of a lookup cache of the given type.</returns>
        public override LookupCache GetLookupCache(string type)
        {
            if (type == LookupCache.User && HttpContext.Current?.Session != null)
            {
                if (!(HttpContext.Current.Session[SessionKey] is LookupCache userCache))
                {
                    userCache = new LookupCache(serviceProvider, null, LookupCache.User);
                    HttpContext.Current.Session[SessionKey] = userCache;
                }
                return userCache;
            }
            return base.GetLookupCache(type);
        }
    }
}
