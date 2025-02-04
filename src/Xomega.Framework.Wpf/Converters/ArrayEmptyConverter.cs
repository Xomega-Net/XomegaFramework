// Copyright (c) 2023 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Globalization;
using System.Windows;

namespace Xomega.Framework.Wpf.Converters
{
    /// <summary>
    /// A value converter that can convert whether an array is empty to a boolean value.
    /// </summary>
    public class ArrayEmptyConverter : OneWayConverter
    {
        /// <summary>
        /// Visibility for null values.
        /// </summary>
        public Visibility Null { get; set; } = Visibility.Collapsed;

        /// <summary>
        /// Visibility for non-null values.
        /// </summary>
        public Visibility NonNull { get; set; } = Visibility.Visible;

        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is Array a ? a.Length == 0 :
               value is ICollection c ? c.Count == 0 : (bool?)null;
    }
}
