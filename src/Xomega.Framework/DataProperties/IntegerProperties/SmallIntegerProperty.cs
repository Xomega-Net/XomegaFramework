// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System.Collections;
using System.Collections.Generic;

namespace Xomega.Framework.Properties
{
    /// <summary>
    /// An integer property for numbers in the range defined by the <c>short</c> CLR type.
    /// The <c>short</c> type is used for the transport value format.
    /// </summary>
    public class SmallIntegerProperty : IntegerProperty
    {
        /// <summary>
        ///  Constructs a SmallIntegerProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public SmallIntegerProperty(DataObject parent, string name)
            : base(parent, name)
        {
            MinimumValue = short.MinValue;
            MaximumValue = short.MaxValue;
        }

        /// <summary>
        /// Overrides the base method to construct a list of non-Nullable short values
        /// for the Transport format.
        /// </summary>
        /// <param name="format">The format to create a new list for.</param>
        /// <returns>A new list for the given format.</returns>
        protected override IList CreateList(ValueFormat format)
        {
            return format == ValueFormat.Transport ? new List<short>() : base.CreateList(format);
        }

        /// <summary>
        /// Converts a single value to a given format. For the transport format
        /// the value is converted to a <c>short</c> type.
        /// </summary>
        /// <param name="value">A single value to convert to the given format.</param>
        /// <param name="format">The value format to convert the value to.</param>
        /// <returns>The value converted to the given format.</returns>
        protected override object ConvertValue(object value, ValueFormat format)
        {
            if (format == ValueFormat.Transport && value is long?)
                return (short?)(long?)value;
            return base.ConvertValue(value, format);
        }
    }

    /// <summary>
    /// An small integer property for positive numbers only.
    /// </summary>
    public class PositiveSmallIntProperty : SmallIntegerProperty
    {
        /// <summary>
        ///  Constructs a PositiveSmallIntProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public PositiveSmallIntProperty(DataObject parent, string name)
            : base(parent, name)
        {
            MinimumValue = 1;
        }
    }

    /// <summary>
    /// A small integer key property to distinguish keys from regular integers. Unlike for regular integers, 
    /// comparison operators other than equality may not be applicable to integer keys.
    /// </summary>
    public class SmallIntegerKeyProperty : SmallIntegerProperty
    {
        /// <summary>
        ///  Constructs a SmallIntegerKeyProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>

        public SmallIntegerKeyProperty(DataObject parent, string name)
            : base(parent, name)
        {
        }
    }
}
