// Copyright (c) 2025 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Xomega.Framework.Properties
{
    /// <summary>
    /// A subtype of enumeration properties where the header IDs are always Guids.
    /// It uses the <c>Guid</c> type for the transport format.
    /// </summary>
    public class EnumGuidProperty : EnumProperty
    {
        /// <summary>
        ///  Constructs an EnumGuidProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public EnumGuidProperty(DataObject parent, string name)
            : base(parent, name)
        {
        }

        /// <summary>
        /// Overrides the base method to construct a list of Guid values for the Transport format.
        /// </summary>
        /// <param name="format">The format to create a new list for.</param>
        /// <returns>A new list for the given format.</returns>
        protected override IList CreateList(ValueFormat format)
        {
            return format == ValueFormat.Transport ? new List<Guid>() : base.CreateList(format);
        }

        /// <summary>
        /// Converts a single value to a given format.
        /// For the transport format it uses the header ID converted as a Guid.
        /// </summary>
        /// <param name="value">A single value to convert to the given format.</param>
        /// <param name="format">The value format to convert the value to.</param>
        /// <returns>The value converted to the given format.</returns>
        protected override object ConvertValue(object value, ValueFormat format)
        {
            if (format == ValueFormat.Transport)
            {
                Header h = value as Header;
                if (h != null && Guid.TryParse(h.Id, out Guid id)) return id;
                else if (value != null && Guid.TryParse(value.ToString(), out id)) return id;
                else return null;
            }
            return base.ConvertValue(value, format);
        }
    }
}
