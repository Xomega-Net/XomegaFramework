// Copyright (c) 2020 Xomega.Net. All rights reserved.

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using Xomega.Framework;
using Xomega.Framework.Services;

namespace Blazor1.Services.Rest
{
    /// <summary>
    /// Base class for token-based authentication controllers.
    /// </summary>
    public class TokenAuthController : BaseController
    {
        /// <summary>
        /// Injected configuration for token authentication.
        /// </summary>
        protected readonly AuthConfig config;

        /// <summary>
        /// Constructs a new TokenAuthController from the injected services and options.
        /// </summary>
        /// <param name="errorList">An error list for the current errors.</param>
        /// <param name="errorParser">An injected error parser.</param>
        /// <param name="configOptions">AuthConfig options.</param>
        public TokenAuthController(ErrorList errorList, ErrorParser errorParser, IOptionsMonitor<AuthConfig> configOptions)
            : base(errorList, errorParser)
        {
            config = configOptions.CurrentValue;
        }

        /// <summary>
        /// Creates a security token for the specified claims identity using the given token handler,
        /// and returns it serialized as a string.
        /// </summary>
        /// <param name="identity">Claims identity to create a security token from.</param>
        /// <param name="tokenHandler">Specific security token handler to use.</param>
        /// <returns>A string with the serialized security token.</returns>
        protected string GetSecurityToken(ClaimsIdentity identity, SecurityTokenHandler tokenHandler)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(config.ExpiresMin),
                SigningCredentials = config.SigningCredentials
            };
            var authToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(authToken);
        }
    }
}