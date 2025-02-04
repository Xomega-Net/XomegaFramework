// Copyright (c) 2025 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Xomega.Framework.Properties
{
    /// <summary>
    /// An integer property for numbers of the <c>byte</c> CLR type.
    /// </summary>
    public class TinyIntegerProperty : DataProperty<byte?>
    {
        /// <summary>
        /// The minimum valid value for the property.
        /// </summary>
        public byte MinimumValue { get; set; }

        /// <summary>
        /// The maximum valid value for the property.
        /// </summary>
        public byte MaximumValue { get; set; }

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

            Validator += ValidateByte;
            Validator += ValidateMinimum;
            Validator += ValidateMaximum;
        }

        /// <inheritdoc/>>
        public override void CopyFrom(DataProperty p)
        {
            if (p is TinyIntegerProperty intProperty)
            {
                MinimumValue = intProperty.MinimumValue;
                MaximumValue = intProperty.MaximumValue;
            }
            base.CopyFrom(p);
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
            if (format.IsTyped())
            {
                if (value is byte?) return value;
                if (value is byte) return (byte?)value;
                if (value is double d) return (byte?)d;
                if (IsValueNull(value, format)) return null;
                if (byte.TryParse(Convert.ToString(value), NumberStyles.Number, null, out byte i)) return i;
                if (format == ValueFormat.Transport) return null;
            }
            return base.ConvertValue(value, format);
        }

        /// <summary>
        /// A validation function that checks if the value is a byte and reports a validation error if not.
        /// </summary>
        /// <param name="dp">Data property being validated.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        public static void ValidateByte(DataProperty dp, object value, DataRow row)
        {
            if (dp != null && !dp.IsValueNull(value, ValueFormat.Internal) && !(value is byte))
                dp.AddValidationError(row, Messages.Validation_IntegerFormat, dp);
        }

        /// <summary>
        /// A validation function that checks if the value is a byte that is not less
        /// than the property minimum and reports a validation error if it is.
        /// </summary>
        /// <param name="dp">Data property being validated.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        public static void ValidateMinimum(DataProperty dp, object value, DataRow row)
        {
            if (dp is TinyIntegerProperty idp && (value is byte?) && ((byte?)value).Value < idp.MinimumValue)
                dp.AddValidationError(row, Messages.Validation_NumberMinimum, dp, idp.MinimumValue);
        }

        /// <summary>
        /// A validation function that checks if the value is a byte that is not greater
        /// than the property maximum and reports a validation error if it is.
        /// </summary>
        /// <param name="dp">Data property being validated.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        public static void ValidateMaximum(DataProperty dp, object value, DataRow row)
        {
            if (dp is TinyIntegerProperty idp && (value is byte?) && ((byte?)value).Value > idp.MaximumValue)
                dp.AddValidationError(row, Messages.Validation_NumberMaximum, dp, idp.MaximumValue);
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
