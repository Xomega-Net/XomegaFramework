// Copyright (c) 2024 Xomega.Net. All rights reserved.

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Xomega.Framework.Client
{
    /// <summary>
    /// A service for managing authorization tokens for secured REST APIs.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Sets the specified authorization token to be used for secured accessing REST APIs.
        /// The service may also update the authentication state of the app using the identity constructed from the current token.
        /// Providing a null token is equivalent to logging out the current user.
        /// </summary>
        /// <param name="authToken">The authorization token to use for API access and current authentication state.</param>
        /// <returns>The claims identity constructed from the authorization token.</returns>
        ClaimsIdentity SetAuthToken(AuthToken authToken);

        /// <summary>
        /// Gets the current access token.
        /// </summary>
        /// <returns>The current access token.</returns>
        string GetAccessToken();

        /// <summary>
        /// Gets expiration date and time for the current access token, if available.
        /// </summary>
        /// <returns>The expiration date and time for the current access token, or null if no token is available.</returns>
        DateTime? GetAccessTokenExpiration();

        /// <summary>
        /// Asynchronously refreshes the current access token.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A task for the current operation.</returns>
        Task RefreshTokenAsync(CancellationToken cancellationToken);
    }
}