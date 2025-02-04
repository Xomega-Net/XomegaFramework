// Copyright (c) 2025 Xomega.Net. All rights reserved.

using System;
using System.Globalization;
using System.Windows;

namespace Xomega.Framework.Wpf.Converters
{
    /// <summary>
    /// A value converter that can convert a null value to visibility to hide a controls for blank properties.
    /// </summary>
    public class NullToVisibilityConverter : OneWayConverter
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
            => value == null ? Null : NonNull;
    }
}
