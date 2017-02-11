// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.IdentityModel.Configuration;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Xml;

namespace Xomega.Framework
{
    /// <summary>
    /// Static utility methods for working with security tokens
    /// </summary>
    public static class TokenUtils
    {
        /// <summary>
        /// Gets claims identities from the specified SAML token as a GenericXmlSecurityToken
        /// </summary>
        /// <param name="token">SAML token to get identities from</param>
        /// <param name="audienceUri">Audience URI used to obtain the token</param>
        /// <param name="trustIssuer">True to automatically trust the issuer.
        /// False to validate the issuer against the app configuration</param>
        /// <returns>A collection of claims identities from the SAML token.</returns>
        public static IEnumerable<ClaimsIdentity> GetIdentitiesFromSamlToken(SecurityToken token, string audienceUri, bool trustIssuer)
        {
            SamlSecurityTokenHandler handler = new SamlSecurityTokenHandler();
            handler.Configuration = new SecurityTokenHandlerConfiguration();
            SamlSecurityToken samlToken = token as SamlSecurityToken;

            if (samlToken == null && token is GenericXmlSecurityToken)
                samlToken = handler.ReadToken(new XmlNodeReader(((GenericXmlSecurityToken)token).TokenXml)) as SamlSecurityToken;

            if (samlToken == null) throw new ArgumentException("The token must be a SAML token or a generic XML SAML token");

            handler.SamlSecurityTokenRequirement.CertificateValidator = X509CertificateValidator.None;
            handler.Configuration.AudienceRestriction.AllowedAudienceUris.Add(new Uri(audienceUri));
            if (trustIssuer)
            {
                // configure to auto-trust the issuer
                ConfigurationBasedIssuerNameRegistry issuers = handler.Configuration.IssuerNameRegistry as ConfigurationBasedIssuerNameRegistry;
                issuers.AddTrustedIssuer(((X509SecurityToken)samlToken.Assertion.SigningToken).Certificate.Thumbprint, "sts");
            }
            else handler.Configuration.IssuerNameRegistry.LoadCustomConfiguration(
                SystemIdentityModelSection.DefaultIdentityConfigurationElement.IssuerNameRegistry.ChildNodes);
            return handler.ValidateToken(samlToken);
        }
    }
}
