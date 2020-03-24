// Copyright (c) 2020 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xomega.Framework.Lookup
{
    /// <summary>
    /// A class that represents a cache of lookup tables by their types.
    /// It provides a static accessor function to get an instance of lookup cache by cache type.
    /// It also supports loading the lookup data from Xomega enumerations in XML format.
    /// </summary>
    public class LookupCache
    {
        #region Static accessors

        /// <summary>
        /// A constant that represents a global lookup cache that is shared for the whole application.
        /// This is typically a default option for the cache type to get.
        /// </summary>
        public const string Global = "GlobalLookupCache";

        /// <summary>
        /// A constant that represents a lookup cache for the current user session.
        /// </summary>
        public const string User = "UserLookupCache";

        /// <summary>
        /// A constant that represents a local lookup cache for the current property.
        /// </summary>
        public const string Local = "LocalLookupCache";

        /// <summary>
        /// Gets an instance of a lookup cache of the specified type.
        /// Typically either <see cref="Global"/> or <see cref="User"/> constants are used as a cache type.
        /// </summary>
        /// <param name="serviceProvider">Service provider to use.</param>
        /// <param name="cacheType">Cache type.</param>
        /// <returns>An instance of a lookup cache of the specified type or <c>null</c> if no cache can be found.</returns>
        public static LookupCache Get(IServiceProvider serviceProvider, string cacheType)
        {
            ILookupCacheProvider cacheProvider = serviceProvider.GetService<ILookupCacheProvider>();
            if (cacheProvider == null)
            {
                Trace.TraceWarning("No ILookupCacheProvider service is configured. Using default SingletonLookupCacheProvider.");
                cacheProvider = new DefaultLookupCacheProvider(serviceProvider);
            }
            return cacheProvider.GetLookupCache(cacheType);
        }

        #endregion

        /// <summary>
        /// Cache type of the current cache.
        /// </summary>
        public string CacheType { get; private set; }

        /// <summary>
        /// Static list of registered lookup cache loaders.
        /// </summary>
        private readonly IEnumerable<ILookupCacheLoader> cacheLoaders;

        /// <summary>
        /// A cache of lookup tables by type.
        /// </summary>
        private readonly ConcurrentDictionary<string, LookupTable> cache = new ConcurrentDictionary<string, LookupTable>();

        /// <summary>
        /// An internal semaphore to ensure that the cache is loaded by one thread at a time.
        /// </summary>
        private readonly SemaphoreSlim loadSem = new SemaphoreSlim(1,1);

        /// <summary>
        /// Constructs a new lookup cache of the specified type.
        /// </summary>
        /// <param name="serviceProvider">Service provider for the cache</param>
        /// <param name="cacheLoaders">A list of specific cache loaders for the current cache.
        /// If null, cache loaders registered with the service provider will be used.</param>
        /// <param name="type">Cache type</param>
        public LookupCache(IServiceProvider serviceProvider, List<ILookupCacheLoader> cacheLoaders, string type)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            this.cacheLoaders = cacheLoaders ?? serviceProvider.GetServices<ILookupCacheLoader>() ?? new List<ILookupCacheLoader>();
            CacheType = type;
        }

        /// <summary>
        /// Gets a lookup table of the specified type from the cache.
        /// </summary>
        /// <param name="type">Lookup table type.</param>
        /// <returns>A lookup table of the specified type or <c>null</c> if no lookup table can be found.</returns>
        public virtual LookupTable GetLookupTable(string type)
        {
            if (type != null && cache.TryGetValue(type, out LookupTable tbl)) return tbl;
            // if the type is not in the cache, try loading it asynchronously and wait for results
            return Task.Run(async () => { return await GetLookupTableAsync(type); }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets a lookup table of the specified type from the cache.
        /// </summary>
        /// <param name="type">Lookup table type.</param>
        /// <returns>A lookup table of the specified type or <c>null</c> if no lookup table can be found.</returns>
        /// <param name="token">Cancellation token.</param>
        public async Task<LookupTable> GetLookupTableAsync(string type, CancellationToken token = default)
        {
            if (type == null) return null;
            if (!cache.ContainsKey(type))
            {
                try
                {
                    await loadSem.WaitAsync();
                    if (!cache.ContainsKey(type))
                        await LoadLookupTableAsync(type, token);
                }
                finally
                {
                    loadSem.Release();
                }
            }
            return cache.ContainsKey(type) ? cache[type] : null;
        }

        /// <summary>
        /// A subroutine for loading the lookup table if it's not loaded.
        /// </summary>
        /// <param name="type">The type of the lookup table to load.</param>
        /// <param name="token">Cancellation token.</param>
        protected virtual async Task LoadLookupTableAsync(string type, CancellationToken token = default)
        {
            await Task.WhenAll(cacheLoaders.Where(cl => cl.IsSupported(CacheType, type))
                .Select(cl => cl.LoadAsync(this, type, token)));
        }

        /// <summary>
        /// Removes the lookup table of the specified type from the cache.
        /// This method can be used to trigger reloading of the lookup table next time it is requested.
        /// </summary>
        /// <param name="type">The type of the lookup table to remove.</param>
        public virtual void RemoveLookupTable(string type)
        {
            cache.TryRemove(type, out _);
        }

        /// <summary>
        /// Stores the given lookup table in the current cache under the table's type.
        /// The lookup table and its type should not be <c>null</c>.
        /// </summary>
        /// <param name="table">A lookup table to store.</param>
        public virtual void CacheLookupTable(LookupTable table)
        {
            if (table == null || table.Type == null) return;
            cache[table.Type] = table;
        }
    }
}
