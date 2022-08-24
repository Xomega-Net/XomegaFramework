// Copyright (c) 2022 Xomega.Net. All rights reserved.

using System.Collections;
using System.Collections.Generic;

namespace Xomega.Framework.Properties
{
    /// <summary>
    /// An integer property for numbers in the range defined by the <c>byte</c> CLR type.
    /// The <c>byte</c> type is used for the transport value format.
    /// </summary>
    public class TinyIntegerProperty : IntegerProperty
    {
        /// <summary>
        ///  Constructs a TinyIntegerProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>

        public TinyIntegerProperty(DataObject parent, string name)
            : base(parent, name)
        {
            MinimumValue = byte.MinValue;
            MaximumValue = byte.MaxValue;
        }

        /// <summary>
        /// Overrides the base method to construct a list of non-Nullable byte values
        /// for the Transport format.
        /// </summary>
        /// <param name="format">The format to create a new list for.</param>
        /// <returns>A new list for the given format.</returns>
        protected override IList CreateList(ValueFormat format)
        {
            return format == ValueFormat.Transport ? new List<byte>() : base.CreateList(format);
        }

        /// <summary>
        /// Converts a single value to a given format. For the transport format
        /// the value is converted to a <c>byte</c> type.
        /// </summary>
        /// <param name="value">A single value to convert to the given format.</param>
        /// <param name="format">The value format to convert the value to.</param>
        /// <returns>The value converted to the given format.</returns>
        protected override object ConvertValue(object value, ValueFormat format)
        {
            if (format == ValueFormat.Transport && value is long?)
                return (byte?)(long?)value;
            return base.ConvertValue(value, format);
        }
    }

    /// <summary>
    /// A tiny integer key property to distinguish keys from regular integers. Unlike for regular integers, 
    /// comparison operators other than equality may not be applicable to integer keys.
    /// </summary>
    public class TinyIntegerKeyProperty : TinyIntegerProperty
    {
        /// <summary>
        ///  Constructs a TinyIntegerKeyProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>

        public TinyIntegerKeyProperty(DataObject parent, string name)
            : base(parent, name)
        {
        }
    }
}
