// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xomega.Framework.Lookup
{
    /// <summary>
    /// Base class for lookup cache loaders that have a local cache and load it
    /// using specified set of input parameters.
    /// </summary>
    public class LocalLookupCacheLoader : LookupCacheLoader
    {
        /// <summary>
        /// The local lookup cache that this cache loader loads.
        /// </summary>
        public LookupCache LocalCache { get; private set; }

        /// <summary>
        /// Returns the specific lookup table that this cache loader populates.
        /// Defaults to the first of the supported table types.
        /// </summary>
        public virtual string TableType => supportedTypes.FirstOrDefault();

        /// <summary>
        /// A dictionary of named input parameters and their values to load the cache.
        /// </summary>
        protected Dictionary<string, object> Parameters { get; private set; }

        /// <summary>
        /// Constructs a new local lookup cache loader from the given service provider for the specified type(s).
        /// </summary>
        /// <param name="serviceProvider">Service provider to use for loading the local cache.</param>
        /// <param name="caseSensitive">Indicates whether or not the loaded lookup tables should be case sensitive.</param>
        /// <param name="tableTypes">A list of lookup table types that this loader can load.
        /// If null, the list will be determined based on the first run.</param>
        public LocalLookupCacheLoader(IServiceProvider serviceProvider, bool caseSensitive, params string[] tableTypes)
            : base(serviceProvider, LookupCache.Local, caseSensitive, tableTypes)
        {
            LocalCache = new LookupCache(serviceProvider, new List<ILookupCacheLoader>() { this }, cacheType);
            Parameters = new Dictionary<string, object>();
        }

        /// <summary>
        /// Sets input parameters for this cache loader and reloads the local cache based on the new parameters.
        /// </summary>
        /// <param name="parameters">New input parameters for the cache loader.</param>
        /// <param name="token">Cancellation token.</param>
        public async Task SetParametersAsync(Dictionary<string, object> parameters, CancellationToken token = default)
        {
            Parameters = parameters;
            foreach (var type in supportedTypes)
                LocalCache.RemoveLookupTable(type);
            foreach (var type in supportedTypes)
                await LocalCache.GetLookupTableAsync(type, token);
        }
    }
}