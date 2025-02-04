// Copyright (c) 2025 Xomega.Net. All rights reserved.

namespace Xomega.Framework.Client
{
    /// <summary>
    /// Configuration for accessing REST APIs from a client.
    /// </summary>
    public class RestApiConfig
    {
        /// <summary>
        /// The name of a configured HTTP client used to make REST calls.
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Base address for the REST API.
        /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        /// Base path for the REST API that prefixes all other endpoints.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// True if REST API uses authorization headers.
        /// </summary>
        public bool Authorization { get; set; }

        /// <summary>
        /// The path to refresh access tokens.
        /// </summary>
        public string RefreshTokenPath { get; set; }
    }
}
