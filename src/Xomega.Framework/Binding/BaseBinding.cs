// Copyright (c) 2019 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;

namespace Xomega.Framework
{
    /// <summary>
    /// A base class for providing bindings between data objects/properties and various web or GUI controls.
    /// Bindings are created via a factory design pattern. A <c>BindingCreator</c>
    /// callback can be registered for any particular type of control.
    /// </summary>
    public class BaseBinding : IDisposable
    {
        /// <summary>
        /// A function that creates a binding for a given framework element.
        /// </summary>
        /// <param name="obj">The framework element to create the binding for.</param>
        /// <returns>A new data property binding for the given framework element.</returns>
        public delegate BaseBinding BindingCreator(object obj);

        /// <summary>
        ///  A static dictionary of registered data property binding creation callbacks
        ///  by the type of the framework element.
        /// </summary>
        private static Dictionary<Type, BindingCreator> bindings = new Dictionary<Type, BindingCreator>();

        /// <summary>
        /// Registers a data property binding creation callback for the given type of the framework element
        /// and all subtypes of that type unless a more specific data property binding is registered for that subtype.
        /// </summary>
        /// <param name="elementType">The type of the framework element to register the data property binding for.</param>
        /// <param name="bindingCreator">The data property binding creation callback to register for the given type.</param>
        public static void Register(Type elementType, BindingCreator bindingCreator)
        {
            if (bindingCreator == null) throw new ArgumentNullException("bindingCreator");
            bindings[elementType] = bindingCreator;
        }

        /// <summary>
        /// Creates a new data property binding for the given framework element
        /// based on the data property binding creation callbacks that have been registered
        /// for the type of the given framework element or any of its base types.
        /// </summary>
        /// <param name="obj">The framework element to create the data property binding for.</param>
        /// <returns>A new data property binding for the given framework element.</returns>
        public static BaseBinding Create(object obj)
        {
            if (obj == null) return null;
            BindingCreator creator;
            for (Type t = obj.GetType(); t != null; t = t.BaseType)
            {
                if (bindings.TryGetValue(t, out creator)) return creator(obj);
            }
            return null;
        }

        /// <summary>
        /// A Boolean flag to prevent updates to the framework element while the data model
        /// is being updated. It is set internally to prevent an infinite recursion,
        /// but can also be set externally temporarily to control the synchronization behavior if needed.
        /// </summary>
        public bool PreventElementUpdate = false;

        /// <summary>
        /// A Boolean flag to prevent updates to the data model while the framework element
        /// is being updated. It is set internally to prevent an infinite recursion,
        /// but can also be set externally temporarily to control the synchronization behavior if needed.
        /// </summary>
        public bool PreventModelUpdate = false;

        /// <summary>
        /// Disposes the binding
        /// </summary>
        public virtual void Dispose()
        {
            // implemented by subclasses
        }
    }
}
