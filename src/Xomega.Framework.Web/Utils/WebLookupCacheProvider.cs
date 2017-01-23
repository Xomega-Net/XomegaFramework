// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System.Web;

namespace Xomega.Framework.Lookup
{
    /// <summary>
    /// An implementation of the <c>ILookupCacheProvider</c> interface for the web.
    /// For a user cache type it returns an instance of the lookup cache from the user session.
    /// Otherwise it returns a global instance for the application.
    /// </summary>
    public class WebLookupCacheProvider : SingletonLookupCacheProvider
    {
        private const string SessionKey = "ILookupCacheProvider";

        /// <summary>
        /// For a user cache type returns an instance of the lookup cache from the user session.
        /// Otherwise it returns a global instance for the application.
        /// </summary>
        /// <param name="type">Cache type.</param>
        /// <returns>An instance of a lookup cache of the given type.</returns>
        public override LookupCache GetLookupCache(string type)
        {
            if (LookupCache.User.Equals(type) && HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                LookupCache userCache = HttpContext.Current.Session[SessionKey] as LookupCache;
                if (userCache == null)
                {
                    userCache = new LookupCache();
                    HttpContext.Current.Session[SessionKey] = userCache;
                }
                return userCache;
            }
            return base.GetLookupCache(type);
        }
    }
}
