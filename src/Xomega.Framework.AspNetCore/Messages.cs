using System;
using System.Resources;

namespace Xomega.Framework.AspNetCore
{
    /// <summary>
    /// Message codes, as well as a resource manager to get a (localized) message text for those.
    /// </summary>
    public static class Messages
    {
        private static readonly Lazy<ResourceManager> resourceManager =
            new Lazy<ResourceManager>(() => new ResourceManager("Xomega.Framework.AspNetCore.Resources", typeof(Messages).Assembly));

        /// <summary>
        /// Resource manager for the current messages.
        /// </summary>
        public static ResourceManager ResourceManager
        {
            get { return resourceManager.Value; }
        }

        /// <summary>
        /// Unexpected server error occurred. Please contact your system administrator.
        /// </summary>
        public const string Exception_Unhandled = "Exception_Unhandled";

        /// <summary>
        /// Lookup table '{0}' is not found in the global lookup cache.
        /// Where {0}=Lookup table name
        /// </summary>
        public const string LookupTableNotFound = "LookupTableNotFound";
    }
}
