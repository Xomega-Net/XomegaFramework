// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Xomega.Framework.Properties
{
    /// <summary>
    /// A data property that holds integer values.
    /// </summary>
    public class IntegerProperty : DataProperty<int?>
    {
        /// <summary>
        /// The minimum valid value for the property.
        /// </summary>
        public int MinimumValue { get; set; }

        /// <summary>
        /// The maximum valid value for the property.
        /// </summary>
        public int MaximumValue { get; set; }

        /// <summary>
        ///  Constructs an IntegerProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public IntegerProperty(DataObject parent, string name)
            : base(parent, name)
        {
            MinimumValue = int.MinValue;
            MaximumValue = int.MaxValue;

            Validator += ValidateInteger;
            Validator += ValidateMinimum;
            Validator += ValidateMaximum;
        }

        /// <summary>
        /// Overrides the base method to construct a list of non-Nullable integer values
        /// for the Transport format, since it's a value type.
        /// </summary>
        /// <param name="format">The format to create a new list for.</param>
        /// <returns>A new list for the given format.</returns>
        protected override IList CreateList(ValueFormat format)
        {
            return format == ValueFormat.Transport ? new List<int>() : base.CreateList(format);
        }

        /// <summary>
        /// Converts a single value to a given format. For typed formats
        /// this method tries to convert various types of values to a nullable integer.
        /// </summary>
        /// <param name="value">A single value to convert to the given format.</param>
        /// <param name="format">The value format to convert the value to.</param>
        /// <returns>The value converted to the given format.</returns>
        protected override object ConvertValue(object value, ValueFormat format)
        {
            if (format.IsTyped())
            {
                if (value is int?) return value;
                if (value is int) return (int?)value;
                if (value is double) return (int?)((double)value);
                if (IsValueNull(value, format)) return null;
                int i;
                if (int.TryParse(Convert.ToString(value), NumberStyles.Number, null, out i)) return i;
                if (format == ValueFormat.Transport) return null;
            }
            return base.ConvertValue(value, format);
        }

        /// <summary>
        /// A validation function that checks if the value is an integer and reports a validation error if not.
        /// </summary>
        /// <param name="dp">Data property being validated.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        public static void ValidateInteger(DataProperty dp, object value, DataRow row)
        {
            if (dp != null && !dp.IsValueNull(value, ValueFormat.Internal)
                && !(value is int) && !(value is short) && !(value is byte))
                dp.AddValidationError(row, Messages.Validation_IntegerFormat, dp);
        }

        /// <summary>
        /// A validation function that checks if the value is an integer that is not less
        /// than the property minimum and reports a validation error if it is.
        /// </summary>
        /// <param name="dp">Data property being validated.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        public static void ValidateMinimum(DataProperty dp, object value, DataRow row)
        {
            if (dp is IntegerProperty idp && (value is int?) && ((int?)value).Value < idp.MinimumValue)
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
            if (dp is IntegerProperty idp && (value is int?) && ((int?)value).Value > idp.MaximumValue)
                dp.AddValidationError(row, Messages.Validation_NumberMaximum, dp, idp.MaximumValue);
        }
    }

    /// <summary>
    /// An integer property for positive numbers only.
    /// </summary>
    public class PositiveIntegerProperty : IntegerProperty
    {
        /// <summary>
        ///  Constructs a PositiveIntegerProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public PositiveIntegerProperty(DataObject parent, string name)
            : base(parent, name)
        {
            MinimumValue = 1;
        }
    }

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
            if (format == ValueFormat.Transport && value is int?)
                return (short?)(int?)value;
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
            if (format == ValueFormat.Transport && value is int?)
                return (byte?)(int?)value;
            return base.ConvertValue(value, format);
        }
    }

    /// <summary>
    /// An integer key property to distinguish keys from regular integers. Unlike for regular integers 
    /// comparison operators other than equality may not be applicable to integer keys.
    /// </summary>
    public class IntegerKeyProperty : IntegerProperty
    {
        /// <summary>
        ///  Constructs a IntegerKeyProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>

        public IntegerKeyProperty(DataObject parent, string name)
            : base(parent, name)
        {
        }
    }
}
