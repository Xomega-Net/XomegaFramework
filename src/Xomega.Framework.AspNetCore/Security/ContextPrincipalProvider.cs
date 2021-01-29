// Copyright (c) 2021 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Principal;

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Current principal provider based on the current HTTP context.
    /// </summary>
    public class ContextPrincipalProvider : IPrincipalProvider
    {
        private readonly IHttpContextAccessor contextAccessor;

        /// <summary>
        /// Constructs the current principal provider using the injected HTTP context accessor.
        /// </summary>
        /// <param name="contextAccessor">Injected HTTP context accessor.</param>
        public ContextPrincipalProvider(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        /// <inheritdoc/>
        public IPrincipal CurrentPrincipal
        {
            get
            {
                return contextAccessor?.HttpContext?.User;
            }
            set
            {
                if (contextAccessor?.HttpContext != null)
                    contextAccessor.HttpContext.User = value as ClaimsPrincipal;
            }
        }
    }
}
