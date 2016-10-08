using System;
using System.Reflection;
using Xomega.Framework.Lookup;

namespace AdventureWorks.Client.Web
{
    static class XomegaFramework
    {
        /// <summary>
        /// Initializes Xomega Framework runtime environment.
        /// </summary>
        public static void Init()
        {
            // register generated Lookup Cache Loaders via the static initializer
            AdventureWorks.Services.LookupCacheLoaders.EnsureRegistered();

            // load enumerations from the generated Enumeration Data XML
            Assembly asm = typeof(AdventureWorks.Enumerations.Operators).Assembly;
            foreach (string resource in asm.GetManifestResourceNames())
                if (resource.EndsWith(".enumerations.xml"))
                {
                    LookupCache.AddCacheLoader(new XmlLookupCacheLoader(asm.GetManifestResourceStream(resource)));
                    break;
                }
        }
    }
}