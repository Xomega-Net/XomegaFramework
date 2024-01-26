// Copyright (c) 2024 Xomega.Net. All rights reserved.

using Microsoft.Extensions.Options;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xomega.Framework.Services;

namespace Xomega.Framework.Client
{
    /// <summary>
    /// Implementation of <see cref="ITokenService"/> that works with JWT tokens.
    /// </summary>
    public class JwtTokenService : ITokenService
    {
        private readonly IPrincipalProvider principalProvider;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly RestApiConfig apiConfig;
        private readonly JsonSerializerOptions serializerOptions;

        private AuthToken authToken;
        private DateTime? validTo;

        /// <summary>
        /// Constructs JwtTokenService using injected services and options.
        /// </summary>
        /// <param name="principalProvider">Injected principal provider.</param>
        /// <param name="httpClientFactory">Injected HTTP client factory.</param>
        /// <param name="apiConfig">Injected REST API config.</param>
        /// <param name="serializerOptions">Injected JSON serializer options.</param>
        public JwtTokenService(IPrincipalProvider principalProvider, IHttpClientFactory httpClientFactory,
            RestApiConfig apiConfig, IOptionsMonitor<JsonSerializerOptions> serializerOptions)
        {
            this.principalProvider = principalProvider;
            this.httpClientFactory = httpClientFactory;
            this.apiConfig = apiConfig;
            this.serializerOptions = serializerOptions.CurrentValue;
        }

        /// <inheritdoc/>
        public virtual string GetAccessToken() => authToken?.AccessToken;

        /// <inheritdoc/>
        public virtual DateTime? GetAccessTokenExpiration() => validTo;

        /// <summary>
        /// Refreshes expired access token by calling a REST endpoint specified by the 
        /// RefreshTokenPath in the injected REST API config.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A task for the current operation.</returns>
        public virtual async Task RefreshTokenAsync(CancellationToken cancellationToken)
        {
            HttpClient Http = httpClientFactory.CreateClient(apiConfig.ClientName);
            using (var resp = await Http.PostAsync(apiConfig.RefreshTokenPath, new StringContent(
                JsonSerializer.Serialize(authToken, serializerOptions), Encoding.UTF8, "application/json"), cancellationToken))
            {
                var content = await resp.Content.ReadAsStreamAsync();
                var res = await JsonSerializer.DeserializeAsync<Output<AuthToken>>(content, serializerOptions, cancellationToken);

                if (!res.Messages.HasErrors())
                    SetAuthToken(res.Result);
            }
        }

        /// <inheritdoc/>
        public virtual ClaimsIdentity SetAuthToken(AuthToken authToken)
        {
            this.authToken = authToken;

            if (authToken == null)
            {
                validTo = null;
                principalProvider.CurrentPrincipal = null;
                RedirectToLogin();
            }
            else
            {
                var jwtTokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtTokenHandler.ReadJwtToken(authToken.AccessToken);
                validTo = jwtToken.ValidTo;

                var claims = jwtToken.Claims.Select(c => new Claim(jwtTokenHandler.InboundClaimTypeMap.ContainsKey(c.Type) ?
                    jwtTokenHandler.InboundClaimTypeMap[c.Type] : c.Type, c.Value, c.ValueType, c.Issuer, c.OriginalIssuer));
                var ci = new ClaimsIdentity(claims, "Bearer");
                principalProvider.CurrentPrincipal = new ClaimsPrincipal(ci);
                return ci;
            }
            return null;
        }

        /// <summary>
        /// Subclasses can override this method to redirect to the login page when the auth token is cleared.
        /// </summary>
        /// <exception cref="Exception">Thrown by default to be handled by the app code.</exception>
        protected virtual void RedirectToLogin() => throw new Exception("Your session has expired. Please log in again.");
    }
}
