// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xomega.Framework.Operators;

namespace Xomega.Framework
{
    /// <summary>
    /// A dynamic data object that has a list of rows as its data instead of specific values.
    /// </summary>
    public abstract class DataListObject : DataObject, INotifyCollectionChanged
    {
        #region List data and edit data

        /// <summary>
        /// The data table for the list stored as an array of arrays
        /// </summary>
        protected DataRowCollection data = new DataRowCollection();

        /// <summary>
        /// Gets the underlying list of data rows for the data list object.
        /// </summary>
        /// <returns>A list of DataRow objects.</returns>
        public IList<DataRow> GetData() { return new ReadOnlyObservableCollection<DataRow>(data); }

        /// <summary>
        /// The number of columns in the data list.
        /// </summary>
        public int ColumnCount { get; private set; }

        /// <summary>
        /// The number of rows in the data list.
        /// </summary>
        public int RowCount { get { return data.Count; } }

        /// <summary>
        /// A temporary variable to store a copy of a row before editing to allow canceling edits.
        /// </summary>
        public DataRow EditRow { get; set; }

        #endregion

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
            SearchAction = new ActionProperty(this, Messages.Action_Search);
            SelectAction = new ActionProperty(this, Messages.Action_Select);

            Expression<Func<DataListObject, bool>> xSel = lst => lst.SelectedRows.Any();
            SelectAction.SetComputedEnabled(xSel, this);

            foreach (DataProperty p in Properties) p.Column = ColumnCount++;
            data.CollectionChanged += (s, e) =>
            {
                FireCollectionChange(e);
                if (e.Action == NotifyCollectionChangedAction.Reset ||
                    e.OldItems != null && e.OldItems.Cast<DataRow>().Any(r => r.Selected) ||
                    e.NewItems.Cast<DataRow>().Any(r => r.Selected))
                {
                    FireSelectionChanged();
                }
            };
        }

        /// <summary>
        /// Clears the data list.
        /// </summary>
        public void Clear()
        {
            AppliedCriteria = null;
            data.Clear();
        }

        /// <summary>
        /// Overrides base method to reset the list by clearing all data
        /// </summary>
        public override void ResetData()
        {
            Clear();
            modified = null;
        }

        /// <summary>
        /// The search action associated with this list object,
        /// which allows controlling the state of the button bound to it.
        /// </summary>
        public ActionProperty SearchAction { get; private set; }

        #endregion

        #region Filtering

