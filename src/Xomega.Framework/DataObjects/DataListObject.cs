// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Xomega.Framework
{
    /// <summary>
    /// A dynamic data object that has a list of rows as its data instead of specific values.
    /// </summary>
    public abstract class DataListObject : DataObject, INotifyCollectionChanged, IEnumerator<DataObject>, IEnumerable<DataObject>
    {
        /// <summary>
        /// The data table for the list stored as an array of arrays
        /// </summary>
        protected List<DataRow> data = new List<DataRow>();

        /// <summary>
        /// Gets the underlying list of data rows for the data list object.
        /// </summary>
        /// <returns>A list of DataRow objects.</returns>
        public IList<DataRow> GetData() { return new ReadOnlyCollection<DataRow>(data); }

        /// <summary>
        /// The number of columns in the data list.
        /// </summary>
        public int ColumnCount { get; protected set; }

        /// <summary>
        /// The number of rows in the data list.
        /// </summary>
        public int RowCount { get { return data.Count; } }

        /// <summary>
        /// The current row index if set.
        /// </summary>
        private int currentRow = -1;

        /// <summary>
        /// Accessor for the current row index.
        /// </summary>
        public int CurrentRow { get { return currentRow; } set { currentRow = value < 0 ? -1 : value; } }

        /// <summary>
        /// A temporary variable to store a copy of a row before editing to allow cancelling edits.
        /// </summary>
        public DataRow EditRow { get; set; }

        /// <summary>
        /// Constructs a new data list object.
        /// </summary>
        public DataListObject()
        {
        }

        /// <summary>
        /// Additional initialization after all properties are constructed
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            foreach (DataProperty p in Properties) p.SetTableColumn(data, ColumnCount++);
        }

        /// <summary>
        /// Clears the data list.
        /// </summary>
        public void Clear()
        {
            data.Clear();
            FireCollectionChanged();
        }

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
            Sort(null);
        }

        /// <summary>
        /// Sorts the data object list according to the specified comparison function.
        /// </summary>
        public void Sort(Comparison<DataRow> cmp)
        {
            if (cmp == null) data.Sort();
            else data.Sort(cmp);
            FireCollectionChanged();
        }
        #endregion

        /// <summary>
        /// Occurs when the data in the list changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Tracks changes to the list of objects to update the modification state
        /// as well as the parent object on all objects that have been added or deleted.
        /// </summary>
        /// <param name="e">Collection change event arguments.</param>
        protected void FireCollectionChange(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null) CollectionChanged(this, e);
        }

        /// <summary>
        /// Fire a CollectionChanged event for the entire list
        /// </summary>
        public void FireCollectionChanged()
        {
            FireCollectionChange(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Insert a new data row at the specified index. The data row should have this list as a parent.
        /// </summary>
        /// <param name="index">Index at which to insert a new data row.</param>
        /// <param name="row">The data row to insert.</param>
        public void Insert(int index, DataRow row)
        {
            if (row.List != this || index < 0 || index > data.Count) return;
            data.Insert(index, row);
            FireCollectionChange(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, row, index));
        }

        /// <summary>
        /// Remove a data row at the specified index.
        /// </summary>
        /// <param name="index">Index to remove a data row at.</param>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= data.Count) return;

            DataRow removed = data[index];
            data.RemoveAt(index);
            FireCollectionChange(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed, index));
        }

        /// <summary>
        /// Populates the data object list and imports the data from the given data contract list.
        /// </summary>
        /// <param name="list">A list of data contract objects to populate the list from.</param>
        public virtual void FromDataContract(IEnumerable list)
        {
            if (list == null) return;
            data.Clear();
            Reset();
            SetModified(false, false);
            foreach (object contractItem in list)
            {
                data.Add(new DataRow(this));
                MoveNext();
                FromDataContract(contractItem);
            }
            FireCollectionChanged();
        }

        #region IEnumerable interfaces

        /// <summary>
        /// Resets the current enumerator and returns this object as an enumerator that iterates through the collection of data objects.
        /// </summary>
        /// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<DataObject> GetEnumerator()
        {
            Reset();
            return this;
        }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        /// <summary>
        /// Gets the current element in the collection.
        /// </summary>
        public DataObject Current { get { return new RowProxyObject(this, CurrentRow); } }

        /// <summary>
        /// Empty dispose method.
        /// </summary>
        public void Dispose() {}

        /// <summary>
        /// Gets the current untyped element in the collection.
        /// </summary>
        object IEnumerator.Current { get { return Current; } }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            if (++CurrentRow < RowCount) return true;
            return false;
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset() { CurrentRow = -1; }

        #endregion

        /// <summary>
        /// A proxy data object for a row in the list that is returned by the enumerator on the list object.
        /// </summary>
        public class RowProxyObject : DataObject
        {
            /// <summary>
            /// The data list object, for which this object is a proxy.
            /// </summary>
            public DataListObject List { get; private set; }

            /// <summary>
            /// The row index of the row in the data list object, for which this is a proxy.
            /// </summary>
            public int Row { get; private set; }

            /// <summary>
            /// Constructs a new data list row proxy object.
            /// </summary>
            /// <param name="list">The list from which to construct the row proxy object.</param>
            /// <param name="row">The row index.</param>
            public RowProxyObject(DataListObject list, int row)
            {
                this.List = list;
                this.Row = row;
            }

            /// <summary>
            /// Empty implemenation of the abstract Initalize method.
            /// </summary>
            protected override void Initialize()
            {
                // no-op for this abstract method
            }

            /// <summary>
            /// Overrides retrieval of data properties by name to return the corresponding property of the data list
            /// after setting its current row to this object's row.
            /// </summary>
            /// <param name="name">The name of the property.</param>
            /// <returns>A data property of the associated list object with the proper current row set.</returns>
            public override DataProperty this[string name]
            {
                get
                {
                    List.CurrentRow = Row;
                    return List[name];
                }
            }
        }
    }
}
