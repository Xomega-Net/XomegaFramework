// Copyright (c) 2023 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// A service that handles sign-in for blazor apps, allowing for a blazor-based login view.
    /// It provides a workaround for blazor issues with cookie-based authentication,
    /// caused by the inability to modify response headers in SignalR hosted blazor server apps.
    /// The service allows to encrypt the authentication ticket for the logged in user,
    /// and pass it to an intermediary full-fledged SignIn razor page,
    /// which would then use it to properly sign the user in.
    /// </summary>
    public class SignInManager
    {
        private readonly NavigationManager Navigation;
        private readonly IHttpContextAccessor ContextAccessor;
        private readonly IDataProtectionProvider DataProtection;

        /// <summary>
        /// A string used as a purpose in data protection services to encrypt/decrypt the authentication ticket.
        /// Subclasses can set it to a different value.
        /// </summary>
        protected string SignInTicketProtectionPurpose = "Blazor.Auth.Ticket";

        /// <summary>
        /// The name of a URL parameter that holds the redirect URI.
        /// Subclasses can set it to a different value.
        /// </summary>
        protected string RedirectUriParam = "redirectUri";

        /// <summary>
        /// Constructs a sign-in manager service with the necessary dependent services.
        /// </summary>
        /// <param name="navigation">Injected navigation service.</param>
        /// <param name="contextAccessor">Injected HTTP context accessor service.</param>
        /// <param name="dataProtection">Injected data protection service.</param>
        public SignInManager(NavigationManager navigation, IHttpContextAccessor contextAccessor,
            IDataProtectionProvider dataProtection)
        {
            Navigation = navigation;
            ContextAccessor = contextAccessor;
            DataProtection = dataProtection;
        }

        /// <summary>
        /// Checks if the current user is authenticated, and if not, then redirects to the specified login URI.
        /// </summary>
        /// <param name="loginUri">Login URI to redirect unauthenticated users to.</param>
        public void Challenge(string loginUri)
        {
            if (!ContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                var challengeUri = QueryHelpers.AddQueryString(loginUri, RedirectUriParam, Navigation.Uri);
                Navigation.NavigateTo(challengeUri, true);
            }
        }

        /// <summary>
        /// Gets an encrypted authentication ticket for the specified principal that expires in the specified number of seconds.
        /// </summary>
        /// <param name="principal">The authenticated claims principal.</param>
        /// <param name="expiresSeconds">The number of seconds for the ticket expiration. Defaults to 1 min, but cannot exceed 5 minutes.</param>
        /// <returns>The encrypted authentication ticket for the specified principal.</returns>
        public string GetSignInTicket(ClaimsPrincipal principal, int expiresSeconds = 60)
        {
            var props = new AuthenticationProperties
            {
                ExpiresUtc = DateTime.Now.AddSeconds(Math.Min(expiresSeconds, 300))
            };
            var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue(RedirectUriParam, out var param))
            {
                props.RedirectUri = param.First();
            }
            var ticket = new AuthenticationTicket(principal, props, principal.Identity.AuthenticationType);
            var dataProtector = DataProtection.CreateProtector(SignInTicketProtectionPurpose);
            var ticketDataFormat = new TicketDataFormat(dataProtector);

            string protectedTicket = ticketDataFormat.Protect(ticket);
            return protectedTicket;
        }

        /// <summary>
        /// Asynchronously signs in the user from the provided authentication ticket.
        /// This method should not be called from any blazor component,
        /// but rather from a full Razor page that can update the cookies in the response.
        /// </summary>
        /// <param name="ticket">Authentication ticket created by this service.</param>
        /// <returns>A task for this function.</returns>
        public async Task SignInAsync(string ticket)
        {
            var dataProtector = DataProtection.CreateProtector(SignInTicketProtectionPurpose);
            var ticketDataFormat = new TicketDataFormat(dataProtector);
            var authTicket = ticketDataFormat.Unprotect(ticket);
            if (authTicket.Properties.ExpiresUtc != null && authTicket.Properties.ExpiresUtc.Value > DateTime.Now)
                await ContextAccessor.HttpContext.SignInAsync(authTicket.Principal);

            if (authTicket.Properties.RedirectUri != null)
                ContextAccessor.HttpContext.Response.Redirect(authTicket.Properties.RedirectUri);
            else ContextAccessor.HttpContext.Response.Redirect("/");
        }

        /// <summary>
        /// Asynchronously signs out the user and redirects to the specified URI.
        /// This method should not be called from any blazor component,
        /// but rather from a full Razor page that can update the cookies in the response.
        /// </summary>
        /// <param name="redirectUri">The URI to redirect the user to.</param>
        /// <returns>A task for this function.</returns>
        public async Task SignOutAsync(string redirectUri)
        {
            await ContextAccessor.HttpContext.SignOutAsync();

            if (redirectUri != null)
                ContextAccessor.HttpContext.Response.Redirect(redirectUri);
            else ContextAccessor.HttpContext.Response.Redirect("/");
        }
    }
}
