using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using Xomega.Framework;

namespace AdventureWorks.Services.Wcf
{
    public class UserNameValidator : UserNameSecurityTokenHandler
    {
        public override bool CanValidateToken { get { return true; } }

        public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            var userNameToken = token as UserNameSecurityToken;
            if (userNameToken == null)
                throw new SecurityTokenException("The security token is not a valid username security token.");

            if (DI.DefaultServiceProvider == null)
                throw new InvalidOperationException("Default service provider is not initialized.");

            try
            {
                // TODO: validate UserName and Password from userNameToken here. 
                // Use DI.DefaultServiceProvider to access any services for that.
                IPersonService svc = DI.DefaultServiceProvider.GetService<IPersonService>();
                svc.Authenticate(new Credentials()
                {
                    Email = userNameToken.UserName,
                    Password = userNameToken.Password
                });

                ClaimsIdentity identity = new ClaimsIdentity(AuthenticationTypes.Password);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userNameToken.UserName));
                identity.AddClaim(new Claim(ClaimTypes.Name, userNameToken.UserName));
                return Array.AsReadOnly(new[] { identity });
            }
            catch (Exception ex)
            {
                ErrorParser errorParser = DI.DefaultServiceProvider.GetService<ErrorParser>();
                ErrorList errors = errorParser.FromException(ex);
                throw new SecurityTokenException(errors.ErrorsText, ex);
            }
        }
    }
}