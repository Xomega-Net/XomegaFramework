// Copyright (c) 2017 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Resources;
using System.Security.Principal;
using System.Threading;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// A base class for service implementation classes that use Xomega Framework.
    /// </summary>
    public class BaseService : IPrincipalProvider
    {
        /// <summary>Triggers instrumentation setup method if called first.</summary>
        private static readonly ValueFormat fmt = ValueFormat.Internal;

        /// <summary>
        /// Instrumentation hook.
        /// </summary>
        static BaseService() {}

        /// <summary>
        /// Service provider for this service
        /// </summary>
        protected IServiceProvider serviceProvider;

        /// <summary>
        /// Error list for the current operation
        /// </summary>
        protected ErrorList currentErrors;

        /// <summary>
        /// The principal for the current operation
        /// </summary>
        private IPrincipal currentPrincipal;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BaseService(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
            this.serviceProvider = serviceProvider;
            currentErrors = serviceProvider.GetService<ErrorList>();
            if (currentErrors == null) currentErrors = new ErrorList(serviceProvider.GetService<ResourceManager>());
        }

        /// <summary>
        /// The principal for the current operation
        /// </summary>
        public IPrincipal CurrentPrincipal
        {
            get { return currentPrincipal ?? Thread.CurrentPrincipal; }
            set { currentPrincipal = value; }
        }

    }
}
