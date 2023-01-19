using System;
using System.Resources;

namespace AdventureWorks.Client.Common
{
    /// <summary>
    /// Message codes, as well as a resource manager to get a (localized) message text for those.
    /// </summary>
    public static class Messages
    {
        private static readonly Lazy<ResourceManager> resourceManager =
            new Lazy<ResourceManager>(() => new ResourceManager("AdventureWorks.Client.Common.Resources", typeof(Messages).Assembly));

        /// <summary>
        /// Resource manager for the current messages.
        /// </summary>
        public static ResourceManager ResourceManager
        {
            get { return resourceManager.Value; }
        }

        /// <summary>
        /// Home
        /// </summary>
        public const string HomeView_NavMenu = "HomeView_NavMenu";

        /// <summary>
        /// From Order Date should be earlier than To Order Date.
        /// </summary>
        public const string OrderFromToDate = "OrderFromToDate";
    }
}
