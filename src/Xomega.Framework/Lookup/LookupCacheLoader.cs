// Copyright (c) 2019 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xomega.Framework.Lookup
{
    /// <summary>
    /// A base abstract class for the lookup cache loader implementations.
    /// It is designed to support cache loaders that either explicitly specify the table types
    /// they can load or load all their lookup tables at once during the first time they run,
    /// which will determine their supported table types.
    /// </summary>
    public abstract class LookupCacheLoader : ILookupCacheLoader
    {
        /// <summary>
        /// Service provider for the lookup cache loader
        /// </summary>
        protected IServiceProvider serviceProvider;

        /// <summary>
        /// The cache type that this loader is designed for. This is null if any type is supported.
        /// </summary>
        protected string cacheType;

        /// <summary>
        /// The list of supported table types, which is either
        /// specified initially or constructed from the first run.
        /// </summary>
        protected ICollection<string> supportedTypes;

        /// <summary>
        /// Indicates whether or not the loaded lookup tables should be case sensitive.
        /// </summary>
        protected bool caseSensitive;

        /// <summary>
        /// Initializes base parameters of the lookup cache loader.
        /// </summary>
        /// <param name="serviceProvider">Service provider for the lookup cache loader</param>
        /// <param name="cacheType">The cache type that this loader is designed for.
        /// It should be null if any type is supported.</param>
        /// <param name="caseSensitive">Indicates whether or not the loaded lookup tables should be case sensitive.</param>
        /// <param name="tableTypes">A list of lookup table types that this loader can load.
        /// If null, the list will be determined based on the first run.</param>
        protected LookupCacheLoader(IServiceProvider serviceProvider, string cacheType, bool caseSensitive, params string[] tableTypes)
        {
            this.serviceProvider = serviceProvider;
            this.cacheType = cacheType;
            this.caseSensitive = caseSensitive;
            if (tableTypes != null && tableTypes.Length > 0)
                supportedTypes = new HashSet<string>(tableTypes);
        }

        /// <summary>
        /// Determines if the given cache type and table type are supported by the current cache loader.
        /// </summary>
        /// <param name="cacheType">The cache type to check.</param>
        /// <param name="tableType">The table type to check.</param>
        /// <returns>True, if the given cache type and table type are supported by the current cache loader,
        /// False otherwise.</returns>
        public virtual bool IsSupported(string cacheType, string tableType)
        {
            return (this.cacheType == null || this.cacheType == cacheType) &&
                (supportedTypes == null || supportedTypes.Contains(tableType));
        }

        /// <summary>
        /// Loads a lookup table for the specified type into the given lookup cache.
        /// Implementation of the corresponding interface method.
        /// </summary>
        /// <param name="cache">The lookup cache where to populate the lookup table.</param>
        /// <param name="tableType">The type of the lookup table to populate.</param>
        /// <param name="token">Cancellation token.</param>
        public virtual async Task LoadAsync(LookupCache cache, string tableType, CancellationToken token = default)
        {
            void cacheUpdater(LookupTable table)
            {
                cache.CacheLookupTable(table);
                // ensure supportedTypes gets populated
                if (supportedTypes == null) supportedTypes = new HashSet<string>();
                supportedTypes.Add(table.Type);
            }
            await LoadCacheAsync(tableType, cacheUpdater, token);
        }

        /// <summary>
        /// A delegate that a subclass should call in the <see cref="LoadCacheAsync(string, CacheUpdater, CancellationToken)"/>
        /// method after creating and loading a lookup table to actually store it in the cache.
        /// </summary>
        /// <param name="table">The created and loaded LookupTable to be stored in the cache.</param>
        protected delegate void CacheUpdater(LookupTable table);

        /// <summary>
        /// Asynchronous subroutine implemented by subclasses to perform the actual loading
        /// of the lookup table and storing it in the cache using the provided updateCache delegate.
        /// </summary>
        /// <param name="tableType">The lookup table type to load.</param>
        /// <param name="updateCache">The method to call to store the loaded lookup table in the cache.</param>
        /// <param name="token">Cancellation token.</param>
        protected virtual async Task LoadCacheAsync(string tableType, CacheUpdater updateCache, CancellationToken token = default)
        {
            // subclasses can implement this
            await Task.CompletedTask;
        }

        /// <summary>
        /// Synchronous wrapper for loading of the lookup table and storing it in the cache using the provided updateCache delegate.
        /// The method invokes the asynchronous routine and waits for it to complete.
        /// </summary>
        /// <param name="tableType">The lookup table type to load.</param>
        /// <param name="updateCache">The method to call to store the loaded lookup table in the cache.</param>
        protected void LoadCache(string tableType, CacheUpdater updateCache)
        {
            Task.Run(async () => await LoadCacheAsync(tableType, updateCache)).Wait();
        }

        /// <summary>
        /// Helper method to set the active flag from a boolean.
        /// </summary>
        protected bool IsActive(bool active) => active;

        /// <summary>
        /// Helper method to set the active flag from a nullable boolean that defaults to true.
        /// </summary>
        protected bool IsActive(bool? active) => active ?? true;
    }
}
