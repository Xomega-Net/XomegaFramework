// Copyright (c) 2017 Xomega.Net. All rights reserved.

namespace Xomega.Framework
{
    /// <summary>
    /// Enumeration for different security access levels, which can be associated with properties,
    /// data objects or other elements that require security.
    /// The access level enumeration constants are listed in the ascending order, so that they can be compared
    /// using the standard 'greater than', 'less than' and 'equals' operators.
    /// </summary>
    public enum AccessLevel
    {
        /// <summary>
        /// The constant indicating no access to the given element.
        /// The user can neither view nor modify the element.
        /// </summary>
        None,

        /// <summary>
        /// The constant indicating view/read only access to the given element.
        /// The user can view the element, but not modify it.
        /// </summary>
        ReadOnly,

        /// <summary>
        /// The constant indicating full access to the given element.
        /// The user can both view and modify the element.
        /// </summary>
        Full
    }
}
