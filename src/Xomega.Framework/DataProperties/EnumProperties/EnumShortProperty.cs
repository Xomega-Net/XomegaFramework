// Copyright (c) 2016 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Xomega.Framework.Properties
{
    /// <summary>
    /// A subtype of enumeration properties where the header IDs are always short integers.
    /// It uses the <c>short</c> type for the transport format.
    /// </summary>
    public class EnumShortProperty : EnumProperty
    {
        /// <summary>
        ///  Constructs an EnumShortProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public EnumShortProperty(DataObject parent, string name)
            : base(parent, name)
        {
        }

        /// <summary>
        /// Overrides the base method to construct a list of short values for the Transport format.
        /// </summary>
        /// <param name="format">The format to create a new list for.</param>
        /// <returns>A new list for the given format.</returns>
        protected override IList CreateList(ValueFormat format)
        {
            return format == ValueFormat.Transport ? new List<short>() : base.CreateList(format);
        }

        /// <summary>
        /// Converts a single value to a given format.
        /// For the transport format it uses the header ID converted as an short integer.
        /// </summary>
        /// <param name="value">A single value to convert to the given format.</param>
        /// <param name="format">The value format to convert the value to.</param>
        /// <returns>The value converted to the given format.</returns>
        protected override object ConvertValue(object value, ValueFormat format)
        {
            Header h = value as Header;
            if (format == ValueFormat.Transport && h != null)
            {
                short id;
                if (short.TryParse(h.Id, out id)) return id;
            }
            return base.ConvertValue(value, format);
        }
    }
}
