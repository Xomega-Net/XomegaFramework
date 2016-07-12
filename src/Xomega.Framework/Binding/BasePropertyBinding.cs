// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;

namespace Xomega.Framework
{
    /// <summary>
    /// A base class for providing bindings between data properties and various web or GUI controls.
    /// A property binding is responsible for making sure that the state of the control
    /// is in sync with the state of the underlying data property. This class implement common functionality
    /// for binding both WPF elements and web controls.
    /// Property bindings are created via a factory design pattern. A <c>PropertyBindingCreator</c>
    /// callback can be registered for any particular type of controls.
    /// </summary>
    public class BasePropertyBinding
    {
        #region BasePropertyBinding factory

        /// <summary>
        /// Triggers <see cref="ValueFormat.StartUp"/> method if called first.
        /// </summary>
        private static readonly ValueFormat fmt = ValueFormat.Internal;

        /// <summary>
        /// A function that creates a data property binding for a given framework element.
        /// </summary>
        /// <param name="obj">The framework element to create the data property binding for.</param>
        /// <returns>A new data property binding for the given framework element.</returns>
        public delegate BasePropertyBinding PropertyBindingCreator(object obj);

        /// <summary>
        ///  A static dictionary of registered data property binding creation callbacks
        ///  by the type of the framework element.
        /// </summary>
        private static Dictionary<Type, PropertyBindingCreator> bindings = new Dictionary<Type, PropertyBindingCreator>();

        /// <summary>
        /// Registers a data property binding creation callback for the given type of the framework element
        /// and all subtypes of that type unless a more specific data property binding is registered for that subtype.
        /// </summary>
        /// <param name="elementType">The type of the framework element to register the data property binding for.</param>
        /// <param name="bindingCreator">The data property binding creation callback to register for the given type.</param>
        public static void Register(Type elementType, PropertyBindingCreator bindingCreator)
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
        public static BasePropertyBinding Create(object obj)
        {
            if (obj == null) return null;
            PropertyBindingCreator creator;
            for (Type t = obj.GetType(); t != null; t = t.BaseType)
            {
                if (bindings.TryGetValue(t, out creator)) return creator(obj);
            }
            return null;
        }
        #endregion

        /// <summary>
        /// The property that the framework element / control is bound to.
        /// Initialized after a data object is set as a data context for the framework element.
        /// </summary>
        protected DataProperty property;

        /// <summary>
        /// The row in the list that this binding is bound to or -1 if the property is not in a list.
        /// </summary>
        protected int row = - 1;

        /// <summary>
        /// Returns the actual data property that the framework element is bound to.
        /// </summary>
        public DataProperty BoundProperty { get { return property; } }

        /// <summary>
        /// Binds the framework element to the given property.
        /// </summary>
        /// <param name="property">The data property to bind the framework element to.</param>
        public virtual void BindTo(DataProperty property)
        {
            if (this.property != null) this.property.Change -= OnPropertyChange;
            this.property = property;
            this.row = property != null ? property.Row : -1;
            if (property != null)
            {
                OnPropertyBound();
                OnPropertyChange(property, new PropertyChangeEventArgs(PropertyChange.All, null, null));
                // don't listen for data list properties, as it gets dispatched to the entire column
                // and significantly degrades performance
                if (property.Column < 0) property.Change += OnPropertyChange;
            }
        }

        /// <summary>
        /// Implements addtional logic after the property has been bound.
        /// Sets the property label from the associated label control (see <see cref="SetLabel"/>).
        /// </summary>
        protected virtual void OnPropertyBound()
        {
            SetLabel();
        }

        /// <summary>
        /// A hook implemented by subclasses to find and store the associated label control 
        /// and set the proper to label accordingly if needed.
        /// </summary>
        protected virtual void SetLabel()
        {
            // Implemented by subclasses
        }

        /// <summary>
        /// A method to update the label text and set it on the property if needed.
        /// </summary>
        /// <param name="lblText">The label text from the label control.</param>
        protected virtual void SetPropertyLabel(string lblText)
        {
            if (lblText != null && property.Label == null)
            {
                lblText = lblText.Replace("_", "").Trim();
                if (lblText.EndsWith(":")) lblText = lblText.Substring(0, lblText.Length - 1);
                property.Label = lblText;
            }
        }

