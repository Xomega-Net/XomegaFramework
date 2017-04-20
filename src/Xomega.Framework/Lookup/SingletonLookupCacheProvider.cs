// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System;

namespace Xomega.Framework.Lookup
{
    /// <summary>
    /// A default implementation of the <c>ILookupCacheProvider</c> interface.
    /// For any cache type it returns the same global instance of the lookup cache.
    /// </summary>
    public class SingletonLookupCacheProvider : ILookupCacheProvider
    {
        /// <summary>
        /// The global instance of the lookup cache provider.
        /// </summary>
        private static LookupCache globalInstance;

        /// <summary>
        /// Service provider for the lookup cache provider
        /// </summary>
        protected IServiceProvider serviceProvider;

        /// <summary>
        /// Constructs a singleton lookup cache provider
        /// </summary>
        /// <param name="serviceProvider">Service provider to use</param>
        public SingletonLookupCacheProvider(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets the global instance of a lookup cache irrespective of the specified type.
        /// </summary>
        /// <param name="type">Cache type.</param>
        /// <returns>The global instance of a lookup cache.</returns>
        public virtual LookupCache GetLookupCache(string type)
        {
            if (globalInstance == null)
                globalInstance = new LookupCache(serviceProvider, LookupCache.Global);
            return globalInstance;
        }
    }
}
