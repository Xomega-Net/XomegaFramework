// Copyright (c) 2021 Xomega.Net. All rights reserved.


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
}
