// Copyright (c) 2025 Xomega.Net. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Xomega.Framework.Lookup
{
    /// <summary>
    /// A specialized lookup cache loader that allows loading look up tables from XML format.
    /// Generally it can be used in simple cases when the static data can be defined in XML format
    /// rather than stored in a relational database structure.
    /// </summary>
    public class XmlLookupCacheLoader : LookupCacheLoader
    {
        /// <summary>
        /// Reference to the XML document for the static data.
        /// </summary>
        protected XDocument doc;

        /// <summary>
        /// Constructs an XML lookup cache loader from the given XML stream.
        /// </summary>
        /// <param name="stream">The XML stream that contains the lookup data.</param>
        public XmlLookupCacheLoader(Stream stream) : this(stream, false) { }

        /// <summary>
        /// Constructs an XML lookup cache loader from the given XML stream.
        /// </summary>
        /// <param name="stream">The XML stream to load the lookup data from.</param>
        /// <param name="caseSensitive">A flag indicating whether or not the lookup tables should be case-sensitive.
        /// Typically this is based on whether or not the application database is case-sensitive.</param>
        public XmlLookupCacheLoader(Stream stream, bool caseSensitive)
            : this(stream, LookupCache.Global, caseSensitive) { }

        /// <summary>
        /// Constructs an XML lookup cache loader from the given XML stream.
        /// </summary>
        /// <param name="stream">The XML stream to load the lookup data from.</param>
        /// <param name="cacheType">Initializes the type of cache that this loader applies too.</param>
        /// <param name="caseSensitive">A flag indicating whether or not the lookup tables should be case-sensitive.
        /// Typically this is based on whether or not the application database is case-sensitive.</param>
        public XmlLookupCacheLoader(Stream stream, string cacheType, bool caseSensitive)
            : base(null, cacheType, caseSensitive)
        {
            doc = XDocument.Load(XmlReader.Create(stream));
        }

        /// <summary>
        /// Loads Xomega enumerations in Xomega XML format into the current cache from the given stream.
        /// </summary>
        /// <param name="tableType">The lookup table type to load.</param>
        /// <param name="updateCache">The method to call to store the loaded lookup table in the cache.</param>
        /// <param name="token">Cancellation token.</param>
        protected override async Task LoadCacheAsync(string tableType, CacheUpdater updateCache, CancellationToken token = default)
        {
            await base.LoadCacheAsync(tableType, updateCache, token);
            string ns = "http://www.xomega.net/omodel";
            foreach (XElement enm in doc.Descendants(XName.Get("enum", ns)))
            {
                string type = enm.Attribute(XName.Get("name")).Value;
                List<Header> data = new List<Header>();
                foreach (XElement itemNode in enm.Elements(XName.Get("item", ns)))
                {
                    XElement textNode = itemNode.Element(XName.Get("text", ns));
                    string text = (textNode != null) ? textNode.Value : itemNode.Attribute(XName.Get("name")).Value;
                    string id = itemNode.Attribute(XName.Get("value")).Value;
                    Header item = new Header(type, id, text);
                    foreach (XElement prop in itemNode.Elements(XName.Get("prop", ns)))
                    {
                        string pName = prop.Attribute(XName.Get("ref")).Value;
                        string pVal = prop.Attribute(XName.Get("value")).Value;
                        if (item[pName] == null) item[pName] = pVal;
                        else item.AddToAttribute(pName, pVal);
                    }
                    XElement properties = itemNode.Parent.Element(XName.Get("properties", ns));
                    if (properties != null)
                    {
                        foreach (XElement prop in properties.Elements(XName.Get("property", ns)))
                        {
                            XAttribute def = prop.Attribute(XName.Get("default"));
                            string pName = prop.Attribute(XName.Get("name")).Value;
                            if (def != null && item[pName] == null) item[pName] = def.Value;
                        }
                    }
                    data.Add(item);
                }
                updateCache(new LookupTable(type, data, caseSensitive));
            }
        }
    }
}
