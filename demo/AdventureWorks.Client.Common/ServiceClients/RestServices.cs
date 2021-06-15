using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xomega.Framework.Services;

namespace AdventureWorks.Services
{
    /// <summary>
    /// Static methods for configuring access to REST services
    /// </summary>
    public static class RestServices
    {
        /// <summary>
        /// Configures service container with REST services
        /// </summary>
        /// <param name="services">Service container</param>
        /// <returns>Configured service container</returns>
        public static IServiceCollection AddRestServices(this IServiceCollection services, string apiBaseAddress)
        {
            services.Configure<JsonSerializerOptions>(o => {
                o.IgnoreNullValues = true;
                o.PropertyNameCaseInsensitive = true;
            });
            services.AddSingleton(new HttpClient
            {
                BaseAddress = new Uri(apiBaseAddress)
            });
            return services;
        }

        /// <summary>
        /// Authenticates with REST services using user name/password
        /// and receives a JWT token for further communication with the services.
        /// </summary>
        /// <param name="user">User name</param>
        /// <param name="password">Password</param>
        public async static Task<ClaimsPrincipal> Authenticate(IServiceProvider serviceProvider, string user, string password)
        {
            var credentials = new
            {
                Username = user,
                Password = password
            };
            var httpClient = serviceProvider.GetRequiredService<HttpClient>();
            var options = serviceProvider.GetService<IOptionsMonitor<JsonSerializerOptions>>();
            using (var resp = await httpClient.PostAsync("authentication", new StringContent(
                JsonSerializer.Serialize(credentials), Encoding.UTF8, "application/json")))
            {
                var content = await resp.Content.ReadAsStringAsync();
                var res = JsonSerializer.Deserialize<Output<string>>(content, options?.CurrentValue);
                res.Messages.AbortIfHasErrors();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {res.Result}");
                var jwtTokenHandler = new JwtSecurityTokenHandler();
                var token = jwtTokenHandler.ReadJwtToken(res.Result);
                var claims = token.Claims.Select(c => new Claim(jwtTokenHandler.InboundClaimTypeMap.ContainsKey(c.Type) ?
                    jwtTokenHandler.InboundClaimTypeMap[c.Type] : c.Type, c.Value, c.ValueType, c.Issuer, c.OriginalIssuer));
                return new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
            }
        }
    }
}