// Copyright (c) 2023 Xomega.Net. All rights reserved.

using System;

namespace Xomega.Framework.Properties
{
    /// <summary>
    /// A data property that has a string value. The maximum length of the string
    /// can be specified by setting the <see cref="DataProperty.Size"/> on the data property.
    /// </summary>
    public class TextProperty : DataProperty<string>
    {
        /// <summary>
        ///  Constructs a TextProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public TextProperty(DataObject parent, string name)
            : base(parent, name)
        {
            Validator += ValidateSize;
        }

        /// <summary>
        /// Converts a single value to a given format, which is always a string.
        /// </summary>
        /// <param name="value">A single value to convert to the given format.</param>
        /// <param name="format">The value format to convert the value to.</param>
        /// <returns>The value converted to the given format.</returns>
        protected override object ConvertValue(object value, ValueFormat format)
        {
            if (format.IsTyped())
            {
                return Convert.ToString(value);
            }
            return base.ConvertValue(value, format);
        }

        /// <summary>
        /// A validation function that checks if the value length is not greater
        /// than the property size and reports a validation error if it is.
        /// </summary>
        /// <param name="dp">Data property being validated.</param>
        /// <param name="value">The value to validate.</param>
        /// <param name="row">The row in a list object or null for regular data objects.</param>
        public static void ValidateSize(DataProperty dp, object value, DataRow row)
        {
            if (dp != null && dp.Size > 0 && Convert.ToString(value).Length > dp.Size)
                dp.AddValidationError(row, Messages.Validation_MaxLength, dp, dp.Size, value);
        }
    }
}
