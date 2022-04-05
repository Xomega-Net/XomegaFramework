// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System.Threading.Tasks;

namespace Xomega.Framework
{
    /// <summary>
    /// A base class for providing bindings between data properties and various web or GUI controls.
    /// A property binding is responsible for making sure that the state of the control
    /// is in sync with the state of the underlying data property. This class implement common functionality
    /// for binding both WPF elements and web controls.
    /// </summary>
    public class BasePropertyBinding : BaseBinding
    {
        /// <summary>
        /// The property that the framework element / control is bound to.
        /// Initialized after a data object is set as a data context for the framework element.
        /// </summary>
        protected DataProperty property;

        /// <summary>
        /// The row in the list that this binding is bound to or null if the property is not in a list.
        /// </summary>
        protected DataRow row;

        /// <summary>
        /// Returns the actual data property that the framework element is bound to.
        /// </summary>
        public DataProperty BoundProperty => property;

        /// <summary>
        /// Binds the framework element to the given property.
        /// </summary>
        /// <param name="property">The data property to bind the framework element to.</param>
        /// <param name="row">The data row in a list to use as a context.</param>
        public virtual void BindTo(DataProperty property, DataRow row)
        {
            if (this.property != null)
                this.property.Change -= OnPropertyChange;
            this.property = property;
            this.row = row;
            if (property != null)
            {
                OnPropertyBound();
                OnPropertyChange(property, new PropertyChangeEventArgs(PropertyChange.All, null, null, row));
                // don't listen for data list properties, as it gets dispatched to the entire column
                // and significantly degrades performance
                if (!(property.GetParent() is DataListObject))
                    property.Change += OnPropertyChange;
            }
        }

        /// <summary>
        /// Implements additional logic after the property has been bound.
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
                property.SetLabel(lblText);
            }
        }

        /// <summary>
        /// Listens to the property change events and updates the framework element accordingly.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Property change event arguments.</param>
        protected virtual void OnPropertyChange(object sender, PropertyChangeEventArgs e)
        {
            if (property == sender && !PreventElementUpdate)
            {
                bool b = PreventModelUpdate;
                PreventModelUpdate = true;
                UpdateElement(e.Change);
                PreventModelUpdate = b;
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
        /// Updates a Required property of the bound element and the label (if any)
        /// based on the Required flag of the data property. Subclasses can handle it in a different way.
        /// </summary>
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
        /// Asynchronously updates the property with the given value from the element.
        /// </summary>
        /// <param name="value">The value to set on the data property.</param>
        protected virtual async Task UpdatePropertyAsync(object value)
        {
            if (property != null && !PreventModelUpdate)
            {
                bool b = PreventElementUpdate;
                PreventElementUpdate = true;
                property.SetEditing(true, row);
                await property.SetValueAsync(value, row);
                PreventElementUpdate = b;
            }
        }

        /// <summary>
        /// Updates the property with the given value from the element.
        /// </summary>
        /// <param name="value">The value to set on the data property.</param>
        protected virtual void UpdateProperty(object value)
        {
            if (property != null && !PreventModelUpdate)
            {
                bool b = PreventElementUpdate;
                PreventElementUpdate = true;
                property.SetEditing(true, row);
                property.SetValue(value, row);
                PreventElementUpdate = b;
            }
        }

        /// <summary>
        /// Disposes the data property binding by unbinding it from the underlying property.
        /// </summary>
        public override void Dispose()
        {
            BindTo(null, null); // unbind
        }

        /// <summary>
        /// A static utility method to find a child data object in the given object by the specified
        /// dot-delimited path to the child object.
        /// </summary>
        /// <param name="obj">The parent object to find the child object in.</param>
        /// <param name="childPath">A dot-delimited path to the child object.</param>
        /// <returns></returns>
        public static DataObject FindChildObject(DataObject obj, string childPath)
        {
            if (string.IsNullOrEmpty(childPath) || obj == null) return obj;
            int idx = childPath.IndexOf(".");
            if (idx < 0) return obj.GetChildObject(childPath);
            DataObject child = obj.GetChildObject(childPath.Substring(0, idx)) as DataObject;
            return FindChildObject(child, childPath.Substring(idx + 1));
        }
    }
}
