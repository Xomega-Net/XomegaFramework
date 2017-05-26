using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xomega.Framework;

namespace AdventureWorks.Services.Rest
{
    public class AppAuthProvider : OAuthAuthorizationServerProvider
    {
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // validate client id here
            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            if (DI.DefaultServiceProvider == null)
                throw new InvalidOperationException("Default service provider is not initialized.");

            try
            {
                // TODO: validate context.UserName and context.Password here.
                // Use DI.DefaultServiceProvider to access any services.
                //string user = string.IsNullOrEmpty(context.UserName) ? "Guest" : context.UserName;
                //ClaimsIdentity guestIdentity = new ClaimsIdentity();
                //guestIdentity.AddClaim(new Claim(ClaimTypes.Name, user));
                //context.Validated(new AuthenticationTicket(guestIdentity, new AuthenticationProperties()));

                using (IServiceScope scope = DI.DefaultServiceProvider.CreateScope())
                {
                    IPersonService svc = scope.ServiceProvider.GetService<IPersonService>();
                    svc.Authenticate(new Credentials()
                    {
                        Email = context.UserName,
                        Password = context.Password
                    });
                    PersonInfo info = svc.Read(context.UserName);
                    ClaimsIdentity identity = SecurityManager.CreateIdentity(AuthenticationTypes.Password, info);
                    context.Validated(new AuthenticationTicket(identity, new AuthenticationProperties()));
                }
            }
            catch (Exception ex)
            {
                ErrorParser errorParser = DI.DefaultServiceProvider.GetService<ErrorParser>();
                ErrorList errors = errorParser.FromException(ex);
                context.SetError("invalid_grant", errors.ErrorsText);
            }
            return Task.FromResult<object>(null);
        }
    }
}