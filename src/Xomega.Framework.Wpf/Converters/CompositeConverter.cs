// Copyright (c) 2023 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xomega.Framework.Wpf.Converters
{
    /// <summary>
    /// A value converter that allows chaining multiple converters.
    /// </summary>
    [ContentProperty("Converters")]
    public class CompositeConverter : OneWayConverter
    {
        /// <summary>
        /// The list of converters to apply.
        /// </summary>
        public List<IValueConverter> Converters { get; } = new List<IValueConverter>();

        /// <inheritdoc/>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue)
                return value;

            foreach (var converter in Converters)
            {
                value = converter.Convert(value, targetType, parameter, culture);
                if (value == System.Windows.Data.Binding.DoNothing)
                    return System.Windows.Data.Binding.DoNothing;
            }
            return value;
        }
    }
}
