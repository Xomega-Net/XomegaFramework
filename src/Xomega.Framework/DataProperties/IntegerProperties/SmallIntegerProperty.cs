// Copyright (c) 2023 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Xomega.Framework.Properties
{
    /// <summary>
    /// An integer property for numbers of the <c>short</c> CLR type.
    /// </summary>
    public class SmallIntegerProperty : DataProperty<short?>
    {
        /// <summary>
        /// The minimum valid value for the property.
        /// </summary>
        public short MinimumValue { get; set; }

        /// <summary>
        /// The maximum valid value for the property.
        /// </summary>
        public short MaximumValue { get; set; }

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

            Validator += ValidateShort;
            Validator += ValidateMinimum;
            Validator += ValidateMaximum;
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
        /// Converts a single value to a given format. For typed formats
        /// this method tries to convert various types of values to a nullable short.
        /// </summary>
        /// <param name="value">A single value to convert to the given format.</param>
        /// <param name="format">The value format to convert the value to.</param>
        /// <returns>The value converted to the given format.</returns>
        protected override object ConvertValue(object value, ValueFormat format)
        {
            if (format.IsTyped())
            {
                if (value is short?) return value;
                if (value is short) return (short?)value;
                if (value is double d) return (short?)d;
                if (IsValueNull(value, format)) return null;
                if (short.TryParse(Convert.ToString(value), NumberStyles.Number, null, out short i)) return i;
                if (format == ValueFormat.Transport) return null;
            }
            return base.ConvertValue(value, format);
        }

        /// <summary>
        /// A validation function that checks if the value is a short and reports a validation error if not.
        /// </summary>
        /// <param name="dp">Data property being validated.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        public static void ValidateShort(DataProperty dp, object value, DataRow row)
        {
            if (dp != null && !dp.IsValueNull(value, ValueFormat.Internal)
                && !(value is short) && !(value is byte))
                dp.AddValidationError(row, Messages.Validation_IntegerFormat, dp);
        }

        /// <summary>
        /// A validation function that checks if the value is a short that is not less
        /// than the property minimum and reports a validation error if it is.
        /// </summary>
        /// <param name="dp">Data property being validated.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        public static void ValidateMinimum(DataProperty dp, object value, DataRow row)
        {
            if (dp is SmallIntegerProperty idp && (value is short?) && ((short?)value).Value < idp.MinimumValue)
                dp.AddValidationError(row, Messages.Validation_NumberMinimum, dp, idp.MinimumValue);
        }

        /// <summary>
        /// A validation function that checks if the value is a short that is not greater
        /// than the property maximum and reports a validation error if it is.
        /// </summary>
        /// <param name="dp">Data property being validated.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        public static void ValidateMaximum(DataProperty dp, object value, DataRow row)
        {
            if (dp is SmallIntegerProperty idp && (value is short?) && ((short?)value).Value > idp.MaximumValue)
                dp.AddValidationError(row, Messages.Validation_NumberMaximum, dp, idp.MaximumValue);
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
