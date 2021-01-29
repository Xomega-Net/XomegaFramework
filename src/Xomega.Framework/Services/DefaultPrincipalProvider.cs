// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System.Security.Principal;

namespace Xomega.Framework
{
    /// <summary>
    /// Default current principal provider.
    /// </summary>
    public class DefaultPrincipalProvider : IPrincipalProvider
    {
        /// <inheritdoc/>
        public IPrincipal CurrentPrincipal { get; set; }
    }
}
