// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Xomega.Framework.Properties
{
    /// <summary>
    /// A subtype of enumeration properties where the items represent boolean values.
    /// It uses the <c>bool</c> type for the transport format.
    /// </summary>
    public class EnumBoolProperty : EnumProperty
    {
        /// <summary>
        ///  Constructs an EnumBoolProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public EnumBoolProperty(DataObject parent, string name)
            : base(parent, name)
        {
        }

        /// <summary>
        /// Overrides the base method to construct a list of boolean values for the Transport format.
        /// </summary>
        /// <param name="format">The format to create a new list for.</param>
        /// <returns>A new list for the given format.</returns>
        protected override IList CreateList(ValueFormat format)
        {
            return format == ValueFormat.Transport ? new List<bool>() : base.CreateList(format);
        }

        /// <summary>
        /// Converts a single value to a given format.
        /// For the transport format it uses the header ID converted to a boolean.
        /// </summary>
        /// <param name="value">A single value to convert to the given format.</param>
        /// <param name="format">The value format to convert the value to.</param>
        /// <returns>The value converted to the given format.</returns>
        protected override object ConvertValue(object value, ValueFormat format)
        {
            Header h = value as Header;
            if (format == ValueFormat.Transport && h != null)
            {
                if (BooleanProperty.TrueStrings.Contains(h.Id)) return true;
                if (BooleanProperty.FalseStrings.Contains(h.Id)) return false;
            }
            else if (format == ValueFormat.Internal && value is bool)
                return base.ConvertValue(value.ToString().ToLower(), format);
            return base.ConvertValue(value, format);
        }
    }
}
