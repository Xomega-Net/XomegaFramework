// Copyright (c) 2022 Xomega.Net. All rights reserved.

using System;

namespace Xomega.Framework
{
    /// <summary>
    /// Event arguments for the property change events.
    /// </summary>
    public class PropertyChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Constructs property change event arguments.
        /// </summary>
        /// <param name="change">The change or combination of changes that took place.</param>
        /// <param name="oldValue">The old value before the change.</param>
        /// <param name="newValue">The new value after the change.</param>
        /// <param name="row">The data row context, if any.</param>
        public PropertyChangeEventArgs(PropertyChange change, object oldValue, object newValue, DataRow row)
        {
            Change = change;
            OldValue = oldValue;
            NewValue = newValue;
            Row = row;
        }

        /// <summary>
        /// The property change or combination of changes for the notification.
        /// </summary>
        public PropertyChange Change { get; }

        /// <summary>
        /// For a single change, the old value before the change occurred, where applicable.
        /// </summary>
        public object OldValue { get; }

        /// <summary>
        /// For a single change, the new value after the change occurred, where applicable.
        /// </summary>
        public object NewValue { get; }

        /// <summary>
        /// The data row context for the property change event, if applicable.
        /// </summary>
        public DataRow Row { get; }

        /// <summary>
        /// Indicates if the property change was fired as an async event.
        /// </summary>
        public bool IsAsync { get; set; }
    }
}
