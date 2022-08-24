// Copyright (c) 2022 Xomega.Net. All rights reserved.

using System.Resources;

namespace Xomega.Framework
{
    /// <summary>
    /// Resource manager extension functions.
    /// </summary>
    public static class ResourceManagerExt
    {
        /// <summary>
        /// Get the value of a string resource for the specified key, using prefixed key where available
        /// to allow overriding generic resources with more specific.
        /// </summary>
        public static string GetString(this ResourceManager resourceManager, string key, string keyPrefix)
        {
            if (resourceManager == null) return null;

            string text = null;
            if (!string.IsNullOrEmpty(keyPrefix))
                text = resourceManager.GetString(keyPrefix + key);
            if (text == null)
                text = resourceManager.GetString(key);
            return text;
        }
    }
}
