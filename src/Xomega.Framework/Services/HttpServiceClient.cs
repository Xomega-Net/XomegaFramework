// Copyright (c) 2022 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// A base class for HTTP-based service client classes that use Xomega Framework.
    /// </summary>
    public class HttpServiceClient
    {
        private readonly HttpClient client;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public HttpServiceClient(HttpClient client)
        {
            this.client = client;
        }

        /// <summary>
        /// Returns the client to perform HTTP operations by subclasses.
        /// </summary>
        public virtual HttpClient Http => client;

        /// <summary>
        /// Utility method to serialize properties of the provided object to a query string format.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A query string with non-blank property values of the object.</returns>
        public virtual string ToQueryString(object obj)
        {
            if (obj == null) return "";
            var queryParams = new List<KeyValuePair<string, string>>();
            foreach (var prop in obj.GetType().GetProperties())
            {
                var val = prop.GetValue(obj);
                if (val == null) continue;
                if (val is ICollection col)
                {
                    foreach (var i in col)
                        queryParams.Add(new KeyValuePair<string, string>(prop.Name, i.ToString()));
                }
                else queryParams.Add(new KeyValuePair<string, string>(prop.Name, val.ToString()));
            }
            return  string.Join("&", queryParams.Select(p => string.Concat(
                Uri.EscapeDataString(p.Key), "=", Uri.EscapeDataString(p.Value))));
        }
    }
}