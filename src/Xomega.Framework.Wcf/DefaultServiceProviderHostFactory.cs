// Copyright (c) 2010-2016 Xomega.Net. All rights reserved.

using System;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Xomega.Framework.Wcf
{
    /// <summary>
    /// A service host factory based on pre-initalized DI.DefaultServiceProvider in Xomega Framework.
    /// </summary>
    public class DefaultServiceProviderHostFactory : ServiceHostFactory
    {
        /// <summary>
        /// Creates a System.ServiceModel.ServiceHost for a specified type of service with a specific base address.
        /// </summary>
        /// <param name="serviceType">Specifies the type of service to host.</param>
        /// <param name="baseAddresses">The System.Array of type System.Uri that contains the base addresses for the service hosted.</param>
        /// <returns></returns>
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            return new ServiceProviderHost(DI.DefaultServiceProvider, serviceType, baseAddresses);
        }
    }
}
