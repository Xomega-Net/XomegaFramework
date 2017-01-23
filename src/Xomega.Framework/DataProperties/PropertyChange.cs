// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System;

namespace Xomega.Framework
{
    /// <summary>
    /// Event arguments for the property change events.
    /// </summary>
    public class PropertyChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Property change or combination of changes for the notification.
        /// </summary>
        private PropertyChange change;

        /// <summary>
        /// For a single change, the old value before the change occured, where appliable.
        /// </summary>
        private object oldValue;

        /// <summary>
        /// For a single change, the new value after the change occured, where appliable.
        /// </summary>
        private object newValue;

        /// <summary>
        /// Constructs property change event arguments.
        /// </summary>
        /// <param name="change">The change or combination of changes that took place.</param>
        /// <param name="oldValue">The old value before the change.</param>
        /// <param name="newValue">The new value after the change.</param>
        public PropertyChangeEventArgs(PropertyChange change, object oldValue, object newValue)
        {
            this.change = change;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        /// <summary>
        /// Returns the property change or combination of changes for the notification.
        /// </summary>
        public PropertyChange Change { get { return change; } }

        /// <summary>
        /// For a single change, returns the old value before the change occured, where appliable.
        /// </summary>
        public object OldValue { get { return oldValue; } }

        /// <summary>
        /// For a single change, returns the new value after the change occured, where appliable.
        /// </summary>
        public object NewValue { get { return newValue; } }
    }

    /// <summary>
    /// A class that represents a single property change or a combination of
    /// multiple property changes for notifying of several changes at once.
    /// </summary>
    public class PropertyChange
    {
        /// <summary>
        /// A static constant representing a combination of all changes.
        /// </summary>
        public static readonly PropertyChange All = new PropertyChange(0xFFFF);

        /// <summary>
        /// A static constant representing a change in property value.
        /// </summary>
        public static readonly PropertyChange Value = new PropertyChange(1 << 0);

        /// <summary>
        /// A static constant representing a change in property editability.
        /// </summary>
        public static readonly PropertyChange Editable = new PropertyChange(1 << 1);

        /// <summary>
        /// A static constant representing a change in whether or not the property is being edited.
        /// </summary>
        public static readonly PropertyChange Editing = new PropertyChange(1 << 2);

        /// <summary>
        /// A static constant representing a change in whether or not the property is required.
        /// </summary>
        public static readonly PropertyChange Required = new PropertyChange(1 << 3);

        /// <summary>
        /// A static constant representing a change in property's list of possible items.
        /// </summary>
        public static readonly PropertyChange Items = new PropertyChange(1 << 4);

        /// <summary>
        /// A static constant representing a change in property visibility.
        /// </summary>
        public static readonly PropertyChange Visible = new PropertyChange(1 << 5);

        /// <summary>
        /// A static constant representing a change in property validation status.
        /// </summary>
        public static readonly PropertyChange Validation = new PropertyChange(1 << 6);

        /// <summary>
        /// Internal bitmask integer representing the property change(s).
        /// </summary>
        private int change;

        /// <summary>
        /// Constructs a property change class.
        /// </summary>
        /// <param name="change">The change(s) bitmask.</param>
        protected PropertyChange(int change)
        {
            this.change = change;
        }

        /// <summary>
        /// Returns if the current combination of changes includes a value change.
        /// </summary>
        /// <returns>True if the current combination of changes includes a value change, otherwise false.</returns>
        public bool IncludesValue() { return (change & Value.change) > 0; }

        /// <summary>
        /// Returns if the current combination of changes includes a change in editability.
        /// </summary>
        /// <returns>True if the current combination of changes includes a change in editability, otherwise false.</returns>
        public bool IncludesEditable() { return (change & Editable.change) > 0; }

        /// <summary>
        /// Returns if the current combination of changes includes a change
        /// in whether or not the property is being edited.
        /// </summary>
        /// <returns>True if the current combination of changes includes a change 
        /// in whether or not the property is being edited, otherwise false.</returns>
        public bool IncludesEditing() { return (change & Editing.change) > 0; }

        /// <summary>
        /// Returns if the current combination of changes includes a change
        /// in whether or not the property is required.
        /// </summary>
        /// <returns>True if the current combination of changes includes a change 
        /// in whether or not the property is required, otherwise false.</returns>
        public bool IncludesRequired() { return (change & Required.change) > 0; }

        /// <summary>
        /// Returns if the current combination of changes includes a change
        /// in property's possible values.
        /// </summary>
        /// <returns>True if the current combination of changes includes a change
        /// in property's possible values, otherwise false.</returns>
        public bool IncludesItems() { return (change & Items.change) > 0; }

        /// <summary>
        /// Returns if the current combination of changes includes a change in visibility.
        /// </summary>
        /// <returns>True if the current combination of changes includes a change
        /// in visibility, otherwise false.</returns>
        public bool IncludesVisible() { return (change & Visible.change) > 0; }

        /// <summary>
        /// Returns if the current combination of changes includes a change in validation status.
        /// </summary>
        /// <returns>True if the current combination of changes includes a change
        /// in validation status, otherwise false.</returns>
        public bool IncludesValidation() { return (change & Validation.change) > 0; }

        /// <summary>
        /// Combines two property changes and returns the change that represents the combination.
        /// </summary>
        /// <param name="lhs">Left-hand side property change.</param>
        /// <param name="rhs">Right-hand side property change.</param>
        /// <returns>The combination of the two property changes.</returns>
        public static PropertyChange operator +(PropertyChange lhs, PropertyChange rhs)
        {
            return new PropertyChange(lhs.change | rhs.change);
        }

        /// <summary>
        /// Removes the right-hand side property change from the left-hand side combination of changes.
        /// </summary>
        /// <param name="lhs">The combination of property changes to remove the change from.</param>
        /// <param name="rhs">The property change to remove from the left-hand side combination.</param>
        /// <returns>The left-hand side property change without the right-hand side change.</returns>
        public static PropertyChange operator -(PropertyChange lhs, PropertyChange rhs)
        {
            return new PropertyChange(lhs.change & ~rhs.change);
        }
    }
}
