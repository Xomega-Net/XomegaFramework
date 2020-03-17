// Copyright (c) 2020 Xomega.Net. All rights reserved.

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using Xomega.Framework;
using Xomega.Framework.Services;

namespace Blazor1.Services.Rest
{
    public class TokenAuthController : BaseController
    {
        protected readonly AuthConfig config;

        public TokenAuthController(ErrorList errorList, ErrorParser errorParser, IOptionsMonitor<AuthConfig> configOptions) : base(errorList, errorParser)
        {
            config = configOptions.CurrentValue;
        }

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
