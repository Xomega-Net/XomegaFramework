﻿// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System;
using System.IdentityModel.Configuration;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Security;

namespace Xomega.Framework.Wcf
{
    /// <summary>
    /// A service host factory based on pre-initalized DI.DefaultServiceProvider in Xomega Framework.
    /// </summary>
    public class DefaultServiceProviderHostFactory : ServiceHostFactory
    {
        /// <summary>
        /// Constructs a new DefaultServiceProviderHostFactory and ensures the app is initalized
        /// </summary>
        public DefaultServiceProviderHostFactory()
        {
            AppInitializer.EnsureInitialized();
        }

        /// <summary>
        /// Creates a System.ServiceModel.ServiceHost for a specified type of service with a specific base address.
        /// </summary>
        /// <param name="serviceType">Specifies the type of service to host.</param>
        /// <param name="baseAddresses">The System.Array of type System.Uri that contains the base addresses for the service hosted.</param>
        /// <returns></returns>
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            IServiceProvider sp = DI.DefaultServiceProvider;
            Type stsc = typeof(SecurityTokenServiceConfiguration);
            if (stsc.IsAssignableFrom(serviceType))
            {
                // try resolving concrete or base STS configuration first to enable DI for it
                SecurityTokenServiceConfiguration stsConfig = sp.GetService(serviceType) as SecurityTokenServiceConfiguration;
                if (stsConfig == null) stsConfig = sp.GetService(stsc) as SecurityTokenServiceConfiguration;
                if (stsConfig == null) stsConfig = Activator.CreateInstance(serviceType) as SecurityTokenServiceConfiguration;
                ServiceHost host = new WSTrustServiceHost(stsConfig, baseAddresses);
                if (stsConfig.ServiceCertificate != null)
                    host.Credentials.ServiceCertificate.Certificate = stsConfig.ServiceCertificate;
                return host;
            }
            return new ServiceProviderHost(sp, serviceType, baseAddresses);
        }
    }
}