        /// <summary>
        /// A Boolean flag to prevent updates to the framework element while the data property
        /// is being updated. It is set internally to prevent an infinite recursion,
        /// but can also be set externally temporarily to control the synchronization behavior if needed.
        /// </summary>
        public bool PreventElementUpdate = false;

        /// <summary>
        /// A Boolean flag to prevent updates to the data property while the framework element
        /// is being updated. It is set internally to prevent an infinite recursion,
        /// but can also be set externally temporarily to control the synchronization behavior if needed.
        /// </summary>
        public bool PreventPropertyUpdate = false;

        /// <summary>
        /// Listens to the property change events and updates the framework element accordingly.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Property change event arguments.</param>
        protected virtual void OnPropertyChange(object sender, PropertyChangeEventArgs e)
        {
            if (property == sender && !PreventElementUpdate)
            {
                bool b = PreventPropertyUpdate;
                PreventPropertyUpdate = true;
                UpdateElement(e.Change);
                PreventPropertyUpdate = b;
            }
        }

        /// <summary>
        /// Updates the framework element based on the given property change.
        /// Subclasses can override this method, but typically they can override
        /// individual methods for each particular type of property change.
        /// </summary>
        /// <param name="change">The property change.</param>
        protected virtual void UpdateElement(PropertyChange change)
        {
            if (property == null) return;

            property.Row = row;

            if (change.IncludesEditable()) UpdateEditability();
            if (change.IncludesVisible()) UpdateVisibility();
            if (change.IncludesRequired() || change.IncludesEditable()) UpdateRequired();
            if (change.IncludesValidation() || change.IncludesEditable() || change.IncludesVisible())
                UpdateValidation();
        }

        /// <summary>
        /// Updates editability of the element based on editability of the property.
        /// Default behavior just disables the control, but subclasses can make it read-only instead
        /// or handle it in a different way.
        /// </summary>
        protected virtual void UpdateEditability()
        {
        }

        /// <summary>
        /// Updates visibility of the element based on the visibility of the property.
        /// Default behavior sets the element visibility to Collapsed if the property is not visible
        /// and updates the label visibility, but subclasses can handle it in a different way.
        /// </summary>
        protected virtual void UpdateVisibility()
        {
        }

        /// <summary>
        /// Updates a Required dependency property of the element and the label (if any)
        /// based on the Required flag of the data property. Subclasses can handle it in a different way.
        /// </summary>
        /// <seealso cref="Property.RequiredProperty"/>
        protected virtual void UpdateRequired()
        {
        }

        /// <summary>
        /// Updates WPF validation status of the element based on the validation status
        /// of the data property. The default implementation adds a single validation error
        /// to the element with a combined error text from all property validation errors.
        /// </summary>
        protected virtual void UpdateValidation()
        {
        }


        /// <summary>
        /// Updates the property with the given value from the element.
        /// </summary>
        /// <param name="value">The value to set on the data property.</param>
        protected virtual void UpdateProperty(object value)
        {
            if (property != null && !PreventPropertyUpdate)
            {
                bool b = PreventElementUpdate;
                PreventElementUpdate = true;
                property.Row = row;
                property.Editing = true;
                property.SetValue(value);
                PreventElementUpdate = b;
            }
        }

        /// <summary>
        /// Disposes the data property binding by unbinding it from the underlying property.
        /// </summary>
        public virtual void Dispose()
        {
            BindTo(null); // unbind
        }

        /// <summary>
        /// A static utility method to find a child data object in the given object by the specified
        /// dot-delimited path to the child object.
        /// </summary>
        /// <param name="obj">The parent object to find the child object in.</param>
        /// <param name="childPath">A dot-delimited path to the child object.</param>
        /// <returns></returns>
        public static IDataObject FindChildObject(DataObject obj, string childPath)
        {
            if (string.IsNullOrEmpty(childPath) || obj == null) return obj;
            int idx = childPath.IndexOf(".");
            if (idx < 0) return obj.GetChildObject(childPath);
            DataObject child = obj.GetChildObject(childPath.Substring(0, idx)) as DataObject;
            return FindChildObject(child, childPath.Substring(idx + 1));
        }
    }
}
