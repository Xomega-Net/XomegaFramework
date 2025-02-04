// Copyright (c) 2025 Xomega.Net. All rights reserved.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

namespace Xomega.Framework.Client
{
    /// <summary>
    /// An HTTP message handler that adds a specific prefix to all outgoing HTTP requests.
    /// </summary>
    public class PathPrefixMessageHandler : DelegatingHandler
    {
        private readonly string pathPrefix;

        /// <summary>
        /// Constructs a PathPrefixMessageHandler with a specific path prefix.
        /// </summary>
        /// <param name="pathPrefix">The prefix to use for all API calls.</param>
        public PathPrefixMessageHandler(string pathPrefix)
        {
            this.pathPrefix = pathPrefix ?? string.Empty;
        }

        /// <inheritdoc/>
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestPath = request.RequestUri?.LocalPath ?? string.Empty;
            if (!requestPath.StartsWith(pathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var baseUrl = request.RequestUri.GetLeftPart(UriPartial.Authority);
                var prefixedPath = $"{pathPrefix.Trim('/')}/{request.RequestUri.PathAndQuery.TrimStart('/')}";
                request.RequestUri = new Uri(new Uri(baseUrl), new Uri(prefixedPath, UriKind.Relative));
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}
