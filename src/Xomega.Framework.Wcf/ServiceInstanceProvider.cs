// Copyright (c) 2010-2016 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.Diagnostics;
using System.ServiceModel.Description;

namespace Xomega.Framework.Wcf
{
    /// <summary>
    /// WCF service instance provider for a given contract type using specified service provider.
    /// It can be registered as a contract behavior, and it also provides handling of ErrorAbortException.
    /// </summary>
    public class ServiceInstanceProvider : IInstanceProvider, IContractBehavior, IErrorHandler
    {
        private readonly IServiceProvider serviceProvider;
        private readonly Type contractType;

        /// <summary>
        /// Constructs a new service instance provider for the specified contract type
        /// </summary>
        /// <param name="serviceProvider">Service provider for the service implementation</param>
        /// <param name="contractType">Contract type</param>
        public ServiceInstanceProvider(IServiceProvider serviceProvider, Type contractType)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            if (contractType == null) throw new ArgumentNullException("contractType");
            this.serviceProvider = serviceProvider;
            this.contractType = contractType;
        }

        #region IInstanceProvider implementation

        /// <summary>
        /// Returns a service object given the specified System.ServiceModel.InstanceContext object.
        /// </summary>
        /// <param name="instanceContext">The current InstanceContext object.</param>
        /// <returns>A user-defined service object.</returns>
        public object GetInstance(InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

        /// <summary>
        /// Returns a service object given the specified System.ServiceModel.InstanceContext object.
        /// </summary>
        /// <param name="instanceContext">The current InstanceContext object.</param>
        /// <param name="message">The message that triggered the creation of a service object.</param>
        /// <returns>The service object.</returns>
        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            InstanceContextServiceScope ctxScope = instanceContext.Extensions.Find<InstanceContextServiceScope>();
            if (ctxScope == null)
            {
                Trace.TraceError("InstanceContextServiceScope not registred for instance context. Using default service provider");
                return serviceProvider.GetService(contractType);
            }
            IServiceScope scope = ctxScope.GetServiceScope(serviceProvider);
            return scope.ServiceProvider.GetService(contractType);
        }

        /// <summary>
        /// Called when an System.ServiceModel.InstanceContext object recycles a service object.
        /// </summary>
        /// <param name="instanceContext">The service's instance context.</param>
        /// <param name="instance">The service object to be recycled.</param>
        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            InstanceContextServiceScope ctxScope = instanceContext.Extensions.Find<InstanceContextServiceScope>();
            if (ctxScope != null) ctxScope.Dispose();
        }

        #endregion

        #region IContractBehavior implementation

        /// <summary>
        /// Implement to confirm that the contract and endpoint can support the contract behavior.
        /// </summary>
        /// <param name="contractDescription">The contract to validate.</param>
        /// <param name="endpoint">The endpoint to validate.</param>
        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }

        /// <summary>
        /// Registers this instance provider and an InstanceContextServiceScope with the dispatch runtime.
        /// </summary>
        /// <param name="contractDescription">The contract description to be modified.</param>
        /// <param name="endpoint">The endpoint that exposes the contract.</param>
        /// <param name="dispatchRuntime">The dispatch runtime that controls service execution.</param>
        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            dispatchRuntime.InstanceProvider = this;
            dispatchRuntime.InstanceContextInitializers.Add(new InstanceContextServiceScope());
            dispatchRuntime.ChannelDispatcher.ErrorHandlers.Add(this);
        }

        /// <summary>
        /// Implements a modification or extension of the client across a contract.
        /// </summary>
        /// <param name="contractDescription">The contract description for which the extension is intended.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="clientRuntime">The client runtime.</param>
        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        /// <summary>
        /// Configures any binding elements to support the contract behavior.
        /// </summary>
        /// <param name="contractDescription">The contract description to modify.</param>
        /// <param name="endpoint">The endpoint to modify.</param>
        /// <param name="bindingParameters">The objects that binding elements require to support the behavior.</param>
        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        #endregion

        #region IErrorHandler implementation

        /// <summary>
        /// Handles ErrorAbortException to provide a fault with ErrorList to the client.
        /// </summary>
        /// <param name="error">The System.Exception object thrown in the course of the service operation.</param>
        /// <param name="version">The SOAP version of the message.</param>
        /// <param name="fault">The System.ServiceModel.Channels.Message object that is returned to the client,
        /// or service, in the duplex case.</param>
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            ErrorAbortException ea = error as ErrorAbortException;
            if (ea == null) return;
            FaultException fe = new FaultException<ErrorList>(ea.Errors, new FaultReason(ea.Message), new FaultCode("Sender"), null);
            fault = Message.CreateMessage(version, fe.CreateMessageFault(), "ErrorList");
        }

        /// <summary>
        /// Enables error-related processing and returns a value that indicates whether the
        /// dispatcher aborts the session and the instance context in certain cases.
        /// </summary>
        /// <param name="error">The exception thrown during processing.</param>
        /// <returns>true if WCF should not abort the session (if there is one)
        /// and instance context if the instance context is not System.ServiceModel.InstanceContextMode.Single;
        /// otherwise, false. The default is false.</returns>
        public bool HandleError(Exception error)
        {
            return error is ErrorAbortException;
        }

        #endregion
    }
}
