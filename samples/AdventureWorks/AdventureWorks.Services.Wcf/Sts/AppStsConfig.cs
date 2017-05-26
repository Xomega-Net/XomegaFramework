using System;
using System.IdentityModel.Configuration;
using System.IdentityModel.Tokens;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace AdventureWorks.Services.Wcf
{
    public class AppStsConfig : SecurityTokenServiceConfiguration
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public AppStsConfig(IServiceProvider svcProvider)
        {
            ServiceProvider = svcProvider;
            // read local certificate for ease of development
            string signingCertificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sts/LocalSTS.pfx");
            X509Certificate2 signignCert = new X509Certificate2(signingCertificatePath, "LocalSTS", X509KeyStorageFlags.PersistKeySet);
            // TODO: read certificate from a store in production instead as follows
            //X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            //store.Open(OpenFlags.ReadOnly);
            //var certs = store.Certificates.Find(X509FindType.FindBySubjectName, "localhost", false);
            //store.Close();
            //if (certs.Count > 0) signignCert = certs[0];
            SigningCredentials = new X509SigningCredentials(signignCert);
            ServiceCertificate = signignCert;
            TokenIssuerName = "AdventureWorks/STS";
            SecurityTokenService = typeof(AppSts);
        }
    }
}