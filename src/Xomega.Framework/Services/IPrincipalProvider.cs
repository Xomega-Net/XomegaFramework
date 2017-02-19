// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System.Security.Principal;

namespace Xomega.Framework
{
    /// <summary>
    /// An interface for services that track the current principal.
    /// </summary>
    public interface IPrincipalProvider
    {
        /// <summary>
        /// The principal for the current operation
        /// </summary>
        IPrincipal CurrentPrincipal { get; set; }
    }
}
