// Copyright (c) 2020 Xomega.Net. All rights reserved.

namespace Xomega.Framework.Lookup
{
    /// <summary>
    /// Enumeration of different types of the lookup value validation.
    /// </summary>
    public enum LookupValidationType
    {
        /// <summary>
        /// The value should be an active item of the enum.
        /// </summary>
        ActiveItem,

        /// <summary>
        /// The value should be any item of the enum (default).
        /// </summary>
        AnyItem,

        /// <summary>
        /// The value is not validated against the lookup table.
        /// </summary>
        None
    }
}