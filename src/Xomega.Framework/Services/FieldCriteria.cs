// Copyright (c) 2024 Xomega.Net. All rights reserved.

namespace Xomega.Framework.Services
{
    /// <summary>
    /// Typed values for a field criteria that are used as a filter in service operations.
    /// </summary>
    /// <typeparam name="T">The field type.</typeparam>
    public class FieldCriteria<T>
    {
        /// <summary>
        /// Operator to use for the criteria. The operator is required.
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// One or more values to use for the criteria as needed for the specified operator.
        /// </summary>
        public T[] Values { get; set; }
    }
}