// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Reflection;

namespace Xomega.Framework
{
    /// <summary>
    /// Base class for configuring services and the application.
    /// Subclasses should have a default constructor.
    /// </summary>
    public abstract class AppInitializer
    {
        /// <summary>
        /// Initializes the application
        /// </summary>
        /// <param name="initalizer">Initalizer to use</param>
        public static void Initalize(AppInitializer initalizer)
        {
            DI.DefaultServiceProvider = initalizer.ConfigureServices();
            initalizer.InitalizeApp(DI.DefaultServiceProvider);
        }

        /// <summary>
        /// Get type by name looking in all current assemblies if needed
        /// </summary>
        /// <param name="name">type name</param>
        /// <returns>Type object for the given type name</returns>
        public static Type GetType(string name)
        {
            if (name == null) return null;
            Type type = Type.GetType(name);
            if (type != null) return type;
            else
            {
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = asm.GetType(name, false);
                    if (type != null) return type;
                }
            }
            return null;
        }

        /// <summary>
        /// Configures specified service container with application services
        /// </summary>
        /// <returns>The configured and built service provider</returns>
        public abstract IServiceProvider ConfigureServices();

        /// <summary>
        /// Initializes application using configured service provider
        /// </summary>
        /// <param name="serviceProvider">Configured service provider</param>
        public virtual void InitalizeApp(IServiceProvider serviceProvider)
        {
        }
    }
}
