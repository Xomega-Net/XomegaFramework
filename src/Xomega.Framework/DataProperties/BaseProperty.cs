// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System;
using System.Text.RegularExpressions;

namespace Xomega.Framework
{
    /// <summary>
    /// The base class for all Xomega properties that defines various additional meta-information
    /// that can be associated with a piece of data, such as description, editability, visibility,
    /// security, whether or not it is required, etc. It also provides support for notification
    /// of any changes in this type of information.
    /// Xomega properties are typically added to Xomega data objects that can serve as a data model
    /// for user interface screens.
    /// </summary>
    public partial class BaseProperty
    {
        /// <summary>
        /// Triggers <see cref="ValueFormat.StartUp"/> method if called first.
        /// </summary>
        private static readonly ValueFormat fmt = ValueFormat.Internal;

        /// <summary>
        /// The parent data object of the property if any. In rare cases the parent can be set to null
        /// and therefore should be always checked for null.
        /// </summary>
        protected DataObject parent;

        /// <summary>
        /// Constructs a base property with the given name and a parent data object.
        /// </summary>
        /// <param name="parent">The parent data object for the property.</param>
        /// <param name="name">Property name that should be unique within the parent object.</param>
        public BaseProperty(DataObject parent, string name)
        {
            this.parent = parent;
            this.Name = name;
        }

        /// <summary>
        /// Returns the parent data object of the property when set, null otherwise.
        /// </summary>
        /// <returns>The parent data object of the property.</returns>
        public DataObject GetParent()
        {
            return parent;
        }

        /// <summary>
        /// A flag indicating if this a key property within its parent data object
        /// </summary>
        public bool IsKey { get; set; }

        /// <summary>
        /// Performs additional property initialization after all other properties and child objects
        /// have been already added to the parent object and would be accessible from within this method.
        /// </summary>
        public virtual void Initialize()
        {
            // the subclasses can implement the additional initialization
        }

        #region Property Description

        /// <summary>
        /// Internal property name, which should be unique within its parent object.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// User-friendly property label that can be used in error messages and other places
        /// to identify the property for the user.
        /// </summary>
        public string Label { get ; set; }

        /// <summary>
        /// Returns a user-friendly string representation of the property.
        /// </summary>
        /// <returns>The property label if it has been set, otherwise the property name
        /// converted to words if Pascal case is used.</returns>
        public override string ToString()
        {
            if (Label != null) return Label;
            // convert Pascal case to words
            string res = Regex.Replace(Name, "([a-z])([A-Z])", "$1 $2");
            res = Regex.Replace(res, "([A-Z][A-Z])([A-Z])([a-z])", "$1 $2$3");
            return res;
        }
        #endregion

        #region Property Change support

        /// <summary>
        /// Generic property change event for listening to all changes to the property.
        /// </summary>
        public event EventHandler<PropertyChangeEventArgs> Change;

        /// <summary>
        /// A method to fire a property change event.
        /// If certain property information is calculated and depends on the factors
        /// that are outside of the property's control (e.g. editability), 
        /// then this method may need to be called from outside to fire a property change event
        /// if certain conditions that affect the calculated value have changed.
        /// </summary>
        /// <param name="args">Property change event arguments.</param>
        public void FirePropertyChange(PropertyChangeEventArgs args)
        {
            Change?.Invoke(this, args);
        }
        #endregion

        #region Editability support

        /// <summary>
        /// An internal flag to allow manually making the property uneditable.
        /// The default value is true.
        /// </summary>
        private bool editable = true;

        /// <summary>
        /// Returns a value indicating whether or not the property is editable.
        /// This value is calculated based on the internal value of the editable field,
        /// the result of the call delegated to the parent object to determine this property's editability
        /// and the value of the security access level. Controls bound to this property
        /// should update their editability based on this value.
        /// Setting this value updates the internal editable flag and fires the property change event if necessary.
        /// </summary>
        public bool Editable
        {
            get
            {
                bool b = editable;
                if (parent != null) b &= parent.IsPropertyEditable(this);
                return b && AccessLevel > AccessLevel.ReadOnly;
            }
            set
            {
                bool oldValue = Editable;
                this.editable = value;
                if (Editable != oldValue) FirePropertyChange(
                    new PropertyChangeEventArgs(PropertyChange.Editable, oldValue, Editable));
            }
        }

