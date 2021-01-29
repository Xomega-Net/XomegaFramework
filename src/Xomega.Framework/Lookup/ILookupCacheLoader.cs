// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System.Threading;
using System.Threading.Tasks;

namespace Xomega.Framework.Lookup
{
    /// <summary>
    /// An interface for all lookup cache loaders, which can be statically registered with the LookupTable
    /// class and called by the framework to populate a given lookup table if it hasn't been loaded yet.
    /// </summary>
    public interface ILookupCacheLoader
    {
        /// <summary>
        /// Determines if the given cache type and table type are supported by the current cache loader.
        /// </summary>
        /// <param name="cacheType">The cache type to check.</param>
        /// <param name="tableType">The table type to check.</param>
        /// <returns>True, if the given cache type and table type are supported by the current cache loader,
        /// False otherwise.</returns>
        bool IsSupported(string cacheType, string tableType);

        /// <summary>
        /// Loads a lookup table for the specified type into the given lookup cache.
        /// The implementation should check the cache type and the table type and do nothing
        /// if the current lookup cache loader is not applicable for those.
        /// </summary>
        /// <param name="cache">The lookup cache where to populate the lookup table.</param>
        /// <param name="tableType">The type of the lookup table to populate.</param>
        /// <param name="token">Cancellation token.</param>
        Task LoadAsync(LookupCache cache, string tableType, CancellationToken token = default);
    }
}
