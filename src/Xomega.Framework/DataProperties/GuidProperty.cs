// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Xomega.Framework.Properties
{
    /// <summary>
    /// A data property that holds GUID type of values
    /// that can serve as globally unique identifiers.
    /// </summary>
    public class GuidProperty : DataProperty<Guid?>
    {
        /// <summary>
        ///  Constructs a GuidProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public GuidProperty(DataObject parent, string name)
            : base(parent, name)
        {
        }

        /// <summary>
        /// Overrides the base method to construct a list of non-nullable Guid values
        /// for the Transport format, since it's a value type.
        /// </summary>
        /// <param name="format">The format to create a new list for.</param>
        /// <returns>A new list for the given format.</returns>
        protected override IList CreateList(ValueFormat format)
        {
            return format == ValueFormat.Transport ? new List<Guid>() : base.CreateList(format);
        }

        /// <summary>
        /// Converts a single value to a given format. For typed formats
        /// this method tries to convert various types of values to a nullable Guid.
        /// </summary>
        /// <param name="value">A single value to convert to the given format.</param>
        /// <param name="format">The value format to convert the value to.</param>
        /// <returns>The value converted to the given format.</returns>
        protected override object ConvertValue(object value, ValueFormat format)
        {
            if (format.IsTyped())
            {
                if (value is Guid?) return value;
                if (value is Guid) return (Guid?)value;
                if (IsValueNull(value, format)) return null;
                try
                {
                    return new Guid?(new Guid(Convert.ToString(value)));
                }
                catch (Exception) { }
            }
            return base.ConvertValue(value, format);
        }
    }
}