        /// <summary>
        /// A filtering function that determines if the value of the specified property in the given row
        /// matches the specified criteria value using supplied operator.
        /// </summary>
        /// <param name="property">The data property of the object to match.</param>
        /// <param name="row">The data row to get the value from.</param>
        /// <param name="oper">Comparison operator to use.</param>
        /// <param name="criteria">The value to compare to.</param>
        /// <param name="caseSensitive">True to perform case-sensitive string matching, false otherwise.</param>
        /// <returns>True if the property value in the given row matches the specified criteria, false otherwise.</returns>
        public virtual bool PropertyValueMatches(DataProperty property, DataRow row, Operator oper, object criteria, bool caseSensitive)
        {
            ValueFormat format = criteria?.GetType() == typeof(string) ? ValueFormat.DisplayString : ValueFormat.Internal;
            object value = property.GetValue(format, row);
            criteria = property.ResolveValue(criteria, format);
            if (format.IsString() && !caseSensitive)
            {
                value = value?.ToString()?.ToLower();
                criteria = criteria?.ToString()?.ToLower();
            }
            return oper.Matches(value, criteria);
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
        public void Sort(Comparison<DataRow> cmp) => data.Sort(cmp);

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
        public string RowSelectionMode
        {
            get { return rowSelectionMode; }
            set
            {
                string oldValue = rowSelectionMode;
                rowSelectionMode = value;
                if (!string.Equals(oldValue, value))
                    FireSelectionChanged();
            }
        }

        // internal selection mode
        private string rowSelectionMode;

        /// <summary>
        /// Occurs when selection has been changed
        /// </summary>
        public event EventHandler SelectionChanged;

        /// <summary>
        /// Fires SelectionChanged event
        /// </summary>
        public void FireSelectionChanged()
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

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
            FireSelectionChanged();
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
        /// A list of currently selected data rows.
        /// </summary>
        public List<DataRow> SelectedRows
        {
            get => data.Where(r => r.Selected).ToList();
            set
            {
                data.ToList().ForEach(r => r.Selected = false);
                if (value != null) value.ForEach(r => r.Selected = true);
                FireSelectionChanged();
            }
        }

        /// <summary>
        /// Checks if the data row with a specified index is selected.
        /// </summary>
        /// <param name="idx">Index of the row to check</param>
        /// <returns>True if the row is selected, false otherwise</returns>
        public bool IsRowSelected(int idx) => (idx >= 0 && idx < data.Count) && data[idx].Selected;

        /// <summary>
        /// Single-select data row with a specified index.
        /// </summary>
        /// <param name="idx">Index of the row to select</param>
        public void SelectRow(int idx) => SelectRows(idx, idx, true);

        /// <summary>
        /// Select a single data row in the list, and clear selection on all other rows.
        /// </summary>
        /// <param name="row">The row to select.</param>
        public void SelectRow(DataRow row) => SelectedRows = new[] { row }.ToList();

        /// <summary>
        /// Select all data rows.
        /// </summary>
        public void SelectAllRows() => SelectRows(0, data.Count - 1, false);

        /// <summary>
        /// Clear selection for all data rows.
        /// </summary>
        public void ClearSelectedRows() => SelectRows(0, -1, true);

        /// <summary>
        /// Toggles selection for the specified row.
        /// </summary>
        /// <param name="row">The row index.</param>
        public void ToggleSelection(DataRow row) => SetRowSelected(row, !row.Selected);

        /// <summary>
        /// Sets selection on the specified row.
        /// </summary>
        /// <param name="row">The row to set selection on.</param>
        /// <param name="selected">True to set the row selected, false otherwise.</param>
        public void SetRowSelected(DataRow row, bool selected)
        {
            if (!row.Selected && RowSelectionMode == SelectionModeSingle)
                ClearSelectedRows();
            row.Selected = selected;
            FireSelectionChanged();
        }

        /// <summary>
        /// Select action for the list object that can be bound to a select button.
        /// By default select action is enabled only when the list has any row selected.
        /// </summary>
        public ActionProperty SelectAction { get; private set; }

        #endregion

        #region Applied criteria

        /// <summary>
        /// Criteria object associated with the current list object
        /// </summary>
        public CriteriaObject CriteriaObject { get; set; }

        private List<FieldCriteriaSetting> appliedCriteria;

        /// <summary>
        /// List of applied criteria settings associated with the data.
        /// </summary>
        public List<FieldCriteriaSetting> AppliedCriteria
        {
            get { return appliedCriteria; }
            set
            {
                appliedCriteria = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(AppliedCriteria)));
            }
        }

        /// <summary>
        /// Get the summary of applied criteria as one string,
        /// e.g. for tooltip when it's too long to fit on the screen.
        /// </summary>
        /// <returns>Applied criteria summary as text.</returns>
        public virtual string GetAppliedCriteriaText()
        {
            string text = "";
            if (appliedCriteria == null || appliedCriteria.Count == 0)
                return text;
            foreach(var fc in appliedCriteria)
            {
                if (text.Length > 0) text += "; ";
                text += fc.Label + ":" + (string.IsNullOrEmpty(fc.Operator) ? "" : " " + fc.Operator) + 
                    (fc.Value != null && fc.Value.Length > 0 ? " " + string.Join(" and ", fc.Value) : "");
            }
            return text;
        }

        #endregion

        #region Validation

        /// <summary>
        /// Gets all validation errors from the data list object and the criteria object, if any.
        /// </summary>
        /// <returns>Validation errors from the data list object and the criteria object.</returns>
        public override ErrorList GetValidationErrors()
        {
            ErrorList errLst = NewErrorList();
            errLst.MergeWith(validationErrorList);
            errLst.MergeWith(CriteriaObject?.GetValidationErrors());
            return errLst;
        }

        /// <summary>
        /// Resets validation status to not validated on the object and the criteria object, if any.
        /// </summary>
        public override void ResetAllValidation()
        {
            ResetValidation();
            if (CriteriaObject != null) CriteriaObject.ResetAllValidation();
            foreach (DataRow row in data) row.ResetValidation(null);
        }

        /// <summary>
        /// Validates the data list object and the criteria object, if any.
        /// </summary>
        /// <param name="force">True to validate regardless of whether or not it has been already validated.</param>
        public override void Validate(bool force)
        {
            if (CriteriaObject != null) CriteriaObject.Validate(force);

            if (force) ResetValidation();
            if (validationErrorList != null) return;

            validationErrorList = NewErrorList();
        }

        #endregion

        #region Collection modification

        /// <summary>
        /// Occurs when the data in the list changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Indicator that collection change event is in progress
        /// </summary>
        public bool CollectionChangeFiring { get; private set; }

        /// <summary>
        /// Tracks changes to the list of objects to update the modification state
        /// as well as the parent object on all objects that have been added or deleted.
        /// </summary>
        /// <param name="e">Collection change event arguments.</param>
        public void FireCollectionChange(NotifyCollectionChangedEventArgs e)
        {
            CollectionChangeFiring = true;
            CollectionChanged?.Invoke(this, e);
            CollectionChangeFiring = false;
        }

        /// <summary>
        /// Insert a new data row at the specified index. The data row should have this list as a parent.
        /// </summary>
        /// <param name="index">Index at which to insert a new data row.</param>
        /// <param name="row">The data row to insert.</param>
        /// <param name="suppressNotification">True to suppress notifications and not mark as modified,
        /// which the caller needs to handle on their own, if desired. Default is false.</param>
        public virtual void Insert(int index, DataRow row, bool suppressNotification = false)
        {
            if (row.List != this || index < 0 || index > data.Count) return;
            if (suppressNotification)
            {
                data.suppressNotification = true;
                data.Insert(index, row);
                data.suppressNotification = false;
            }
            else
            {
                data.Insert(index, row);
                SetModified(true, false);
            }
        }

        /// <summary>
        /// Insert a new data row at the specified index. The data row should have this list as a parent.
        /// </summary>
        /// <param name="index">Index at which to insert a new data row.</param>
        /// <param name="row">The data row to insert.</param>
        /// <param name="suppressNotification">True to suppress notifications and not mark as modified,
        /// which the caller needs to handle on their own, if desired. Default is false.</param>
        public virtual async Task InsertAsync(int index, DataRow row, bool suppressNotification = false)
        {
            Insert(index, row, suppressNotification);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Remove a data row at the specified index.
        /// </summary>
        /// <param name="index">Index to remove a data row at.</param>
        public virtual void RemoveAt(int index)
        {
            if (index < 0 || index >= data.Count) return;
            SetModified(true, false);
            data.RemoveAt(index);
        }

        /// <summary>
        /// Remove the specified data rows from the list.
        /// </summary>
        /// <param name="rows">A list of data rows to remove.</param>
        /// <param name="suppressNotification">True to suppress notifications and not mark as modified,
        /// which the caller needs to handle on their own, if desired. Default is false.</param>
        public virtual async Task RemoveRows(IEnumerable<DataRow> rows, bool suppressNotification = false)
        {
            if (suppressNotification)
            {
                data.suppressNotification = true;
                data.RemoveRows(rows.Where(r => r.List == this));
                data.suppressNotification = false;
            }
            else
            {
                data.RemoveRows(rows.Where(r => r.List == this));
                SetModified(true, false);
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Replaces data in the specified row with data in the new row.
        /// </summary>
        /// <param name="row">The row to replace.</param>
        /// <param name="newRow">The new row to use.</param>
        /// <param name="suppressNotification">True to suppress notifications and not mark as modified,
        /// which the caller needs to handle on their own, if desired. Default is false.</param>
        public virtual async Task UpdateRow(DataRow row, DataRow newRow, bool suppressNotification = false)
        {
            if (suppressNotification)
            {
                data.suppressNotification = true;
                await data.ReplaceRowAsync(newRow, row);
                data.suppressNotification = false;
            }
            else
            {
                await data.ReplaceRowAsync(newRow, row);
                SetModified(true, false);
            }
        }

        /// <summary>
        /// Overrides the base method to check modification state of the list's rows
        /// rather than properties or child objects.
        /// </summary>
        /// <returns>Modification state of the list object.</returns>
        public override bool? IsModified()
        {
            bool? res = modified;
            foreach (var row in data)
            {
                var rowmod = row.IsModified();
                if (rowmod.HasValue) res |= rowmod;
            }
            return !TrackModifications && res != null ? false : res;
        }

        /// <summary>
        /// Overrides the base method to set modification state of the list's rows
        /// rather than properties or child objects.
        /// </summary>
        public override void SetModified(bool? modified, bool recursive)
        {
            this.modified = modified;
            if (recursive)
            {
                foreach (var row in data) row.SetModified(modified);
            }
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Modified)));
        }

        #endregion

        #region Data Contract support

        /// <summary>
        /// Populates the data object list and imports the data from the given data contract list
        /// with ability to preserve currently selected entities.
        /// </summary>
        /// <param name="dataContract">Enumeration of data contract objects to populate the list from.</param>
        /// <param name="options">Additional options for the operation.</param>
        public override void FromDataContract(object dataContract, object options)
        {
            if (!(dataContract is IEnumerable list)) return;
            List<DataRow> sel = new List<DataRow>();
            ListSortCriteria keys = new ListSortCriteria();
            if (options is CrudOpions opts && opts.PreserveSelection)
            {
                sel = SelectedRows;
                keys.AddRange(Properties.Where(p => p.IsKey).Select(p => new ListSortField() { PropertyName = p.Name }));
            }

            List<DataRow> rows = new List<DataRow>();
            foreach (object contractItem in list)
            {
                DataRow r = new DataRow(this);
                rows.Add(r);
                FromDataContract(contractItem, options, r);
                r.Selected = sel.Any(s => SameEntity(s, r, keys));
            }
            SetModified(false, false);
            data.ReplaceData(rows);
            if (CriteriaObject != null) AppliedCriteria = CriteriaObject.GetFieldCriteriaSettings();
        }

        /// <summary>
        /// Populates the data object list and imports the data from the given data contract list
        /// with ability to preserve currently selected entities.
        /// </summary>
        /// <param name="dataContract">Enumeration of data contract objects to populate the list from.</param>
        /// <param name="options">Additional options for the operation.</param>
        /// <param name="token">Cancellation token.</param>
        public override async Task FromDataContractAsync(object dataContract, object options, CancellationToken token = default)
        {
            if (!(dataContract is IEnumerable list)) return;
            List<DataRow> sel = new List<DataRow>();
            ListSortCriteria keys = new ListSortCriteria();
            if (options is CrudOpions opts && opts.PreserveSelection)
            {
                sel = SelectedRows;
                keys.AddRange(Properties.Where(p => p.IsKey).Select(p => new ListSortField() { PropertyName = p.Name }));
            }

            List<DataRow> rows = new List<DataRow>();
            foreach (object contractItem in list)
            {
                DataRow r = new DataRow(this);
                rows.Add(r);
                await FromDataContractAsync(contractItem, options, r, token);
                r.Selected = sel.Any(s => SameEntity(s, r, keys));
            }
            SetModified(false, false);
            data.ReplaceData(rows);
            if (CriteriaObject != null) AppliedCriteria = CriteriaObject.GetFieldCriteriaSettings();
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
            return r1 != null && keys != null && keys.Count != 0 && r1.CompareTo(r2, keys) == 0;
        }

        /// <summary>
        /// Exports the data from the data object list to the list of data contract objects.
        /// </summary>
        /// <param name="obj">The list of data contract objects to export the data to.
        /// The list should be generic with a single type parameter.</param>
        /// <param name="options">Additional options for the operation.</param>
        public override void ToDataContract(object obj, object options)
        {
            if (!(obj is IList list)) return;
            Type listType = list.GetType();
            if (!listType.IsGenericType || listType.GetGenericArguments().Length != 1) return;

            Type contractItemType = list.GetType().GetGenericArguments()[0];
            foreach (DataRow row in data)
            {
                object item = Activator.CreateInstance(contractItemType);
                ToDataContractProperties(item, contractItemType.GetProperties(), options, row);
                list.Add(item);
            }
        }

        #endregion

        #region DataRowCollection

        /// <summary>
        /// Observable collection of data rows that allows bulk updates and sorting.
        /// </summary>
        public class DataRowCollection : ObservableCollection<DataRow>
        {
            internal bool suppressNotification;

            /// <inheritdoc/>
            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                if (!suppressNotification)
                    base.OnCollectionChanged(e);
            }

            /// <summary>
            /// Replaces collection data with the new data provided with a single collection change notification.
            /// </summary>
            /// <param name="source">The new data to replace the current list data with.</param>
            public void ReplaceData(IEnumerable<DataRow> source)
            {
                suppressNotification = true;
                Clear();
                foreach (var row in source) Add(row);
                suppressNotification = false;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

            /// <summary>
            /// Replaces data in the original row with the data from the specified row.
            /// </summary>
            /// <param name="editRow">The edit row to replace the original row.</param>
            /// <param name="originalRow">The original row to replace, or null to use the original row from the specified row.</param>
            public async Task ReplaceRowAsync(DataRow editRow, DataRow originalRow = null)
            {
                var row = originalRow ?? editRow?.OriginalRow;
                if (row == null || editRow == null) return;
                await row.CopyFromAsync(editRow);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, editRow, row));
            }

            /// <summary>
            /// Remove the specified data rows from the collection with a single change notification.
            /// </summary>
            /// <param name="rows">A list of data rows to remove.</param>
            public void RemoveRows(IEnumerable<DataRow> rows)
            {
                suppressNotification = true;
                foreach (var row in rows)
                    Remove(row);
                suppressNotification = false;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, rows.ToList()));
            }

            /// <summary>
            /// Sorts the current collection of rows using the specified comparison function.
            /// If no comparison function is specified, sorts the rows based on the sort criteria
            /// of the associated data list object.
            /// </summary>
            /// <param name="comparison">The comparison function to use.</param>
            public void Sort(Comparison<DataRow> comparison)
            {
                List<DataRow> rows = new List<DataRow>(this);
                if (comparison == null) rows.Sort();
                else rows.Sort(comparison);
                ReplaceData(rows);
            }
        }

        #endregion
    }
}
