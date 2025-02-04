// Copyright (c) 2025 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Xomega.Framework.Properties
{
    /// <summary>
    /// A data property that holds a date and time as a value.
    /// If the date and time cannot be parsed it holds the raw input value,
    /// but will fail validation.
    /// </summary>
    public class DateTimeProperty : DataProperty<DateTime?>
    {
        /// <summary>
        /// Default format for displaying the date and time.
        /// </summary>
        public static string DefaultDateTimeFormat = DateTimeFormatInfo.CurrentInfo.ShortDatePattern + 
            " " + DateTimeFormatInfo.CurrentInfo.ShortTimePattern;

        /// <summary>
        /// The format for displaying the date and time for the current property.
        /// It can be configured externally by setting it to a new value.
        /// </summary>
        public string Format = DefaultDateTimeFormat;

        /// <summary>
        ///  Constructs a DateTimeProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public DateTimeProperty(DataObject parent, string name)
            : base(parent, name)
        {
            Validator += ValidateDateTime;
        }

        /// <inheritdoc/>>
        public override void CopyFrom(DataProperty p)
        {
            if (p is DateTimeProperty dtProperty)
            {
                Format = dtProperty.Format;
            }
            base.CopyFrom(p);
        }

        /// <summary>
        /// Overrides the base method to construct a list of non-Nullable DateTime values
        /// for the Transport format, since it's a value type.
        /// </summary>
        /// <param name="format">The format to create a new list for.</param>
        /// <returns>A new list for the given format.</returns>
        protected override IList CreateList(ValueFormat format)
        {
            return format == ValueFormat.Transport ? new List<DateTime>() : base.CreateList(format);
        }

        /// <summary>
        /// Converts a single value to a given format. For typed formats
        /// this method tries to convert various types of values to a nullable DateTime.
        /// For string formats it displays the internal DateTime formatted according
        /// to the specified <see cref="Format"/>.
        /// </summary>
        /// <param name="value">A single value to convert to the given format.</param>
        /// <param name="format">The value format to convert the value to.</param>
        /// <returns>The value converted to the given format.</returns>
        protected override object ConvertValue(object value, ValueFormat format)
        {
            if (format.IsTyped())
            {
                if (value is DateTime?) return value;
                if (value is DateTime) return (DateTime?)value;
                if (IsValueNull(value, format)) return null;
                DateTime dt;
                if (DateTime.TryParse(Convert.ToString(value), out dt)) return dt;
                if (format == ValueFormat.Transport) return null;
            }
            if (format.IsString())
            {
                DateTime? dt = value as DateTime?;
                if (dt == null && value is DateTime) dt = (DateTime)value;
                if (dt != null && dt.HasValue && !string.IsNullOrEmpty(Format))
                    return dt.Value.ToString(Format);
            }
            return base.ConvertValue(value, format);
        }

        /// <summary>
        /// A validation function that checks if the value is a DateTime and reports a validation error if not.
        /// </summary>
        /// <param name="dp">Data property being validated.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        public static void ValidateDateTime(DataProperty dp, object value, DataRow row)
        {
            DateTimeProperty dtp = dp as DateTimeProperty;
            if (dp != null && !dp.IsValueNull(value, ValueFormat.Internal) && !(value is DateTime))
                dp.AddValidationError(row, Messages.Validation_DateTimeFormat, dp, value, dtp != null ? dtp.Format : "N/A");
        }
    }

    /// <summary>
    /// A DateTimeProperty for the date part only.
    /// </summary>
    public class DateProperty : DateTimeProperty
    {
        /// <summary>
        /// Default format for displaying the date.
        /// </summary>
        public static string DefaultDateFormat = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;

        /// <summary>
        ///  Constructs a DateProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public DateProperty(DataObject parent, string name)
            : base(parent, name)
        {
            Format = DefaultDateFormat;
        }
    }

    /// <summary>
    /// A DateTimeProperty for the time part only.
    /// </summary>
    public class TimeProperty : DateTimeProperty
    {
        /// <summary>
        /// Default format for displaying the time.
        /// </summary>
        public static string DefaultTimeFormat = DateTimeFormatInfo.CurrentInfo.ShortTimePattern;

        /// <summary>
        /// A Boolean flag to control whether to treat a single integer under 24
        /// as minutes or hours. The default is to treat it as hours.
        /// </summary>
        public bool MinutesCentric = false;

        /// <summary>
        ///  Constructs a TimeProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public TimeProperty(DataObject parent, string name)
            : base(parent, name)
        {
            Format = DefaultTimeFormat;
        }

        /// <inheritdoc/>>
        public override void CopyFrom(DataProperty p)
        {
            if (p is TimeProperty timeProperty)
            {
                MinutesCentric = timeProperty.MinutesCentric;
            }
            base.CopyFrom(p);
        }

        /// <summary>
        /// Converts a single value to a given format. For typed formats
        /// this method tries to convert various types of values to a nullable DateTime.
        /// It also handles parsing strings that are input without a colon for speed entry (e.g. 1500).
        /// </summary>
        /// <param name="value">A single value to convert to the given format.</param>
        /// <param name="format">The value format to convert the value to.</param>
        /// <returns>The value converted to the given format.</returns>
        protected override object ConvertValue(object value, ValueFormat format)
        {
            if (format.IsTyped() && value is string)
            {
                string str = (string)value;
                int i;
                if (int.TryParse(str, out i) && i >= 0)
                {
                    if (i > 23 && i < 60 || i < 24 && MinutesCentric)
                        return new DateTime?(DateTime.Parse("0:" + i));
                    else if (i < 24) return new DateTime?(DateTime.Parse(i + ":0"));
                    else if (str.Length == 4)
                    {
                        DateTime dt;
                        if (DateTime.TryParse(str.Substring(0, 2) + ":" + str.Substring(2), out dt))
                            return new DateTime?(dt);
                    }
                }
            }
            return base.ConvertValue(value, format);
        }
    }
}
