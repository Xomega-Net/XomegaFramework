// Copyright (c) 2023 Xomega.Net. All rights reserved.

using System;
using System.Globalization;
using System.Windows;

namespace Xomega.Framework.Wpf.Converters
{
    /// <summary>
    /// A value converter that can convert a boolean value of the Required Xomega property
    /// to a corresponding font weight to enable marking required fields with bold labels.
    /// </summary>
    public class RequiredToFontWeightConverter : OneWayConverter
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
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool bval && bval ? Required : (object)Optional;
    }
}
