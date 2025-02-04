// Copyright (c) 2023 Xomega.Net. All rights reserved.

using System;
using System.Globalization;
using System.Windows.Media;

namespace Xomega.Framework.Wpf.Converters
{
    /// <summary>
    /// A value converter that can convert a Severity value of the ErrorMessage to an icon image source.
    /// It uses default system icons for severities, but allows setting custom icons as well.
    /// </summary>
    public class SeverityToBrushConverter : OneWayConverter
    {
        /// <summary>
        /// Brush for the <see cref="ErrorSeverity.Info"/>.
        /// </summary>
        public Brush Info { get; set; } = Brushes.Black;

        /// <summary>
        /// Brush for the <see cref="ErrorSeverity.Warning"/>.
        /// </summary>
        public Brush Warning { get; set; } = Brushes.DarkBlue;

        /// <summary>
        /// Brush for the <see cref="ErrorSeverity.Error"/>.
        /// </summary>
        public Brush Error { get; set; } = Brushes.Red;

        /// <summary>
        /// Brush for the <see cref="ErrorSeverity.Critical"/>.
        /// </summary>
        public Brush Critical { get; set; } = Brushes.Red;

        /// <summary>
        /// Converts the severity value to a brush.
        /// </summary>
        /// <param name="value">The Xomega severity enumeration value.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The brush for the specified severity.</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush brush = ErrorSeverity.Info.Equals(value) ? Info :
                ErrorSeverity.Warning.Equals(value) ? Warning :
                ErrorSeverity.Error.Equals(value) ? Error :
                ErrorSeverity.Critical.Equals(value) ? Critical :
                null;

            return brush;
        }
    }
}
