// Copyright (c) 2019 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;

namespace Xomega.Framework.Wcf
{
    /// <summary>
    /// Self-registering instance context extnsion that stores the service scope for an instance context
    /// </summary>
    public class InstanceContextServiceScope : IExtension<InstanceContext>, IInstanceContextInitializer
    {
        // service scope for the current instance context
        private IServiceScope serviceScope;

        /// <summary>
        /// Gets the service scope for the current instance context.
        /// </summary>
        /// <param name="serviceProvider">Service provider used to create service scope.</param>
        /// <returns></returns>
        public IServiceScope GetServiceScope(IServiceProvider serviceProvider)
        {
            if (serviceScope == null) serviceScope = serviceProvider.CreateScope();
            return serviceScope;
        }

        /// <summary>
        /// Dispose service scope for the current instance context
        /// </summary>
        public void Dispose()
        {
            if (serviceScope != null) serviceScope.Dispose();
            serviceScope = null;
        }

        #region IExtension<InstanceContext> implementation

        /// <summary>
        /// Enables an extension object to find out when it has been aggregated.
        /// </summary>
        /// <param name="owner">The extensible object that aggregates this extension.</param>
        public void Attach(InstanceContext owner)
        {
        }

        /// <summary>
        /// Enables an extension object to find out when it is no longer aggregated.
        /// </summary>
        /// <param name="owner">The extensible object that aggregates this extension.</param>
        public void Detach(InstanceContext owner)
        {
        }

        #endregion

        #region IInstanceContextInitializer implementation

        /// <summary>
        /// Provides the ability to modify the newly created System.ServiceModel.InstanceContext object.
        /// </summary>
        /// <param name="instanceContext">The system-supplied instance context.</param>
        /// <param name="message">The message that triggered the creation of the instance context.</param>
        public void Initialize(InstanceContext instanceContext, Message message)
        {
            instanceContext.Extensions.Add(new InstanceContextServiceScope());
        }

        #endregion
    }
}
