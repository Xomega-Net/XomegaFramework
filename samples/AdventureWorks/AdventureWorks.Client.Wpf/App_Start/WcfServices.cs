using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.ServiceModel.Security;
using System.Threading;
using Xomega.Framework;
using Xomega.Framework.Wcf;

namespace AdventureWorks.Client.Wpf
{
    /// <summary>
    /// Static methods for configuring access to WCF services
    /// </summary>
    public static class WcfServices
    {
        /// <summary>
        /// Audience URI for the current application, which should be configured and accepted by the WCF services
        /// </summary>
        public const string AudienceUri = "http://Client.Wpf";

        /// <summary>
        /// Security token for communcation with WCF services
        /// </summary>
        private static SecurityToken IssuedToken { get; set; }

        /// <summary>
        /// Configures service container with WCF services and error parser
        /// </summary>
        /// <param name="services">Service container</param>
        /// <returns>Configured service container</returns>
        public static IServiceCollection AddWcfServices(this IServiceCollection services)
        {
            services.AddWcfErrorParser();
            services.AddWcfClientServices(() => IssuedToken, null, null);
            return services;
        }

        /// <summary>
        /// Authenticates with WCF services using user name/password
        /// and receives a token for further communication with the services.
        /// </summary>
        /// <param name="user">User name</param>
        /// <param name="password">Password</param>
        public static void Authenticate(string user, string password)
        {
            try
            {
                var factory = new WSTrustChannelFactory("sts message");
                factory.Credentials.UserName.UserName = user;
                factory.Credentials.UserName.Password = password;
                var channel = factory.CreateChannel();
                IssuedToken = channel.Issue(new RequestSecurityToken(RequestTypes.Issue, KeyTypes.Bearer)
                {
                    AppliesTo = new EndpointReference(AudienceUri)
                });

                var identities = TokenUtils.GetIdentitiesFromSamlToken(IssuedToken, AudienceUri, true);
                Thread.CurrentPrincipal = new ClaimsPrincipal(identities);
            }
            catch (MessageSecurityException)
            {
                ErrorList errors = new ErrorList();
                errors.AddError(ErrorType.Security, "Invalid credentials");
                errors.Abort(errors.ErrorsText);
            }
        }
    }
}