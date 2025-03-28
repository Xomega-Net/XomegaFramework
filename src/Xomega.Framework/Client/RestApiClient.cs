﻿// Copyright (c) 2025 Xomega.Net. All rights reserved.

using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Resources;
using System.Text.Json;
using System.Threading.Tasks;
using Xomega.Framework.Services;

namespace Xomega.Framework.Client
{
    /// <summary>
    /// A base class for HTTP-based service client classes that use Xomega Framework.
    /// </summary>
    public class RestApiClient
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly RestApiConfig apiConfig;
        private readonly JsonSerializerOptions serializerOptions;
        private readonly ResourceManager resourceManager;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RestApiClient(IHttpClientFactory httpClientFactory, RestApiConfig apiConfig,
            IOptionsMonitor<JsonSerializerOptions> options, ResourceManager resourceManager = null)
        {
            serializerOptions = options.CurrentValue;
            this.httpClientFactory = httpClientFactory;
            this.apiConfig = apiConfig;
            this.resourceManager = resourceManager;
        }

        /// <summary>
        /// Returns the client to perform HTTP operations by subclasses.
        /// </summary>
        protected virtual HttpClient Http => httpClientFactory.CreateClient(apiConfig.ClientName);

        /// <summary>
        /// Serializer options for HTTP operations in subclasses.
        /// </summary>
        protected virtual JsonSerializerOptions SerializerOptions => serializerOptions;


        /// <summary>
        /// Reads response content with the output. Throws an error with the status code, if content is empty.
        /// </summary>
        /// <param name="response">Service response.</param>
        /// <returns>Response content as a stream.</returns>
        public virtual async Task<string> ReadOutputContentAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (content.Length == 0)
            {
                var errorList = new ErrorList(resourceManager);
                errorList.AddError(ErrorType.System, Messages.Service_EmptyResponse, (int)response.StatusCode, response.StatusCode);
                errorList.Abort(Messages.Service_EmptyResponse);
            }
            return content;
        }

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
                else if (val is SortField[] sortFields)
                {
                    for (int i = 0; i < sortFields.Length; i++)
                    {
                        var field = sortFields[i];
                        queryParams.Add(new KeyValuePair<string, string>($"{prop.Name}[{i}].FieldName", field.FieldName));
                        queryParams.Add(new KeyValuePair<string, string>($"{prop.Name}[{i}].SortDirection", field.SortDirection.ToString()));
                    }
                }
                else if (val is ICollection col)
                {
                    foreach (var i in col)
                        queryParams.Add(new KeyValuePair<string, string>(prop.Name, i.ToString()));
                }
                else if (val.GetType().IsGenericType &&
                    val.GetType().GetGenericTypeDefinition() == typeof(FieldCriteria<>))
                {
                    var oper = val.GetType().GetProperty("Operator").GetValue(val)?.ToString();
                    if (oper != null)
                        queryParams.Add(new KeyValuePair<string, string>($"{prop.Name}.Operator", oper));
                    var vals = val.GetType().GetProperty("Values").GetValue(val);
                    if (vals is ICollection vcol)
                    {
                        foreach (var i in vcol)
                            queryParams.Add(new KeyValuePair<string, string>($"{prop.Name}.Values", i.ToString()));
                    }
                }
                else queryParams.Add(new KeyValuePair<string, string>(prop.Name, val.ToString()));
            }
            return string.Join("&", queryParams.Select(p => string.Concat(
                Uri.EscapeDataString(p.Key), "=", Uri.EscapeDataString(p.Value))));
        }
    }
}