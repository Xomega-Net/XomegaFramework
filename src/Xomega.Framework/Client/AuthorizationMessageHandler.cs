// Copyright (c) 2025 Xomega.Net. All rights reserved.

using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System;

namespace Xomega.Framework.Client
{
    /// <summary>
    /// Delegating HTTP message handler for REST API calls secured with JWT bearer tokens.
    /// It works with an injected token service to get an access token and set it in the Authorization header.
    /// It tries to refresh expired tokens before making a REST call and clears the current auth token
    /// if the call resulted in 401 (NotAuthorized) response, which may redirect the user to the login page.
    /// </summary>
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        private readonly ITokenService tokenService;
        private readonly RestApiConfig apiConfig;

        /// <summary>
        /// Constructs a new AuthorizationMessageHandler using injected token service and REST API config.
        /// </summary>
        /// <param name="tokenService">Token service to provide or refresh access tokens.</param>
        /// <param name="apiConfig">REST API config to check the path to the endpoint for refreshing access tokens.</param>
        public AuthorizationMessageHandler(ITokenService tokenService, RestApiConfig apiConfig)
        {
            this.tokenService = tokenService;
            this.apiConfig = apiConfig;
        }

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool isTokenRefresh = !string.IsNullOrEmpty(apiConfig.RefreshTokenPath) &&
                request.RequestUri.LocalPath.TrimStart('/').StartsWith(apiConfig.RefreshTokenPath.TrimStart('/'));

            if (!isTokenRefresh)
            { // make sure we don't call token refresh while actually refreshing the token
                var tokenExpiration = tokenService.GetAccessTokenExpiration();
                var now = DateTime.UtcNow;
                if (tokenExpiration != null && tokenExpiration < now)
                    await tokenService.RefreshTokenAsync(cancellationToken);

                string token = tokenService.GetAccessToken();
                if (token != null)
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                await tokenService.SetAuthTokenAsync(null);

            return response;
        }
    }
}
