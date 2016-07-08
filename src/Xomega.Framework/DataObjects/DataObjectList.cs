// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Xomega.Framework
{
    /// <summary>
    /// A dynamic list of data objects of a certain type, which can be used independently
    /// or as a child object of another data object. It is also a parent for all its contained objects.
    /// A data object list supports validation and modification tracking as well as
    /// exporting its data to or importing it from a data contract that is a list of data contract objects.
    /// It also supports manipulating certain aspects of the properties for all data objects in the list,
    /// such as visibility, editability or whether or not the property is required. This way, for example,
    /// if a data object list is bound to a data grid, one could hide or unhide the whole column on the grid
    /// by setting the visibility of the corresponding data object list property.
    /// </summary>
    /// <typeparam name="T">The type of the underlying data object, which should have a default constructor.</typeparam>
    [Obsolete("Use DataListObject class instead for performance")]
    public class DataObjectList<T> : ObservableCollection<T>, IDataObjectList where T : DataObject, new()
    {
        /// <summary>
        /// A template data object for controlling properties of all data objects in the list.
        /// </summary>
        protected DataObject template;

        /// <summary>
        /// Constructs a new data object list.
        /// </summary>
        public DataObjectList()
        {
            template = new T();
        }

        /// <summary>
        /// Returns a template property for the whole list by the property name.
        /// It corresponds to a whole column in a grid, which could be controlled through this property.
        /// Because there is no real data in template properties, an instance of <c>BaseProperty</c>
        /// is returned as opposed to a regular <c>DataProperty</c>.
        /// </summary>
        /// <param name="propertyName">The name of the property to return.</param>
        /// <returns>A base property that represents a column in the data object list.</returns>
        public BaseProperty this[string propertyName] { get { return template[propertyName]; } }

        #region Sorting

        /// <summary>
        /// Gets or sets sort criteria for the data object list.
        /// </summary>
        public ListSortCriteria SortCriteria { get; set; }

        /// <summary>
        /// Sorts the data object list according to the specified <see cref="SortCriteria"/>.
        /// </summary>
        public virtual void Sort()
        {
            if (SortCriteria == null || SortCriteria.Count == 0) return;

            Sort(delegate(T obj1, T obj2)
            {
                int res = 0;
                foreach (ListSortField sortFld in SortCriteria)
                {
                    DataProperty p1 = obj1[sortFld.PropertyName];
                    DataProperty p2 = obj2[sortFld.PropertyName];
                    if (p1 != null && p2 != null)
                    {
                        object val1 = p1.InternalValue;
                        object val2 = p2.InternalValue;
                        if (val1 == val2) res = 0;
                        else if (val1 == null && val2 != null) res = -1;
                        else if (val1 != null && val2 == null) res = 1;
                        else if (val1 is IComparable) res = ((IComparable)val1).CompareTo(val2);
                        else if (val2 is IComparable) res = -((IComparable)val2).CompareTo(val1);
                        else res = string.Compare(p1.DisplayStringValue, p2.DisplayStringValue);
                        if (sortFld.SortDirection == ListSortDirection.Descending) res *= -1;
                    }
                    if (res != 0) return res;
                }
                return res;
            });
        }

        /// <summary>
        /// Sorts the data object list according to the specified comparison function.
        /// </summary>
        public void Sort(Comparison<T> cmp)
        {
            List<T> items = Items as List<T>;
            items.Sort(cmp);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        #endregion

        #region IDataObjectList Members

        /// <summary>
        /// Gets or sets the parent object for the current data object list.
        /// </summary>
        public IDataObject Parent
        {
            get { return template.Parent; }
            set { template.Parent = value; }
        }

        /// <summary>
        /// Returns if the current data object list is editable, which may be based on several factors.
        /// Allows making the object non-editable by setting this field to true.
        /// </summary>
        public bool Editable
        {
            get { return template.Editable; }
            set { template.Editable = value; }
        }

        /// <summary>
        /// Delegates determining property editability to the template object.
        /// </summary>
        /// <param name="p">The property to check the editability of.</param>
        /// <returns>True if the property should be editable, false otherwise.</returns>
        public bool IsPropertyEditable(BaseProperty p)
        {
            DataProperty tp = template[p.Name];
            return tp == null || tp.Editable;
        }

        /// <summary>
        /// Delegates determining if property is required to the template object.
        /// </summary>
        /// <param name="p">The property being checked if it's required.</param>
        /// <returns>True if the property should be required, false otherwise.</returns>
        public bool IsPropertyRequired(BaseProperty p)
        {
            DataProperty tp = template[p.Name];
            return tp == null || tp.Required;
        }

        /// <summary>
        /// Delegates determining property visibility to the template object.
        /// </summary>
        /// <param name="p">The property to check the visibility of.</param>
        /// <returns>True if the property should be visible, false otherwise.</returns>
        public bool IsPropertyVisible(BaseProperty p)
        {
            DataProperty tp = template[p.Name];
            return tp == null || tp.Visible;
        }

        /// <summary>
        /// Fires a property change event on the template object.
        /// </summary>
        /// <param name="args">Property change event arguments.</param>
        public void FirePropertyChange(PropertyChangeEventArgs args)
        {
            template.FirePropertyChange(args);
        }

        /// <summary>
        /// Perform a deep copy of the state from another data object list (presumably of the same type).
        /// </summary>
        /// <param name="obj">The object to copy the state from.</param>
        public virtual void CopyFrom(IDataObject obj)
        {
            IList lst = obj as IList;
            if (lst == null) return;
            if (Count > 0) Clear();
            foreach (object o in lst)
            {
                IDataObject dObj = o as IDataObject;
                if (dObj == null) continue;
                T newObj = new T();
                newObj.CopyFrom(dObj);
                Add(newObj);
            }
        }

        /// <summary>
        /// Gets or sets a data object that is currently being edited.
        /// </summary>
        public DataObject EditObject { get; set; }

        #endregion

        #region Validation

        /// <summary>
        /// A list of validation errors that are not tied to any particular
        /// data property but rather to the data object list as a whole.
        /// Null value means that the object list has not been validated yet.
        /// </summary>
        protected ErrorList validationErrorList;

        /// <summary>
        /// Resets validation status to not validated on the object list
        /// by setting the validation error list to null.
        /// </summary>
        public void ResetValidation()
        {
            validationErrorList = null;
        }

        /// <summary>
        /// Validates the data object list and all its contained objects recursively.
        /// </summary>
        /// <param name="force">True to validate regardless of
        /// whether or not it has been already validated.</param>
        public virtual void Validate(bool force)
        {
            foreach (DataObject obj in this) obj.Validate(force);
            if (force) ResetValidation();
            if (validationErrorList != null) return;

            validationErrorList = new ErrorList();
        }

        /// <summary>
        /// Gets all validation errors from the data object list and all its contained objects recursively.
        /// </summary>
        /// <returns>Validation errors from the data object and all its contained objects.</returns>
        public ErrorList GetValidationErrors()
        {
            Validate(false);
            ErrorList errLst = new ErrorList();
            if (validationErrorList != null) errLst.MergeWith(validationErrorList);
            foreach (DataObject obj in this) errLst.MergeWith(obj.GetValidationErrors());
            return errLst;
        }

        /// <summary>
        /// Resets validation status to not validated on the object list
        /// and all its contained objects recursively.
        /// </summary>
        public void ResetAllValidation()
        {
            ResetValidation();
            foreach (DataObject obj in Items) obj.ResetAllValidation();
        }

        #endregion
        
        #region Modification support

        /// <summary>
        /// Tracks the modification state of the data object list,
        /// which includes if any new objects have been added or any existing objects have been deleted or modified.
        /// Null means the date object list has never been initialized with data.
        /// False means the data object list has been initialized, but has not been changed since then.
        /// True means that the data object list has been modified since it was initialized.
        /// </summary>
        protected bool? modified;

        /// <summary>
        /// Returns the modification state of the data object list,
        /// which includes if any new objects have been added or any existing objects have been deleted or modified.
        /// </summary>
        /// <returns>The modification state of the data object list.
        /// Null means the date object list has never been initialized with data.
        /// False means the data object list has been initialized, but has not been changed since then.
        /// True means that the data object list has been modified since it was initialized.</returns>
        public bool? IsModified()
        {
            bool? res = modified;
            foreach (DataObject obj in this)
            {
                bool? objModified = obj.IsModified();
                if (objModified.HasValue) res |= objModified;
            }
            return res;
        }

        /// <summary>
        /// Sets the modification state for the data object list to the specified value.
        /// </summary>
        /// <param name="modified">The modification state value.
        /// Null means the date object list has never been initialized with data.
        /// False means the data object list has been initialized, but has not been changed since then.
        /// True means that the data object list has been modified since it was initialized.</param>
        /// <param name="recursive">True to propagate the modification state
        /// to all contained objects, false otherwise.</param>
        public void SetModified(bool? modified, bool recursive)
        {
            this.modified = modified;
            if (recursive) foreach (DataObject obj in this) obj.SetModified(modified, true);
        }

        #endregion

        /// <summary>
        /// Constructs a new data object of the appropriate type for the data object list.
        /// </summary>
        /// <returns>A new data object of the appropriate type for the data object list.</returns>
        public virtual DataObject NewDataObject()
        {
            return new T();
        }

        /// <summary>
        /// Tracks changes to the list of objects to update the modification state
        /// as well as the parent object on all objects that have been added or deleted.
        /// </summary>
        /// <param name="e">Collection change event arguments.</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (DataObject obj in e.OldItems) obj.Parent = null;
                // if items were removed, mark the list as modified
                if (e.OldItems.Count > 0) SetModified(true, false);
            }
            if (e.NewItems != null)
                foreach (DataObject obj in e.NewItems)
                {
                    obj.Parent = this;
                    // if new items are added that have not been read, mark the list as modified
                    if (!obj.IsModified().HasValue) SetModified(true, false);
                }

            base.OnCollectionChanged(e);
        }

        /// <summary>
        /// Populates the data object list and imports the data from the given data contract list.
        /// </summary>
        /// <param name="list">A list of data contract objects to populate the list from.</param>
        public virtual void FromDataContract(IEnumerable list)
        {
            if (list == null) return;
            Clear();
            SetModified(false, false);
            foreach (object contractItem in list)
            {
                T objectItem = NewDataObject() as T;
                if (objectItem == null) continue;
                objectItem.FromDataContract(contractItem);
                Add(objectItem);
            }
        }

        /// <summary>
        /// Exports the data from the data object list to the list of data contract objects.
        /// </summary>
        /// <param name="list">The list of data contract objects to export the data to.
        /// The list should be generic with a single type parameter.</param>
        public virtual void ToDataContract(IList list)
        {
            if (list == null) return;
            Type listType = list.GetType();
            if (!listType.IsGenericType || listType.GetGenericArguments().Length != 1) return;

            Type contractItemType = list.GetType().GetGenericArguments()[0];
            foreach (DataObject objectItem in this)
            {
                object item = Activator.CreateInstance(contractItemType);
                objectItem.ToDataContract(item);
                list.Add(item);
            }
        }

        /// <summary>
        /// Implements IDataObject.ResetData by clearing the list
        /// </summary>
        public void ResetData()
        {
            ClearItems();
        }
    }
}
