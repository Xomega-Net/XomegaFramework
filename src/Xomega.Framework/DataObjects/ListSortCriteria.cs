// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System.Collections.Generic;

namespace Xomega.Framework
{
    /// <summary>
    /// A class that represents sort criteria for a list.
    /// It can also be used independently as data row comparer.
    /// </summary>
    public class ListSortCriteria : List<ListSortField>, IComparer<DataRow>
    {
        /// <summary>
        /// Constructs blank sort criteria.
        /// </summary>
        public ListSortCriteria() { }

        /// <summary>
        /// Constructs sort criteria from the specified list of fields.
        /// </summary>
        public ListSortCriteria(IEnumerable<ListSortField> fields) : base(fields) { }

        /// <summary>
        /// Implementation of the <see cref="IComparer{T}"/> interface.
        /// </summary>
        /// <param name="x">The row to compare.</param>
        /// <param name="y">The row to compare to.</param>
        /// <returns>The integer result of comparison of the two rows.</returns>
        public int Compare(DataRow x, DataRow y) => x?.CompareTo(y, this) ?? 0;
    }

    /// <summary>
    /// A class that represents an individual sort field with a property name and a sort direction.
    /// </summary>
    public class ListSortField
    {
        /// <summary>
        /// The property name to sort by.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The sort direction: ascending or descending.
        /// </summary>
        public ListSortDirection SortDirection { get; set; }
    }

    /// <summary>
    /// A class that represents a sort direction.
    /// </summary>
    public class ListSortDirection
    {
        /// <summary>
        /// A static value that represents an ascending sort order.
        /// </summary>
        public static readonly ListSortDirection Ascending = new ListSortDirection();

        /// <summary>
        /// A static value that represents a descending sort order.
        /// </summary>
        public static readonly ListSortDirection Descending = new ListSortDirection();

        /// <summary>
        /// A nonpublic constructor.
        /// </summary>
        protected ListSortDirection() { }

        /// <summary>
        /// A static method that toggles the given sort direction.
        /// </summary>
        /// <param name="direction">The sort direction to toggle.</param>
        /// <returns>A toggled sort direction.</returns>
        public static ListSortDirection Toggle(ListSortDirection direction)
        {
            return direction == Ascending ? Descending : Ascending;
        }

        /// <summary>
        /// Constructs sort direction from common strings.
        /// </summary>
        /// <param name="dir">A string representing a sort direction.</param>
        /// <returns>Specific instance of the list sort direction.</returns>
        public static ListSortDirection FromString(string dir)
            => dir != null && dir.ToUpper().StartsWith("D") ? Descending : Ascending;
    }
}
