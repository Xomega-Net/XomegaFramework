// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System.Security.Principal;
using System.Threading;

namespace Xomega.Framework
{
    /// <summary>
    /// Default current principal provider based on the current thread.
    /// </summary>
    public class ThreadPrincipalProvider : IPrincipalProvider
    {
        /// <inheritdoc/>
        public IPrincipal CurrentPrincipal
        {
            get
            {
                return Thread.CurrentPrincipal;
            }
            set
            {
                Thread.CurrentPrincipal = value;
            }
        }
    }
}
