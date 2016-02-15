// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System.Collections;

namespace Xomega.Framework
{
    /// <summary>
    /// Data object interface to be implemented by the parent object
    /// or all child objects of any data object.
    /// Both <c>DataObject</c> and <c>DataObjectList</c> implement this interface.
    /// </summary>
    public interface IDataObject : IValidatable
    {
        /// <summary>
        /// Gets or sets the parent object for the current data object.
        /// </summary>
        IDataObject Parent { get; set; }

        /// <summary>
        /// Returns if the current object is editable, which may be based on several factors.
        /// Allows making the object non-editable by setting this field to true.
        /// </summary>
        bool Editable { get; set; }
        
        /// <summary>
        /// Allows controlling property editability on the data object level.
        /// </summary>
        /// <param name="p">The property to check the editability of.</param>
        /// <returns>True if the property should be editable, false otherwise.</returns>
        bool IsPropertyEditable(BaseProperty p);

        /// <summary>
        /// Allows controlling if the property is required on the data object level.
        /// </summary>
        /// <param name="p">The property being checked if it's required.</param>
        /// <returns>True if the property should be required, false otherwise.</returns>
        bool IsPropertyRequired(BaseProperty p);

        /// <summary>
        /// Allows controlling property visibility on the data object level.
        /// </summary>
        /// <param name="p">The property to check the visibility of.</param>
        /// <returns>True if the property should be visible, false otherwise.</returns>
        bool IsPropertyVisible(BaseProperty p);

        /// <summary>
        /// Fires a property change event recursively through all properties and child objects.
        /// </summary>
        /// <param name="args">Property change event arguments.</param>
        void FirePropertyChange(PropertyChangeEventArgs args);

        /// <summary>
        /// Gets all validation errors from the data object,
        /// all its properties and child objects recursively.
        /// </summary>
        /// <returns>Validation errors from the data object,
        /// all its properties and child objects.</returns>
        ErrorList GetValidationErrors();

        /// <summary>
        /// Resets validation status to not validated on the object,
        /// all its properties and child objects recursively.
        /// </summary>
        void ResetAllValidation();

        /// <summary>
        /// Returns the modification state of the data object.
        /// </summary>
        /// <returns>The modification state of the data object.
        /// Null means the date object has never been initialized with data.
        /// False means the data object has been initialized, but has not been changed since then.
        /// True means that the data object has been modified since it was initialized.</returns>
        bool? IsModified();

        /// <summary>
        /// Sets the modification state for the data object to the specified value.
        /// </summary>
        /// <param name="modified">The modification state value.
        /// Null means the date object has never been initialized with data.
        /// False means the data object has been initialized, but has not been changed since then.
        /// True means that the data object has been modified since it was initialized.</param>
        /// <param name="recursive">True to propagate the modification state
        /// to all properties and child objects, false otherwise.</param>
        void SetModified(bool? modified, bool recursive);

        /// <summary>
        /// Perform a deep copy of the state from another data object (presumably of the same type).
        /// </summary>
        /// <param name="obj">The object to copy the state from.</param>
        void CopyFrom(IDataObject obj);
    }

    /// <summary>
    /// An interface implemented by child or parent data object lists.
    /// </summary>
    public interface IDataObjectList : IDataObject, IList
    {
        /// <summary>
        /// Constructs a new data object of the appropriate type for the data object list.
        /// </summary>
        /// <returns>A new data object of the appropriate type for the data object list.</returns>
        DataObject NewDataObject();

        /// <summary>
        /// Populates the data object list and imports the data from the given data contract list.
        /// </summary>
        /// <param name="list">A list of data contract objects to populate the list from.</param>
        void FromDataContract(IEnumerable list);

        /// <summary>
        /// Exports the data from the data object list to the list of data contract objects.
        /// </summary>
        /// <param name="list">The list of data contract objects to export the data to.</param>
        void ToDataContract(IList list);

        /// <summary>
        /// Gets or sets sort criteria for the data object list.
        /// </summary>
        ListSortCriteria SortCriteria { get; set; }

        /// <summary>
        /// Sorts the data object list according to the specified <see cref="SortCriteria"/>.
        /// </summary>
        void Sort();

        /// <summary>
        /// Gets or sets a data object that is currently being edited.
        /// </summary>
        DataObject EditObject { get; set; }
    }
}
