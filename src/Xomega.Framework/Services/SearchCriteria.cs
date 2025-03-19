// Copyright (c) 2025 Xomega.Net. All rights reserved.

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Base class for search criteria structures
    /// </summary>
    public class SearchCriteria
    {
        /// <summary>
        /// The number of records to skip before returning the results.
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// The number of records to return.
        /// </summary>
        public int? Take { get; set; }

        /// <summary>
        /// A flag indicating whether to get the total count of records that match the criteria.
        /// </summary>
        public bool GetTotalCount { get; set; }

        /// <summary>
        /// The fields to sort by.
        /// </summary>
        public SortField[] Sort { get; set; }

        /// <summary>
        /// Return a copy of this search criteria.
        /// </summary>
        /// <returns>Copy of current search criteria.</returns>
        public virtual SearchCriteria Clone() => new SearchCriteria
        {
            Skip = Skip,
            Take = Take,
            GetTotalCount = GetTotalCount,
            Sort = Sort
        };

        /// <summary>
        /// Sets the search criteria from another instance of the same class.
        /// </summary>
        /// <param name="other">Search criteria object to copy values from.</param>
        public virtual void SetFrom(SearchCriteria other)
        {
            if (other == null) return;
            Skip = other.Skip;
            Take = other.Take;
            GetTotalCount = other.GetTotalCount;
            Sort = other.Sort;
        }
    }
}