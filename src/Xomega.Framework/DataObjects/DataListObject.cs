// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

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
        /// List of applied criteria settings associated with the data.
        /// </summary>
        public List<FieldCriteriaSetting> AppliedCriteria { get; set; }

        #region Construction and initialization

        /// <summary>
        /// Constructs a new data list object.
        /// </summary>
        public DataListObject()
        {
        }

        /// <summary>
        /// Constructs a new data list object with a service provider
        /// </summary>
        public DataListObject(IServiceProvider serviceProvider) : base(serviceProvider)
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
            AppliedCriteria = null;
            data.Clear();
            FireCollectionChanged();
        }

        /// <summary>
        /// Overrides base method to reset the list by clearing all data
        /// </summary>
        public override void ResetData()
        {
            Clear();
            modified = null;
        }

        #endregion

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

        #region Selection support

        /// <summary>
        /// Data list supports single selection
        /// </summary>
        public const string SelectionModeSingle = "single";

        /// <summary>
        /// Data list supports multiple selection
        /// </summary>
        public const string SelectionModeMultiple = "multiple";

        /// <summary>
        /// Current selection mode for data list rows. Null means selection is not supported
        /// </summary>
        public string RowSelectionMode { get; set; }

        /// <summary>
        /// Selects data rows specified by the provided start and end indexes.
        /// </summary>
        /// <param name="startIdx">Start row index to select</param>
        /// <param name="endIdx">End row index to select</param>
        /// <param name="clearOthers">True to clear selection for rows not in the specified range, false otherwise</param>
        /// <returns>True if selection was allowed, false otherwise</returns>
        public bool SelectRows(int startIdx, int endIdx, bool clearOthers)
        {
            int count = endIdx - startIdx + 1;
            if (RowSelectionMode == SelectionModeSingle && count > 1) return false;
            if (count > 0)
                data.Skip(startIdx).Take(count).ToList().ForEach(r => r.Selected = true);
            if (clearOthers || RowSelectionMode == SelectionModeSingle)
            {
                data.Take(startIdx).ToList().ForEach(r => r.Selected = false);
                data.Skip(endIdx + 1).ToList().ForEach(r => r.Selected = false);
            }
            return true;
        }

        /// <summary>
        /// A list of currently selected data row indexes
        /// </summary>
        public List<int> SelectedRowIndexes
        {
            get
            {
                List<int> res = new List<int>();
                for (int i=0; i < data.Count; i++)
                    if (data[i].Selected) res.Add(i);
                return res;
            }
        }

        /// <summary>
        /// A list of currently selected data rows
        /// </summary>
        public List<DataRow> SelectedRows { get { return data.Where(r => r.Selected).ToList(); } }

        /// <summary>
        /// Single-select data row with a specified index
        /// </summary>
        /// <param name="idx">Index of the row to select</param>
        /// <returns>True if selection was allowed, false otherwise</returns>
        public bool SelectRow(int idx)
        {
            return SelectRows(idx, idx, true);
        }

        /// <summary>
        /// Select all data rows
        /// </summary>
        public void SelectAllRows()
        {
            SelectRows(0, data.Count - 1, false);
        }

        /// <summary>
        /// Clear selection for all data rows
        /// </summary>
        public void ClearSelectedRows()
        {
            SelectRows(0, -1, true);
        }
        #endregion

        #region Collection modification

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

        #endregion

        #region Data Contract support

        /// <summary>
        /// Populates the data object list and imports the data from the given data contract list.
        /// </summary>
        /// <param name="list">A list of data contract objects to populate the list from.</param>
        public virtual void FromDataContract(IEnumerable list)
        {
            FromDataContract(list, false);
        }

        /// <summary>
        /// Populates the data object list and imports the data from the given data contract list
        /// with ability to preserve currently selected entities.
        /// </summary>
        /// <param name="list">A list of data contract objects to populate the list from.</param>
        /// <param name="preserveSelection">A flag indicating whether or not to preserve selection.</param>
        public virtual void FromDataContract(IEnumerable list, bool preserveSelection)
        {
            if (list == null) return;
            List<DataRow> sel = new List<DataRow>();
            ListSortCriteria keys = new ListSortCriteria();
            if (preserveSelection)
            {
                sel = SelectedRows;
                keys.AddRange(Properties.Where(p => p.IsKey).Select(p => new ListSortField() { PropertyName = p.Name }));
            }
            data.Clear();
            Reset();
            SetModified(false, false);
            foreach (object contractItem in list)
            {
                DataRow r = new DataRow(this);
                data.Add(r);
                MoveNext();
                FromDataContract(contractItem);
                r.Selected = sel.Any(s => SameEntity(s, r, keys));
            }
            FireCollectionChanged();
        }

        /// <summary>
        /// Checks if two data rows represent the same entity. Can be overridden in subclasses.
        /// </summary>
        /// <param name="r1">First row</param>
        /// <param name="r2">Second row</param>
        /// <param name="keys">Sort criteria with key property names</param>
        /// <returns>True, if the two rows represent the same entity, false otherwise.</returns>
        protected virtual bool SameEntity(DataRow r1, DataRow r2, ListSortCriteria keys)
        {
            return r1 == null || keys == null || keys.Count == 0 ? false : r1.CompareTo(r2, keys) == 0;
        }

        /// <summary>
        /// Exports the data from the data object list to the list of data contract objects.
        /// </summary>
        /// <param name="obj">The list of data contract objects to export the data to.
        /// The list should be generic with a single type parameter.</param>
        public override void ToDataContract(Object obj)
        {
            IList list = obj as IList;
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

        #endregion

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

        #region RowProxyObject

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

        #endregion
    }
}
