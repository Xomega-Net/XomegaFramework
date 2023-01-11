// Copyright (c) 2023 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.Diagnostics;
using System.ServiceModel.Description;
using Xomega.Framework.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Xomega.Framework.Wcf
{
    /// <summary>
    /// WCF service instance provider for a given contract type using specified service provider, which can be registered as a contract behavior.
    /// </summary>
    public class ServiceInstanceProvider : IInstanceProvider, IContractBehavior, IParameterInspector
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
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.contractType = contractType ?? throw new ArgumentNullException(nameof(contractType));
        }

        #region IInstanceProvider implementation

        /// <summary>
        /// Returns a service object given the specified <see cref="InstanceContext"/> object.
        /// </summary>
        /// <param name="instanceContext">The current InstanceContext object.</param>
        /// <returns>A user-defined service object.</returns>
        public object GetInstance(InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

        /// <summary>
        /// Returns a service object given the specified <see cref="InstanceContext"/> object.
        /// </summary>
        /// <param name="instanceContext">The current InstanceContext object.</param>
        /// <param name="message">The message that triggered the creation of a service object.</param>
        /// <returns>The service object.</returns>
        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            InstanceContextServiceScope ctxScope = instanceContext.Extensions.Find<InstanceContextServiceScope>();
            if (ctxScope == null)
            {
                Trace.TraceError("InstanceContextServiceScope not registered for instance context. Using default service provider");
                return serviceProvider.GetService(contractType);
            }
            IServiceScope scope = ctxScope.GetServiceScope(serviceProvider);

            // find and set the current principal on the principal provider for the current scope
            IPrincipalProvider principalProvider = scope.ServiceProvider.GetService<IPrincipalProvider>();
            if (principalProvider != null)
            {
                if (ServiceSecurityContext.Current.AuthorizationContext.Properties.TryGetValue("ClaimsPrincipal", out object principal))
                    principalProvider.CurrentPrincipal = principal as ClaimsPrincipal;
            }

            return scope.ServiceProvider.GetService(contractType);
        }

        /// <summary>
        /// Called when an <see cref="InstanceContext"/> object recycles a service object.
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
            foreach (var oper in dispatchRuntime.Operations)
            {
                oper.ParameterInspectors.Add(this);
            }
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

        #region IParameterInspector implementation

        /// <summary>
        /// Called before client calls are sent and after service responses are returned.
        /// </summary>
        /// <param name="operationName">The name of the operation.</param>
        /// <param name="inputs">The objects passed to the method by the client.</param>
        /// <returns>The correlation state that is returned as the correlationState parameter in <see cref="AfterCall(string, object[], object, object)"/>.
        /// Return null if you do not intend to use correlation state.</returns>
        public object BeforeCall(string operationName, object[] inputs)
        {
            InstanceContextServiceScope ctxScope = OperationContext.Current.InstanceContext.Extensions.Find<InstanceContextServiceScope>();
            var svcProvider = ctxScope?.GetServiceScope(null)?.ServiceProvider;
            ErrorList errors = svcProvider?.GetService<ErrorList>();
            if (errors == null) return null;

            foreach (object obj in inputs)
            {
                foreach(ValidationResult result in DataAnnotationValidator.GetValidationErrors(serviceProvider, obj))
                {
                    errors.AddValidationError(result.ErrorMessage);
                }
            }
            return null;
        }

        /// <summary>
        /// Called after client calls are returned and before service responses are sent.
        /// </summary>
        /// <param name="operationName">The name of the invoked operation.</param>
        /// <param name="outputs">Any output objects.</param>
        /// <param name="returnValue">The return value of the operation.</param>
        /// <param name="correlationState">Any correlation state returned from the <see cref="BeforeCall(string, object[])"/> method, or null.</param>
        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
        }

        #endregion
    }
}
