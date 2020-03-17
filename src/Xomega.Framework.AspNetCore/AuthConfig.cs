// Copyright (c) 2020 Xomega.Net. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Xomega.Framework.Services
{
    public class AuthConfig
    {
        public string SigningKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpiresMin { get; set; }

        public SymmetricSecurityKey SymmetricSigningKey => new SymmetricSecurityKey(Encoding.ASCII.GetBytes((SigningKey)));

        public SigningCredentials SigningCredentials => new SigningCredentials(SymmetricSigningKey, SecurityAlgorithms.HmacSha256);

        public TokenValidationParameters ValidationParameters => new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = SymmetricSigningKey,
            ValidIssuer = Issuer,
            ValidateIssuer = false,
            ValidAudience = Audience,
            ValidateAudience = false
        };
    }

    public static class AuthConfigRegistration
    {
        public static AuthConfig AddAuthConfig(this IServiceCollection services, IConfiguration configuration,
            string authConfigSection = nameof(AuthConfig))
        {
            var jwtConfig = configuration.GetSection(authConfigSection);
            services.Configure<AuthConfig>(jwtConfig);
            return jwtConfig.Get<AuthConfig>();
        }
    }
}