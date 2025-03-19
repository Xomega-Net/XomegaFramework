// Copyright (c) 2025 Xomega.Net. All rights reserved.

namespace Xomega.Framework.Services
{
    /// <summary>
    /// A class that represents a field to sort a sort direction.
    /// </summary>
    public class SortField
    {
        /// <summary>
        /// The field name to sort by.
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// The sort direction: ascending or descending.
        /// </summary>
        public Direction SortDirection { get; set; }

        /// <summary>
        /// A flag indicating whether the sort direction is descending.
        /// </summary>
        public bool IsDescending => SortDirection == Direction.Descending;

        /// <summary>
        /// A class that represents a sort direction.
        /// </summary>
        public enum Direction
        {
            /// <summary>
            /// A value that represents an ascending sort order.
            /// </summary>
            Ascending,

            /// <summary>
            /// A value that represents a descending sort order.
            /// </summary>
            Descending
        }
    }
}
