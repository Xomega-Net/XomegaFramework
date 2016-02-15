// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System.Collections;
using System.Collections.Generic;

namespace Xomega.Framework
{
    /// <summary>
    /// A class that represents sort criteria for a list.
    /// </summary>
    public class ListSortCriteria : List<ListSortField>
    {
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
    }
}
