// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;

namespace Xomega.Framework
{
    /// <summary>
    /// Data row is a class that is used to store data for individual rows in the <see cref="DataListObject"/>.
    /// It provides ability to compare rows using the list's sort criteria and some other utility functions.
    /// </summary>
    public class DataRow : List<object>, IComparable<DataRow>
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
            DataRow dr = dataItem as DataRow;
            if (dr == null) return null;
            DataProperty dp = dr.List[property];
            if (dp == null || dp.Column < 0 || dp.Column >= dr.Count) return null;
            return dp.ResolveValue(dr[dp.Column], format);
        }
        #endregion

        /// <summary>
        /// The parent data list object for this data row.
        /// </summary>
        public DataListObject List { get; private set; }

        /// <summary>
        /// A flag indicating if the current row is selected
        /// </summary>
        public bool Selected { get; internal set; }

        /// <summary>
        /// Constructs a new data row for the specified data list object.
        /// </summary>
        /// <param name="dataList">Data list object that contains this row.</param>
        public DataRow(DataListObject dataList)
        {
            this.List = dataList;
            for (int i = 0; i < dataList.ColumnCount; i++) Add(null); // pre-create columns
        }

        /// <summary>
        /// Copy of a row from another row.
        /// </summary>
        /// <param name="otherRow">Another row to copy from.</param>
        public void CopyFrom(DataRow otherRow)
        {
            for (int i = 0; i < otherRow.Count; i++) this[i] = otherRow[i];
        }

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
            return CompareTo(other, List == null ? null : List.SortCriteria);
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
                    object val1 = this[p.Column];
                    object val2 = other[p.Column];
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
    }
}
