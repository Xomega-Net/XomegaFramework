// Copyright (c) 2023 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Xomega.Framework.Properties
{
    /// <summary>
    /// A data property that holds Boolean values.
    /// </summary>
    public class BooleanProperty : DataProperty<bool?>
    {
        /// <summary>
        /// An array of strings that should be parsed as a true Boolean value.
        /// To default values are: "true", "1", "yes".
        /// It can also be set externally for a more precise control over this behavior.
        /// </summary>
        public static string[] TrueStrings = { "true", "1", "yes" };

        /// <summary>
        /// An array of strings that should be parsed as a false Boolean value.
        /// To default values are: "false", "0", "no".
        /// It can also be set externally for a more precise control over this behavior.
        /// </summary>
        public static string[] FalseStrings = { "false", "0", "no" };

        /// <summary>
        ///  Constructs a BooleanProperty with a given name and adds it to the parent data object under this name.
        /// </summary>
        /// <param name="parent">The parent data object to add the property to if applicable.</param>
        /// <param name="name">The property name that should be unique within the parent data object.</param>
        public BooleanProperty(DataObject parent, string name)
            : base(parent, name)
        {
        }

        /// <summary>
        /// Overrides the base method to construct a list of non-Nullable Boolean values
        /// for the Transport format, since it's a value type.
        /// </summary>
        /// <param name="format">The format to create a new list for.</param>
        /// <returns>A new list for the given format.</returns>
        protected override IList CreateList(ValueFormat format)
        {
            return format == ValueFormat.Transport ? new List<bool>() : base.CreateList(format);
        }

        /// <summary>
        /// Converts a single value to a given format. For typed formats
        /// this method tries to convert various types of values to a nullable Boolean
        /// and may utilize lists of strings that represent true or false values
        /// (see <see cref="TrueStrings"/> and <see cref="FalseStrings"/>).
        /// </summary>
        /// <param name="value">A single value to convert to the given format.</param>
        /// <param name="format">The value format to convert the value to.</param>
        /// <returns>The value converted to the given format.</returns>
        protected override object ConvertValue(object value, ValueFormat format)
        {
            if (format.IsTyped())
            {
                if (value is bool?) return value;
                if (value is bool) return (bool?)value;
                if (1.Equals(value)) return true;
                if (0.Equals(value)) return false;
                if (IsValueNull(value, format)) return null;
                string str = Convert.ToString(value).Trim().ToLower();
                if (TrueStrings.Contains(str)) return true;
                if (FalseStrings.Contains(str)) return false;
                if (format == ValueFormat.Transport) return false;
            }
            return base.ConvertValue(value, format);
        }
    }
}
