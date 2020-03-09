// Copyright (c) 2020 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;

namespace Xomega.Framework.Blazor
{
    /// <summary>
    /// Static class that sets up default blazor security services.
    /// </summary>
    public static class SecurityServices
    {
        /// <summary>
        /// Adds default blazor security services to the specified container.
        /// </summary>
        /// <param name="services">Container to add services to.</param>
        public static void AddBlazorSecurity(this IServiceCollection services)
        {
            services.AddTransient<IPrincipalProvider, ContextPrincipalProvider>();
            services.AddScoped<SignInManager>();
        }
    }
}
