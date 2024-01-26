// Copyright (c) 2023 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using Xomega.Framework.Client;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Base class for JWT token-based authentication controllers.
    /// </summary>
    /// <param name="errorList">An error list for the current errors.</param>
    /// <param name="errorParser">An injected error parser.</param>
    /// <param name="jwtOptMon">JWT options.</param>
    public class JwtAuthController(IOptionsMonitor<JwtBearerOptions> jwtOptMon,
        ErrorList errorList, ErrorParser errorParser) : BaseController(errorList, errorParser)
    {
        /// <summary>
        /// Injected configuration for token authentication.
        /// </summary>
        protected readonly JwtBearerOptions jwtOpt = jwtOptMon.Get(JwtBearerDefaults.AuthenticationScheme);

        /// <summary>
        /// Generates a JWT-base auth token for the given claims identity and a refresh token.
        /// </summary>
        /// <param name="identity">The claims identity for the access token.</param>
        /// <param name="refreshToken">Refresh token to use.</param>
        /// <param name="expireMinutes">The number of minutes for the token expiration. Default is 15 minutes.</param>
        /// <returns>The generated AuthToken.</returns>
        /// <exception cref="Exception">Thrown when no JWT signing keys are configured in JwtBearerOptions.</exception>
        protected AuthToken GenerateAuthToken(ClaimsIdentity identity, string refreshToken, int expireMinutes = 15)
        {
            var issuer = jwtOpt.TokenValidationParameters.ValidIssuers.FirstOrDefault();
            var audience = jwtOpt.TokenValidationParameters.ValidAudiences.FirstOrDefault();
            var signingKey = jwtOpt.TokenValidationParameters.IssuerSigningKeys.FirstOrDefault() ??
                throw new Exception("No JWT signing keys configured for valid issuers.");
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            // generate jwt token
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Issuer = issuer,
                Audience = audience,
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                SigningCredentials = signingCredentials
            };
            var secToken = jwtTokenHandler.CreateToken(tokenDescriptor);
            AuthToken authToken = new()
            {
                AccessToken = jwtTokenHandler.WriteToken(secToken),
                RefreshToken = refreshToken
            };
            return authToken;
        }

        /// <summary>
        /// Validates the provided expired JWT token using validation parameters 
        /// from the current JWT options, excluding the token lifetime.
        /// </summary>
        /// <param name="jwtToken">The JWT token to validate.</param>
        /// <returns>Claims identity constructed from the JWT token.</returns>
        protected ClaimsIdentity ValidateExpiredToken(string jwtToken)
        {
            var baseValidationParameters = jwtOpt.TokenValidationParameters;
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = baseValidationParameters.ValidateAudience,
                ValidAudiences = baseValidationParameters.ValidAudiences,
                ValidateIssuer = baseValidationParameters.ValidateIssuer,
                ValidIssuers = baseValidationParameters.ValidIssuers,
                ValidateIssuerSigningKey = baseValidationParameters.ValidateIssuerSigningKey,
                IssuerSigningKeys = baseValidationParameters.IssuerSigningKeys,
                ValidateLifetime = false // don't validate the token's expiration date
            };

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var principal = jwtTokenHandler.ValidateToken(jwtToken, tokenValidationParameters, out SecurityToken secToken);
            if (secToken is not JwtSecurityToken jwtSecToken || 
                !jwtSecToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    return null;

            return principal?.Identity as ClaimsIdentity;
        }

        /// <summary>
        /// Generates a string for a new random refresh token of a certain size.
        /// </summary>
        /// <param name="size">The number of bytes to use for refresh token generation.</param>
        /// <returns>Base64-encoded refresh token.</returns>
        protected string GenerateRefreshToken(int size = 32)
        {
            using var rng = RandomNumberGenerator.Create();
            var tokenBytes = new byte[size];
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes);
        }
    }
}