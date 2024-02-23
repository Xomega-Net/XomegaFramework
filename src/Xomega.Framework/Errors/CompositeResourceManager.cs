// Copyright (c) 2024 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace Xomega.Framework
{
    /// <summary>
    /// A resource manager that combines resources from multiple resource managers.
    /// Resource values from the resource managers listed first will override the values
    /// for the same codes from the resource managers listed last. This allows applications
    /// to override resources provided by the base library or framework.
    /// </summary>
    public class CompositeResourceManager : ResourceManager
    {
        private IEnumerable<ResourceManager> resources;

        /// <summary>
        /// Constructs a composite resource manager using the underlying resource managers.
        /// </summary>
        /// <param name="resources">Resource managers to combine resources of.</param>
        public CompositeResourceManager(params ResourceManager[] resources)
        {
            this.resources = resources ?? throw new ArgumentNullException(nameof(resources));
        }

        /// <summary>
        /// Overridden to releases all resources for the underlying resource managers.
        /// </summary>
        public override void ReleaseAllResources()
        {
            base.ReleaseAllResources();
            foreach (ResourceManager rm in resources)
                rm.ReleaseAllResources();
        }

        /// <summary>
        /// Returns the value of the specified non-string resource.
        /// </summary>
        /// <param name="name">The name of the resource to get.</param>
        /// <returns>The value of the resource localized for the caller's current culture settings.
        /// If an appropriate resource set exists but name cannot be found, the method returns null.</returns>
        public override object GetObject(string name)
        {
            object res = null;
            foreach (ResourceManager rm in resources)
            {
                if (res == null && rm != null)
                    res = rm.GetObject(name);
                if (res != null) break;
            }
            return res;
        }

        /// <summary>
        ///  Gets the value of the specified non-string resource localized for the specified culture.
        /// </summary>
        /// <param name="name">The name of the resource to get.</param>
        /// <param name="culture">The culture for which the resource is localized. If the resource is not localized
        /// for this culture, the resource manager uses fallback rules to locate an appropriate
        /// resource.If this value is null, the System.Globalization.CultureInfo object is 
        /// obtained by using the System.Globalization.CultureInfo.CurrentUICulture property.</param>
        /// <returns>The value of the resource localized for the caller's current culture settings.
        /// If an appropriate resource set exists but name cannot be found, the method returns null.</returns>
        public override object GetObject(string name, CultureInfo culture)
        {
            object res = null;
            foreach (ResourceManager rm in resources)
            {
                if (res == null && rm != null)
                    res = rm.GetObject(name, culture);
                if (res != null) break;
            }
            return res;
        }

        /// <summary>
        /// Returns the value of the specified string resource.
        /// </summary>
        /// <param name="name">The name of the resource to get.</param>
        /// <returns>The value of the resource localized for the caller's current culture settings.
        /// If an appropriate resource set exists but name cannot be found, the method returns null.</returns>
        public override string GetString(string name)
        {
            string res = null;
            foreach (ResourceManager rm in resources)
            {
                if (res == null && rm != null)
                    res = rm.GetString(name);
                if (res != null) break;
            }
            return res;
        }

        /// <summary>
        ///  Gets the value of the specified string resource localized for the specified culture.
        /// </summary>
        /// <param name="name">The name of the resource to get.</param>
        /// <param name="culture">The culture for which the resource is localized. If the resource is not localized
        /// for this culture, the resource manager uses fallback rules to locate an appropriate
        /// resource.If this value is null, the System.Globalization.CultureInfo object is 
        /// obtained by using the System.Globalization.CultureInfo.CurrentUICulture property.</param>
        /// <returns>The value of the resource localized for the caller's current culture settings.
        /// If an appropriate resource set exists but name cannot be found, the method returns null.</returns>
        public override string GetString(string name, CultureInfo culture)
        {
            string res = null;
            foreach (ResourceManager rm in resources)
            {
                if (res == null && rm != null)
                    res = rm.GetString(name, culture);
                if (res != null) break;
            }
            return res;
        }
    }
}
