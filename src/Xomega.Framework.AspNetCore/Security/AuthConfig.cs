// Copyright (c) 2022 Xomega.Net. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Configuration for token-based authentication
    /// </summary>
    public class AuthConfig
    {
        /// <summary>
        /// A string containing a secret key for signing tokens.
        /// </summary>
        public string SigningKey { get; set; }

        /// <summary>
        /// Token issuer.
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Intended audience for the token.
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// The number of minutes for the token to expire after creation.
        /// </summary>
        public int ExpiresMin { get; set; }

        /// <summary>
        /// Token signing key built from the bytes of the secret key.
        /// </summary>
        public SymmetricSecurityKey SymmetricSigningKey => new SymmetricSecurityKey(Encoding.ASCII.GetBytes((SigningKey)));

        /// <summary>
        /// Signing credentials for the token.
        /// </summary>
        public SigningCredentials SigningCredentials => new SigningCredentials(SymmetricSigningKey, SecurityAlgorithms.HmacSha256);

        /// <summary>
        /// Default token validation parameters based on the current configuration.
        /// </summary>
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

    /// <summary>
    /// Extension method for registering AuthConfig options with the service container.
    /// </summary>
    public static class AuthConfigRegistration
    {
        /// <summary>
        /// Adds AuthConfig options to the service container from the given configuration section,
        /// and returns an instance of the options object.
        /// </summary>
        /// <param name="services">The service collection to register the options with.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="authConfigSection">The name of the AuthConfig section in the app configuration.</param>
        /// <returns></returns>
        public static AuthConfig AddAuthConfig(this IServiceCollection services, IConfiguration configuration,
            string authConfigSection = nameof(AuthConfig))
        {
            var jwtConfig = configuration.GetSection(authConfigSection);
            services.Configure<AuthConfig>(jwtConfig);
            return jwtConfig.Get<AuthConfig>();
        }
    }
}