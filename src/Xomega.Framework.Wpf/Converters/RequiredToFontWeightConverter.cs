// Copyright (c) 2019 Xomega.Net. All rights reserved.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Xomega.Framework.Converters
{
    /// <summary>
    /// A value converter that can convert a boolean value of the Required Xomega property
    /// to a corresponding font weight to enable marking required fields with bold labels.
    /// </summary>
    public class RequiredToFontWeightConverter : IValueConverter
    {
        /// <summary>
        /// Font weight for the required fields, e.g. Bold.
        /// </summary>
        public FontWeight Required { get; set; }

        /// <summary>
        /// Font weight for non-required fields, e.g. Normal.
        /// </summary>
        public FontWeight Optional { get; set; }

        /// <summary>
        /// Converts the required boolean value to a font weight.
        /// </summary>
        /// <param name="value">The required boolean value.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The <see cref="Required"/> property for required value, otherwise the <see cref="Optional"/> property.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value) return Required;
            else return Optional;
        }

        /// <summary>
        /// Returns the value as is. No conversion back is supported.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The value passed in as is.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
