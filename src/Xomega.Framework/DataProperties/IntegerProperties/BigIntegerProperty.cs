// Copyright (c) 2024 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Xomega.Framework.Properties
{
    /// <summary>
    /// A data property that holds big integer values (long).
    /// </summary>
    public class BigIntegerProperty : DataProperty<long?>
    {
        /// <summary>
        /// The minimum valid value for the property.
        /// </summary>
        public long MinimumValue { get; set; }

        /// <summary>
        /// The maximum valid value for the property.
        /// </summary>
        public long MaximumValue { get; set; }

        /// <summary>
        ///  Constructs a BigIntegerProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public BigIntegerProperty(DataObject parent, string name)
            : base(parent, name)
        {
            MinimumValue = long.MinValue;
            MaximumValue = long.MaxValue;

            Validator += ValidateLong;
            Validator += ValidateMinimum;
            Validator += ValidateMaximum;
        }

        /// <inheritdoc/>>
        public override void CopyFrom(DataProperty p)
        {
            if (p is BigIntegerProperty intProperty)
            {
                MinimumValue = intProperty.MinimumValue;
                MaximumValue = intProperty.MaximumValue;
            }
            base.CopyFrom(p);
        }

        /// <summary>
        /// Overrides the base method to construct a list of non-Nullable long values
        /// for the Transport format, since it's a value type.
        /// </summary>
        /// <param name="format">The format to create a new list for.</param>
        /// <returns>A new list for the given format.</returns>
        protected override IList CreateList(ValueFormat format)
        {
            return format == ValueFormat.Transport ? new List<long>() : base.CreateList(format);
        }

        /// <summary>
        /// Converts a single value to a given format. For typed formats
        /// this method tries to convert various types of values to a nullable long.
        /// </summary>
        /// <param name="value">A single value to convert to the given format.</param>
        /// <param name="format">The value format to convert the value to.</param>
        /// <returns>The value converted to the given format.</returns>
        protected override object ConvertValue(object value, ValueFormat format)
        {
            if (format.IsTyped())
            {
                if (value is long?) return value;
                if (value is long) return (long?)value;
                if (value is double d) return (long?)d;
                if (IsValueNull(value, format)) return null;
                if (long.TryParse(Convert.ToString(value), NumberStyles.Number, null, out long i)) return i;
                if (format == ValueFormat.Transport) return null;
            }
            return base.ConvertValue(value, format);
        }

        /// <summary>
        /// A validation function that checks if the value is a long and reports a validation error if not.
        /// </summary>
        /// <param name="dp">Data property being validated.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        public static void ValidateLong(DataProperty dp, object value, DataRow row)
        {
            if (dp != null && !dp.IsValueNull(value, ValueFormat.Internal)
                && !(value is long) && !(value is int) && !(value is short) && !(value is byte))
                dp.AddValidationError(row, Messages.Validation_IntegerFormat, dp);
        }

        /// <summary>
        /// A validation function that checks if the value is a long that is not less
        /// than the property minimum and reports a validation error if it is.
        /// </summary>
        /// <param name="dp">Data property being validated.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        public static void ValidateMinimum(DataProperty dp, object value, DataRow row)
        {
            if (dp is BigIntegerProperty idp && (value is long?) && ((long?)value).Value < idp.MinimumValue)
                dp.AddValidationError(row, Messages.Validation_NumberMinimum, dp, idp.MinimumValue);
        }

        /// <summary>
        /// A validation function that checks if the value is an integer that is not greater
        /// than the property maximum and reports a validation error if it is.
        /// </summary>
        /// <param name="dp">Data property being validated.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        public static void ValidateMaximum(DataProperty dp, object value, DataRow row)
        {
            if (dp is BigIntegerProperty idp && (value is long?) && ((long?)value).Value > idp.MaximumValue)
                dp.AddValidationError(row, Messages.Validation_NumberMaximum, dp, idp.MaximumValue);
        }
    }

    /// <summary>
    /// An big integer property for positive numbers only.
    /// </summary>
    public class PositiveBigIntProperty : BigIntegerProperty
    {
        /// <summary>
        ///  Constructs a PositiveBigIntProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public PositiveBigIntProperty(DataObject parent, string name)
            : base(parent, name)
        {
            MinimumValue = 1;
        }
    }

    /// <summary>
    /// An big integer key property to distinguish keys from regular big integers. Unlike for regular long values, 
    /// comparison operators other than equality may not be applicable to big integer keys.
    /// </summary>
    public class BigIntegerKeyProperty : BigIntegerProperty
    {
        /// <summary>
        ///  Constructs a BigIntKeyProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>

        public BigIntegerKeyProperty(DataObject parent, string name)
            : base(parent, name)
        {
        }
    }
}