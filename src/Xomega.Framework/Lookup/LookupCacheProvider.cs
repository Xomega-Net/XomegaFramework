// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System;

namespace Xomega.Framework.Lookup
{
    /// <summary>
    /// An interface that allows providing lookup caches for a give cache type.
    /// </summary>
    public interface ILookupCacheProvider
    {
        /// <summary>
        /// Gets an instance of a lookup cache of the specified type.
        /// Typically either <see cref="LookupCache.Global"/> or <see cref="LookupCache.User"/> constants are used as a cache type.
        /// </summary>
        /// <param name="type">Cache type.</param>
        /// <returns>An instance of a lookup cache of the specified type or <c>null</c> if no cache can be found.</returns>
        LookupCache GetLookupCache(string type);
    }

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
        /// Gets the global instance of a lookup cache irrespective of the specified type.
        /// </summary>
        /// <param name="type">Cache type.</param>
        /// <returns>The global instance of a lookup cache.</returns>
        public LookupCache GetLookupCache(string type)
        {
            if (globalInstance == null) globalInstance = new LookupCache();
            return globalInstance;
        }
    }
}
