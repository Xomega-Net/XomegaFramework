using Microsoft.Owin.Security;
using System;
using System.IdentityModel.Tokens;

namespace AdventureWorks.Services.Rest
{
    public class AppTokenFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private string issuer;
        private byte[] key;

        public AppTokenFormat(string issuer, byte[] key)
        {
            this.issuer = issuer;
            this.key = key;
            if (key.Length != 32)
                throw new ArgumentException("Key length must be 32");
        }

        public string Protect(AuthenticationTicket data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var signingKey = new SigningCredentials(new InMemorySymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest);
            var issued = data.Properties.IssuedUtc;
            var expires = data.Properties.ExpiresUtc;

            return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(issuer, "Any", data.Identity.Claims,
                issued.Value.UtcDateTime, expires.Value.UtcDateTime, signingKey));
        }

        public AuthenticationTicket Unprotect(string protectedText)
        {
            throw new NotImplementedException();
        }
    }
}