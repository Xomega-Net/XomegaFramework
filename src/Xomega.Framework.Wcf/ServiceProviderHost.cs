// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Xomega.Framework.Wcf
{
    /// <summary>
    /// A service host for a given service type based on the specified service provider.
    /// </summary>
    public class ServiceProviderHost : ServiceHost
    {
        /// <summary>
        /// Constructs a new service host for a given service type based on the specified service provider.
        /// </summary>
        /// <param name="serviceProvider">Service provider for service implementations</param>
        /// <param name="serviceType">The type of hosted service.</param>
        /// <param name="baseAddresses">An array of type System.Uri that contains the base addresses for the hosted service.</param>
        public ServiceProviderHost(IServiceProvider serviceProvider, Type serviceType, params Uri[] baseAddresses) : base(serviceType, baseAddresses)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            foreach (ContractDescription contract in ImplementedContracts.Values)
            {
                contract.ContractBehaviors.Add(new ServiceInstanceProvider(serviceProvider, contract.ContractType));
            }
        }

        public class Factory 
        {

        }
    }
}
