// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Xomega.Framework.Properties
{
    /// <summary>
    /// A data property that holds decimal values.
    /// The CLR type for those will be in Nullable decimal.
    /// </summary>
    public class DecimalProperty : DataProperty<decimal?>
    {
        /// <summary>
        /// The minimum valid value for the property.
        /// </summary>
        public decimal MinimumValue { get; set; }

        /// <summary>
        /// The maximum valid value for the property.
        /// </summary>
        public decimal MaximumValue { get; set; }

        /// <summary>
        /// A combination of styles for parsing the decimal number.
        /// </summary>
        public NumberStyles ParseStyles { get; set; }

        /// <summary>
        /// The format for displaying the number as a string.
        /// </summary>
        public string DisplayFormat { get; set; }

        /// <summary>
        ///  Constructs a DecimalProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public DecimalProperty(DataObject parent, string name)
            : base(parent, name)
        {
            MinimumValue = decimal.MinValue;
            MaximumValue = decimal.MaxValue;
            ParseStyles = NumberStyles.Number;

            Validator += ValidateDecimal;
            Validator += ValidateMinimum;
            Validator += ValidateMaximum;
        }

        /// <summary>
        /// Overrides the base method to construct a list of non-Nullable decimal values
        /// for the Transport format, since it's a value type.
        /// </summary>
        /// <param name="format">The format to create a new list for.</param>
        /// <returns>A new list for the given format.</returns>
        protected override IList CreateList(ValueFormat format)
        {
            return format == ValueFormat.Transport ? new List<decimal>() : base.CreateList(format);
        }

        /// <summary>
        /// Converts a single value to a given format. For typed formats
        /// this method tries to convert various types of values to a nullable decimal.
        /// For string formats it displays the internal decimal formatted according
        /// to the specified <see cref="DisplayFormat"/> if set.
        /// </summary>
        /// <param name="value">A single value to convert to the given format.</param>
        /// <param name="format">The value format to convert the value to.</param>
        /// <returns>The value converted to the given format.</returns>
        protected override object ConvertValue(object value, ValueFormat format)
        {
            if (format.IsTyped())
            {
                if (value is decimal?) return value;
                if (value is decimal) return (decimal?)value;
                if (value is int) return (decimal?)((int)value);
                if (IsValueNull(value, format)) return null;
                decimal d;
                if (decimal.TryParse(Convert.ToString(value), ParseStyles, null, out d)) return d;
                if (format == ValueFormat.Transport) return null;
            }
            if (format == ValueFormat.DisplayString && value is decimal? && !IsValueNull(value, format))
                return ((decimal?)value).Value.ToString(DisplayFormat);
            return base.ConvertValue(value, format);
        }

        /// <summary>
        /// A validation function that checks if the value is a decimal and reports a validation error if not.
        /// </summary>
        /// <param name="dp">Data property being validated.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        public static void ValidateDecimal(DataProperty dp, object value, DataRow row)
        {
            if (dp != null && !dp.IsValueNull(value, ValueFormat.Internal) && !(value is decimal))
                dp.AddValidationError(row, Messages.Validation_DecimalFormat, dp);
        }

        /// <summary>
        /// A validation function that checks if the value is a decimal that is not less
        /// than the property minimum and reports a validation error if it is.
        /// </summary>
        /// <param name="dp">Data property being validated.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        public static void ValidateMinimum(DataProperty dp, object value, DataRow row)
        {
            if (dp is DecimalProperty ddp && (value is decimal?) && ((decimal?)value).Value < ddp.MinimumValue)
                dp.AddValidationError(row, Messages.Validation_NumberMinimum, dp, ddp.MinimumValue);
        }

        /// <summary>
        /// A validation function that checks if the value is a decimal that is not greater
        /// than the property maximum and reports a validation error if it is.
        /// </summary>
        /// <param name="dp">Data property being validated.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        public static void ValidateMaximum(DataProperty dp, object value, DataRow row)
        {
            if (dp is DecimalProperty ddp && (value is decimal?) && ((decimal?)value).Value > ddp.MaximumValue)
                dp.AddValidationError(row, Messages.Validation_NumberMaximum, dp, ddp.MaximumValue);
        }
    }

    /// <summary>
    /// A decimal property for positive numbers only.
    /// </summary>
    public class PositiveDecimalProperty : DecimalProperty
    {
        /// <summary>
        ///  Constructs a PositiveDecimalProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public PositiveDecimalProperty(DataObject parent, string name)
            : base(parent, name)
        {
            MinimumValue = 0;
        }
    }
}
