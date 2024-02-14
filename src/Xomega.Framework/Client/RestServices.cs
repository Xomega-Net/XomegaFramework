// Copyright (c) 2024 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xomega.Framework.Client
{

    /// <summary>
    /// Static methods for configuring access to REST services
    /// </summary>
    public static class RestServices
    {
        /// <summary>
        /// Configures service container with services for calling a REST API, such as the HTTP client,
        /// additional message handlers, token service and default serialization options.
        /// </summary>
        /// <param name="services">Service container.</param>
        /// <param name="apiConfig">Config for the REST API.</param>
        /// <returns>Configured service container</returns>
        public static IServiceCollection AddRestServices(this IServiceCollection services, RestApiConfig apiConfig)
        {
            services.Configure<JsonSerializerOptions>(o =>
            {
                o.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                o.PropertyNameCaseInsensitive = true;
            });
            var b = services.AddHttpClient(apiConfig.ClientName, client =>
            {
                client.BaseAddress = new Uri(apiConfig.BaseAddress);
            });
            if (apiConfig.Authorization)
            {
                services.TryAddSingleton<ITokenService, JwtTokenService>();
                services.TryAddScoped<AuthorizationMessageHandler>();
                b.AddHttpMessageHandler<AuthorizationMessageHandler>();
            }
            if (!string.IsNullOrEmpty(apiConfig.BasePath))
            {
                b.AddHttpMessageHandler(() => new PathPrefixMessageHandler(apiConfig.BasePath));
            }
            return services;
        }
    }
}