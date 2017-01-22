// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

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
        /// Triggers <see cref="ValueFormat.StartUp"/> method if called first.
        /// </summary>
        private static readonly ValueFormat fmt = ValueFormat.Internal;

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
        /// An instance of the cache provider constructed for the application from the current configuration.
        /// </summary>
        private static ILookupCacheProvider cacheProvider { get; set; }

        /// <summary>
        /// Gets an instance of a lookup cache of the specified type.
        /// Typically either <see cref="LookupCache.Global"/> or <see cref="LookupCache.User"/> constants are used as a cache type.
        /// </summary>
        /// <param name="cacheType">Cache type.</param>
        /// <returns>An instance of a lookup cache of the specified type or <c>null</c> if no cache can be found.</returns>
        public static LookupCache Get(string cacheType)
        {
            if (cacheProvider == null && DI.DefaultServiceProvider != null)
                cacheProvider = DI.DefaultServiceProvider.GetService<ILookupCacheProvider>();
            if (cacheProvider == null)
            {
                Trace.TraceWarning("No ILookupCacheProvider service is configured. Using default SingletonLookupCacheProvider.");
                cacheProvider = new SingletonLookupCacheProvider();
            }
            LookupCache res = cacheProvider.GetLookupCache(cacheType);
            if (res != null) res.CacheType = cacheType;
            return res;
        }

        /// <summary>
        /// Static list of registered lookup cache loaders.
        /// </summary>
        private static List<ILookupCacheLoader> cacheLoaders = new List<ILookupCacheLoader>();

        /// <summary>
        /// Statically registers the given lookup cache loader.
        /// </summary>
        /// <param name="loader">The lookup cache loader to register.</param>
        public static void AddCacheLoader(ILookupCacheLoader loader)
        {
            if (!cacheLoaders.Contains(loader)) cacheLoaders.Add(loader);
        }
        #endregion

        /// <summary>
        /// Cache type of the current cache.
        /// </summary>
        public string CacheType { get; internal set; }

        /// <summary>
        /// A cache of lookup tables by type.
        /// </summary>
        private Dictionary<string, LookupTable> cache = new Dictionary<string, LookupTable>();

        /// <summary>
        /// A dictionary by lookup table type of listeners
        /// waiting to be notified when the lookup table is loaded.
        /// </summary>
        private Dictionary<string, LookupTableReady> notifyQueues = new Dictionary<string, LookupTableReady>();

        /// <summary>
        /// An internal reader/writer lock to synchronize access to the data.
        /// </summary>
        private ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        /// <summary>
        /// A delegate for notifying the caller that requested a lookup table that this table
        /// has been loaded when the latter happened asynchronously (e.g. a WCF service call in Silverlight).
        /// </summary>
        /// <param name="type">The type of lookup table that has been loaded.</param>
        public delegate void LookupTableReady(string type);

        /// <summary>
        /// Gets a lookup table of the specified type from the cache.
        /// </summary>
        /// <param name="type">Lookup table type.</param>
        /// <returns>A lookup table of the specified type or <c>null</c> if no lookup table can be found.</returns>
        public virtual LookupTable GetLookupTable(string type)
        {
            return GetLookupTable(type, null);
        }

        /// <summary>
        /// Gets a lookup table of the specified type from the cache.
        /// </summary>
        /// <param name="type">Lookup table type.</param>
        /// <param name="onReadyCallback">The method to call when the loading is complete if it happened asynchronously.</param>
        /// <returns>A lookup table of the specified type or <c>null</c> if no lookup table can be found.</returns>
        public virtual LookupTable GetLookupTable(string type, LookupTableReady onReadyCallback)
        {
            if (type == null) return null;
            rwLock.EnterUpgradeableReadLock();
            try
            {
                if (!cache.ContainsKey(type)) LoadLookupTable(type, onReadyCallback);
                return cache.ContainsKey(type) ? cache[type] : null;
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// A subroutine for loading the lookup table if it's not loaded.
        /// </summary>
        /// <param name="type">The type of the lookup table to load.</param>
        /// <param name="onReadyCallback">The method to call when the loading is complete if it happened asynchronously.</param>
        protected virtual void LoadLookupTable(string type, LookupTableReady onReadyCallback)
        {
            // Protection from queuing up listeners for a table type that is not supported,
            // which will never be notified thereby creating a memory leak.
            if (!cacheLoaders.Any(cl => cl.IsSupported(CacheType, type)))
            {
                notifyQueues.Remove(type);
                return;
            }
            if (notifyQueues.ContainsKey(type))
            { // The table is already being loaded, so just add the listener to the queue to be notified.
                LookupTableReady notify = notifyQueues[type];
                if (notify == null) notify = onReadyCallback;
                else if (onReadyCallback != null) notify += onReadyCallback;
                notifyQueues[type] = notify;
            }
            else
            { // Set up the notify queue and start loading.
                notifyQueues[type] = onReadyCallback;
                foreach (ILookupCacheLoader cl in cacheLoaders) cl.Load(this, type);
            }
        }

        /// <summary>
        /// Removes the lookup table of the specified type from the cache.
        /// This method can be used to trigger reloading of the lookup table next time it is requested.
        /// </summary>
        /// <param name="type">The type of the lookup table to remove.</param>
        public virtual void RemoveLookupTable(string type)
        {
            cache.Remove(type);
            notifyQueues.Remove(type);
        }

        /// <summary>
        /// Stores the given lookup table in the current cache under the table's type.
        /// The lookup table and its type should not be <c>null</c>.
        /// </summary>
        /// <param name="table">A lookup table to store.</param>
        public virtual void CacheLookupTable(LookupTable table)
        {
            if (table == null || table.Type == null) return;
            rwLock.EnterWriteLock();
            try
            {
                cache[table.Type] = table;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
            LookupTableReady notify;
            if (notifyQueues.TryGetValue(table.Type, out notify) && notify != null) notify(table.Type);
            notifyQueues.Remove(table.Type);
        }
    }
}
