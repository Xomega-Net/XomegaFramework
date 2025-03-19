// Copyright (c) 2025 Xomega.Net. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Xomega.Framework.Services;

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

        /// <summary>
        /// Converts the list sort criteria to the sort criteria for service calls.
        /// </summary>
        /// <returns></returns>
        public SortField[] ToSortFields()
            => this.Select(f => f.ToSortField()).ToArray();
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

        /// <summary>
        /// Converts the sort field to the sort field for service calls.
        /// </summary>
        /// <returns>A sort field for service calls.</returns>
        public SortField ToSortField()
            => new SortField { FieldName = PropertyName, SortDirection = SortDirection.ToSortDirection() };
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

        /// <summary>
        /// Converts the list sort direction to the sort direction for services.
        /// </summary>
        /// <returns></returns>
        public SortField.Direction ToSortDirection()
            => this == Descending ? SortField.Direction.Descending : SortField.Direction.Ascending;
    }
}
