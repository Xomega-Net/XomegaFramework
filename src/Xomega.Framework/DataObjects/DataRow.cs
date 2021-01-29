// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Xomega.Framework
{
    /// <summary>
    /// Data row is a class that is used to store data for individual rows in the <see cref="DataListObject"/>.
    /// It provides ability to compare rows using the list's sort criteria and some other utility functions.
    /// </summary>
    public class DataRow : DynamicObject, IComparable<DataRow>
    {
        #region Utility methods for getting property values from a data row

        /// <summary>
        /// Gets internal value of the specified property from the given data row.
        /// </summary>
        /// <param name="dataItem">data row to get the data from.</param>
        /// <param name="property">proprety name to retrieve.</param>
        public static object GetValue(object dataItem, string property)
        {
            return Get(dataItem, property, ValueFormat.Internal);
        }

        /// <summary>
        /// Gets a string value of the specified property from the given data row.
        /// </summary>
        /// <param name="dataItem">data row to get the data from.</param>
        /// <param name="property">proprety name to retrieve.</param>
        public static string GetString(object dataItem, string property)
        {
            object val = Get(dataItem, property, ValueFormat.DisplayString);
            return val != null ? val.ToString() : "";
        }

        /// <summary>
        /// Gets a string value of the specified property from the given data row.
        /// </summary>
        /// <param name="dataItem">data row to get the data from.</param>
        /// <param name="property">proprety name to retrieve.</param>
        public static string GetEditString(object dataItem, string property)
        {
            object val = Get(dataItem, property, ValueFormat.EditString);
            return val != null ? val.ToString() : "";
        }

        /// <summary>
        /// Gets a value of the specified property from the given data row.
        /// </summary>
        /// <param name="dataItem">data row to get the data from.</param>
        /// <param name="property">proprety name to retrieve.</param>
        /// <param name="format">value format to return.</param>
        /// <returns></returns>
        public static object Get(object dataItem, string property, ValueFormat format)
        {
            var dr = dataItem as DataRow;
            return dr?.List[property]?.GetValue(format, dr);
        }
        #endregion

        /// <summary>
        /// The parent data list object for this data row.
        /// </summary>
        public DataListObject List { get; private set; }

        /// <summary>
        /// The data for the row
        /// </summary>
        private readonly List<object> data = new List<object>();

        /// <summary>
        /// A flag indicating if the current row is selected
        /// </summary>
        public bool Selected { get; set; }

        /// <summary>
        /// Dummy parameterless constructor to satisfy creation by Activator.
        /// </summary>
        public DataRow() {}

        /// <summary>
        /// Constructs a new data row for the specified data list object.
        /// </summary>
        /// <param name="dataList">Data list object that contains this row.</param>
        public DataRow(DataListObject dataList)
        {
            List = dataList;
            for (int i = 0; i < dataList.ColumnCount; i++) data.Add(null); // pre-create columns
        }

        #region Edit row

        /// <summary>
        /// Constructs a new edit row for the specified data row.
        /// </summary>
        public DataRow(DataRow orignalRow) : this(orignalRow.List)
        {
            OriginalRow = orignalRow;
            CopyFrom(orignalRow);
        }

        /// <summary>
        /// The original data row this row is editing, if any.
        /// </summary>
        public DataRow OriginalRow { get; private set; }

        #endregion

        /// <summary>
        /// Internal access to the row data by column index for properties.
        /// </summary>
        /// <param name="column">The column index.</param>
        /// <returns>The row's value at the specified column.</returns>
        internal object this[int column]
        {
            get => column >= 0 && column < data.Count ? data[column] : null;
            set { if (column >= 0 && column < data.Count) data[column] = value; }
        }

        /// <summary>
        /// Copy of a row from another row.
        /// </summary>
        /// <param name="otherRow">Another row to copy from.</param>
        public void CopyFrom(DataRow otherRow)
        {
            for (int i = 0; i < otherRow.data.Count; i++) data[i] = otherRow.data[i];
        }

        #region Dynamic object

        /// <inheritdoc/>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            if (List == null) return new string[0];
            return List.Properties.Select(p => p.Name);
        }

        /// <inheritdoc/>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var prop = List?[binder.Name];
            if (prop != null)
            {
                result = prop.GetValue(ValueFormat.Internal, this);
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        /// <inheritdoc/>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var prop = List?[binder.Name];
            if (prop != null)
            {
                prop.SetValue(value, this);
                return true;
            }
            return base.TrySetMember(binder, value);
        }

        #endregion

        #region Validation

        private Dictionary<string, ErrorList> ValidationErrors = new Dictionary<string, ErrorList>();

        /// <summary>
        /// Adds the specified validation error with arguments to the data row.
        /// </summary>
        /// <param name="property">Invalid data property, or null if the error is not property-specific.</param>
        /// <param name="error">The error code. Null value indicates the validation happened and succeeded.</param>
        /// <param name="args">Error message parameters, if any.</param>
        internal void AddValidationError(DataProperty property, string error, params object[] args)
        {
            string key = property?.Name ?? "";
            if (!ValidationErrors.TryGetValue(key, out ErrorList errorList))
                ValidationErrors[key] = errorList = List.NewErrorList();
            if (error != null)
                errorList.AddValidationError(error, args);
        }

        /// <summary>
        /// Resets the validation errors for this row for the specified property, or for the entire row.
        /// </summary>
        /// <param name="property">The property to reset validation for, or null to reset for the entire row.</param>
        internal void ResetValidation(DataProperty property)
        {
            if (property == null) ValidationErrors.Clear();
            else ValidationErrors.Remove(property.Name);
        }

        /// <summary>
        /// Gets validation errors for a specific property in the data row, or non-property specific errors,
        /// if the specified property is null.
        /// </summary>
        /// <param name="property">The property to get validation errors for, or null for row-level errors.</param>
        /// <returns>Null, if the property or row were never validated. Otherwise the a list of validation errors
        /// for the specified property or for the entire row.</returns>
        internal ErrorList GetValidationErrors(DataProperty property)
        {
            string key = property?.Name ?? "";
            if (ValidationErrors.TryGetValue(key, out ErrorList errorList))
                return errorList;
            else return null;
        }

        #endregion

        #region Row comparison

        /// <summary>
        /// Implementation of the IComparable interface for DataRow classes. Compares this row
        /// with the other row provided using the sort criteria of the parent list object.
        /// </summary>
        /// <param name="other">The other data row to compare this row to.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects
        /// being compared. The return value has the following meanings: Value Meaning
        /// Less than zero This object is less than the other parameter. Zero This object
        /// is equal to other. Greater than zero This object is greater than other.</returns>
        public int CompareTo(DataRow other)
        {
            return CompareTo(other, List?.SortCriteria);
        }

        /// <summary>
        /// Compares this row with the other row provided using specified sort criteria.
        /// </summary>
        /// <param name="other">The other data row to compare this row to.</param>
        /// <param name="criteria">Sort criteria to use.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects
        /// being compared. The return value has the following meanings: Value Meaning
        /// Less than zero This object is less than the other parameter. Zero This object
        /// is equal to other. Greater than zero This object is greater than other.</returns>
        public int CompareTo(DataRow other, ListSortCriteria criteria)
        {
            int res = 0;

            if (criteria == null || other == null || List != other.List) return res;

            foreach (ListSortField sortFld in criteria)
            {
                DataProperty p = List[sortFld.PropertyName];
                if (p != null)
                {
                    object val1 = p.GetValue(ValueFormat.Internal, this);
                    object val2 = p.GetValue(ValueFormat.Internal, other);
                    if (val1 == val2) res = 0;
                    else if (val1 == null && val2 != null) res = -1;
                    else if (val1 != null && val2 == null) res = 1;
                    else if (val1 is IComparable) res = ((IComparable)val1).CompareTo(val2);
                    else if (val2 is IComparable) res = -((IComparable)val2).CompareTo(val1);
                    else res = string.Compare(
                        "" + p.ResolveValue(val1, ValueFormat.DisplayString),
                        "" + p.ResolveValue(val2, ValueFormat.DisplayString));
                    if (sortFld.SortDirection == ListSortDirection.Descending) res *= -1;
                }
                if (res != 0) return res;
            }
            return res;
        }

        #endregion
    }
}