        /// <summary>
        /// An internal flag that keeps track of whether or not the property is currently being edited.
        /// </summary>
        private bool editing = false;

        /// <summary>
        /// Returns whether or not the property is currently being edited by the user.
        /// Controls that are bound to this property should set this value to true or false
        /// when they gain or lose focus respectively.
        /// </summary>
        public bool Editing
        {
            get
            {
                return editing;
            }
            set
            {
                bool b = editing;
                this.editing = value;
                PropertyChange change = PropertyChange.Editing;
                if (!editing) change += PropertyChange.Value;
                if (editing != b) FirePropertyChange(
                    new PropertyChangeEventArgs(change, b, editing));
            }
        }
        #endregion

        #region Visibility support

        /// <summary>
        /// A internal flag to allow manually making the property invisible.
        /// The default value is true.
        /// </summary>
        private bool visible = true;

        /// <summary>
        /// Returns a value indicating whether or not the property is visible.
        /// This value is calculated based on the internal value of the visible field,
        /// the result of the call delegated to the parent object to determine this property's visibility
        /// and the value of the security access level. Controls bound to this property
        /// should update their visibility based on this value.
        /// Setting this value updates the internal visible flag and fires the property change event if necessary.
        /// </summary>
        public bool Visible
        {
            get
            {
                bool b = visible;
                if (parent != null) b &= parent.IsPropertyVisible(this);
                return b && AccessLevel > AccessLevel.None;
            }
            set
            {
                bool oldValue = Visible;
                this.visible = value;
                if (Visible != oldValue) FirePropertyChange(
                    new PropertyChangeEventArgs(PropertyChange.Visible, oldValue, Visible));
            }
        }
        #endregion

        #region Required support

        /// <summary>
        /// A internal flag that keeps track of whether or not the property is required.
        /// The default value is false.
        /// </summary>
        private bool required;

        /// <summary>
        /// Returns a value indicating whether or not the property is required.
        /// This value is calculated based on the internal value of the required field and
        /// the result of the call delegated to the parent object to determine if this property is required.
        /// Setting this value updates the internal required flag and fires the property change event if necessary.
        /// </summary>
        public bool Required
        {
            get
            {
                bool b = required;
                if (parent != null) b &= parent.IsPropertyRequired(this);
                return b;
            }
            set
            {
                bool oldValue = Required;
                this.required = value;
                if (Required != oldValue) FirePropertyChange(
                    new PropertyChangeEventArgs(PropertyChange.Required, oldValue, Required));
            }
        }
        #endregion

        #region Security support

        /// <summary>
        /// Internal field that stores the security access level for the property.
        /// The default value is full access.
        /// </summary>
        private AccessLevel accessLevel = AccessLevel.Full;

        /// <summary>
        /// Returns the current access level for the property.
        /// Allows setting a new access level and fires a property change event
        /// for property editability and visibility, since they both depend on the security access level.
        /// </summary>
        public AccessLevel AccessLevel
        {
            get { return accessLevel; }
            set
            {
                AccessLevel oldValue = accessLevel;
                accessLevel = value;
                FirePropertyChange(new PropertyChangeEventArgs(
                    PropertyChange.Editable + PropertyChange.Visible, oldValue, accessLevel));
            }
        }

        /// <summary>
        ///  Checks if the property is restricted, i.e. there is no access level.
        /// </summary>
        /// <returns>True if the access level is None, false otherwise.</returns>
        public bool IsRestricted() { return AccessLevel == AccessLevel.None; }

        #endregion
    }
}
