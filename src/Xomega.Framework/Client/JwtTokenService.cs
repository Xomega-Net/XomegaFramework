// Copyright (c) 2025 Xomega.Net. All rights reserved.

using Microsoft.Extensions.Options;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Resources;
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
        private readonly ResourceManager resourceManager;
        private readonly JsonSerializerOptions serializerOptions;

        private AuthToken authToken;
        private DateTime? validTo;

        /// <summary>
        /// The current token service error
        /// </summary>
        protected string currentError;

        /// <summary>
        /// Constructs JwtTokenService using injected services and options.
        /// </summary>
        /// <param name="principalProvider">Injected principal provider.</param>
        /// <param name="httpClientFactory">Injected HTTP client factory.</param>
        /// <param name="apiConfig">Injected REST API config.</param>
        /// <param name="serializerOptions">Injected JSON serializer options.</param>
        /// <param name="resourceManager">Resource manager for message localization.</param>
        public JwtTokenService(IPrincipalProvider principalProvider, IHttpClientFactory httpClientFactory,
            RestApiConfig apiConfig, IOptionsMonitor<JsonSerializerOptions> serializerOptions, ResourceManager resourceManager)
        {
            this.principalProvider = principalProvider;
            this.httpClientFactory = httpClientFactory;
            this.apiConfig = apiConfig;
            this.resourceManager = resourceManager;
            this.serializerOptions = serializerOptions.CurrentValue;
        }

        /// <inheritdoc/>
        public virtual string GetAccessToken() => authToken?.AccessToken;

        /// <inheritdoc/>
        public virtual DateTime? GetAccessTokenExpiration() => validTo;

        /// <inheritdoc/>
        public virtual string GetCurrentError() => currentError;

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
                    await SetAuthTokenAsync(res.Result);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<ClaimsIdentity> SetAuthTokenAsync(AuthToken authToken)
        {
            this.authToken = authToken;

            if (authToken == null)
            {
                validTo = null;
                principalProvider.CurrentPrincipal = null;
                await RedirectToLogin();
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
                currentError = null;
                return ci;
            }
            return null;
        }

        /// <summary>
        /// Subclasses can override this method to redirect to the login page when the auth token is cleared.
        /// </summary>
        /// <exception cref="Exception">Thrown by default to be handled by the app code.</exception>
        protected virtual Task RedirectToLogin()
        {
            currentError = Messages.Login_SessionExpired;
            return Task.CompletedTask;
        }
    }
}
