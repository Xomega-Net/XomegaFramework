// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace Xomega.Framework.Lookup
{
    using IndexedTable = Dictionary<string, Header>;

    /// <summary>
    /// A self-indexing lookup table. The data set for the table is based on a list of values of type <c>Header</c>.
    /// The table allows looking up values based on any string represenation of the headers as defined 
    /// by the format string that you pass in. If the data is not indexed by that format, the table will build
    /// and cache the index first.
    /// </summary>
    /// <remarks>
    /// The table returns a deep copy of its values to protect from accidental changes of the data
    /// by the caller, which would otherwise affect all other users of the lookup table.
    /// Access to the table data is synchronized.
    /// </remarks>
    [DataContract]
    public class LookupTable : IEqualityComparer<string>
    {
        /// <summary>
        /// This is a constant that is used when indexing by non-unique fields/formats to store
        /// the rest of items with the same key, while only the first one is returned by the lookup.
        /// This is a prefix, which combined with the format string, forms an attribute name
        /// in the first item where additional items with the same key are stored.
        /// </summary>
        public const string GroupAttrPrefix = "Group:";

        /// <summary>
        /// Raw data as a list.
        /// </summary>
        [DataMember]
        protected IEnumerable<Header> data;

        /// <summary>
        /// Indexed data by key format that is used to get the key.
        /// </summary> 
        protected Dictionary<string, IndexedTable> indexedData = new Dictionary<string, IndexedTable>();

        /// <summary>
        /// A flag of whether or not to use case-sensitive lookups.
        /// </summary>
        [DataMember]
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// An internal reader/writer lock to synchronize access to the data.
        /// </summary>
        protected ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Constructs a new lookup table from the specified data set.
        /// </summary>
        /// <param name="type">Lookup table type string.</param>
        /// <param name="data">A list of headers that serves as the table's data set.</param>
        /// <param name="caseSensitive">A boolean flag of whether or not to perform case sensitive look-ups.</param>
        public LookupTable(string type, IEnumerable<Header> data, bool caseSensitive)
        {
            Type = type;
            this.data = data;
            CaseSensitive = caseSensitive;
            foreach (Header h in data) if (h != null) h.Type = type;
        }

        /// <summary>
        /// A type string for the lookup table.
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        /// <summary>
        /// Temporarily exposed raw data for (de)serializing by System.Text.Json, which needs public properties.
        /// See https://github.com/dotnet/runtime/issues/29743
        /// </summary>
        public IEnumerable<Header> Data { get => data; set => data = value; }

        /// <summary>
        /// Enumerates all values in the table.
        /// </summary>
        /// <returns>An enumeration that contains a copy each value in the table.</returns>
        public IEnumerable<Header> GetValues() { return GetValues(null); }

        /// <summary>
        /// Get a copy of the table values filtered by the supplied function.
        /// Only values that match the filter will be cloned, which is better for performance.
        /// </summary>
        /// <param name="filterFunc">A function to filter the values or <c>null</c> to return all values.</param>
        /// <param name="row">The data row context, if any.</param>
        /// <returns>A filtered enumeration that contains copies of each value matching the filter.</returns>
        public IEnumerable<Header> GetValues(Func<Header, DataRow, bool> filterFunc, DataRow row = null)
        {
            IEnumerable<Header> lst = data;
            if (filterFunc != null) lst = lst.Where(h => filterFunc(h, row));
            return lst.Where(h => h != null).Select(h => h.Clone()).AsEnumerable();
        }

        /// <summary>
        /// Looks up a Header item by the ID field. <see cref="Header.FieldId"/> is used as a format.
        /// </summary>
        /// <param name="id">The ID value to look up by.</param>
        /// <returns>A copy of the Header with the specified ID.</returns>
        public Header LookupById(string id)
        {
            return LookupByFormat(Header.FieldId, id);
        }

        /// <summary>
        /// Looks up an item in the table by a value of the item string representation
        /// specified by the supplied format parameter. If the table is not indexed
        /// by the given format, it builds such an index first.
        /// If multiple items have the same value for the given format, then only the
        /// first one will be returned and the rest of them will be stored in an attribute
        /// with a name composed from the <see cref="GroupAttrPrefix"/> constant and the format string.
        /// </summary>
        /// <param name="format">The format used to evaluate a string value for each item.</param>
        /// <param name="value">The value to look up by.</param>
        /// <returns>A copy of the Header item, for which evaluation of the given format
        /// matches the value provided. If no match is found a <c>null</c> value is returned.</returns>
        public Header LookupByFormat(string format, string value)
        {
            rwLock.EnterUpgradeableReadLock();
            try
            {
                if (!indexedData.TryGetValue(format, out IndexedTable tbl))
                    tbl = BuildIndexedTable(format);
                if (tbl.TryGetValue(value, out Header res)) return res;
                return res;
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Clears all indexes in the table.
        /// The indexes will be rebuilt as needed at the first subsequent attempt to look up a value by any format.
        /// </summary>
        public void ResetIndexes()
        {
            rwLock.EnterWriteLock();
            try
            {
                indexedData.Clear();
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Clears an index for the given format. The index will be rebuilt at the next attempt
        /// to look up a value by this format.
        /// </summary>
        /// <param name="format">Format string.</param>
        /// <returns>true if the index was found and cleared; otherwise false.</returns>
        public bool ClearIndex(string format)
        {
            rwLock.EnterWriteLock();
            try
            {
                return indexedData.Remove(format);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Builds an index for the specified format.
        /// </summary>
        /// <param name="format">Format string.</param>
        /// <returns>An indexed table for the specified format.</returns>
        protected IndexedTable BuildIndexedTable(string format)
        {
            rwLock.EnterWriteLock();
            try
            {
                IndexedTable tbl = new IndexedTable(this);
                foreach (Header h in data)
                {
                    if (h == null) continue;
                    string key = h.ToString(format);
                    if (tbl.TryGetValue(key, out Header h1))
                        h1.AddToAttribute(GroupAttrPrefix + format, h);
                    else tbl[key] = h;
                }
                indexedData[format] = tbl;
                return tbl;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Compares two strings in a case-(in)sensitive manner as specified by the caseSensitive member.
        /// </summary>
        public bool Equals(string x, string y)
        {
            StringComparison opt = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            return string.Equals(x, y, opt);
        }

        /// <summary>
        /// Returns a hash code for a string in a case-(in)sensitive manner as specified by the caseSensitive member.
        /// </summary>
        public int GetHashCode(string obj)
        {
            return obj == null ? 0 : CaseSensitive ? obj.GetHashCode() : obj.ToUpper().GetHashCode();
        }
    }
}
