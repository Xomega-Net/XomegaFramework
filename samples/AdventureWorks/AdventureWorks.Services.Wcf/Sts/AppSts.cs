using Microsoft.Extensions.DependencyInjection;
using System;
using System.IdentityModel;
using System.IdentityModel.Configuration;
using System.IdentityModel.Protocols.WSTrust;
using System.Security.Claims;
using Xomega.Framework;

namespace AdventureWorks.Services.Wcf
{
    public class AppSts : SecurityTokenService
    {
        public AppSts(SecurityTokenServiceConfiguration config) : base(config)
        {
        }

        protected override ClaimsIdentity GetOutputClaimsIdentity(ClaimsPrincipal principal, RequestSecurityToken request, Scope scope)
        {
            if (principal == null) throw new InvalidRequestException("The caller's principal is null.");
            AppStsConfig cfg = SecurityTokenServiceConfiguration as AppStsConfig;
            if (cfg == null) throw new InvalidOperationException("SecurityTokenServiceConfiguration should be AppStsConfig");
            if (!principal.Identity.IsAuthenticated || principal.Identity.Name == null)
                throw new UnauthorizedAccessException("User is not authorized.");

            try
            {
                // TODO: construct user identity here. Use cfg.ServiceProvider to access any services
                //ClaimsIdentity ci = new ClaimsIdentity(principal.Identity.AuthenticationType);
                //ci.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.Identity.Name));
                //ci.AddClaim(new Claim(ClaimTypes.Name, principal.Identity.Name));
                //return ci;
                IPersonService svc = cfg.ServiceProvider.GetService<IPersonService>();
                PersonInfo info = svc.Read(principal.Identity.Name);
                return SecurityManager.CreateIdentity(principal.Identity.AuthenticationType, info);
            }
            catch (Exception ex)
            {
                ErrorParser errorParser = cfg.ServiceProvider.GetService<ErrorParser>();
                ErrorList errors = errorParser.FromException(ex);
                throw new RequestFailedException(errors.ErrorsText, ex);
            }
        }

        protected override Scope GetScope(ClaimsPrincipal principal, RequestSecurityToken request)
        {
            if (request.AppliesTo == null) throw new InvalidRequestException("The AppliesTo is null.");

            Scope scope = new Scope(request.AppliesTo.Uri.OriginalString, SecurityTokenServiceConfiguration.SigningCredentials);
            scope.TokenEncryptionRequired = false;
            scope.SymmetricKeyEncryptionRequired = false;
            scope.ReplyToAddress = (string.IsNullOrEmpty(request.ReplyTo)) ? scope.AppliesToAddress : request.ReplyTo;

            return scope;
        }
    }
}