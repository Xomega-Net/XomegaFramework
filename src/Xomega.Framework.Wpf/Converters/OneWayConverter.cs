// Copyright (c) 2023 Xomega.Net. All rights reserved.

using System;
using System.Globalization;
using System.Windows.Data;

namespace Xomega.Framework.Wpf.Converters
{
    /// <summary>
    /// An abstract base class for one-way value converters.
    /// </summary>
    public abstract class OneWayConverter : IValueConverter
    {
        /// <inhheritdoc/>
        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        /// <summary>
        /// Returns the value as is. No conversion back is supported.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The value passed in as is.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    }
}
