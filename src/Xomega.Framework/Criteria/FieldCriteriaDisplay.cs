// Copyright (c) 2025 Xomega.Net. All rights reserved.

namespace Xomega.Framework.Criteria
{
    /// <summary>
    /// A structure to hold the values for a field criteria used for display purposes.
    /// </summary>
    public struct FieldCriteriaDisplay
    {
        /// <summary>
        /// The internal field name.
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// The localized field label to display.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The operator value to display.
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// Localized value for the "and" connector.
        /// </summary>
        public string And { get; set; }

        /// <summary>
        /// Array of display values for the criteria.
        /// For single-value criteria the array will have one element.
        /// For range values the array will have two elements - from and to.
        /// For multi-valued criteria the array will have multiple elements.
        /// </summary>
        public string[] Value { get; set; }
    }
}