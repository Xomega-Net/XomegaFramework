// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Xomega.Framework.Converters
{
    /// <summary>
    /// A value converter that can convert a Severity value of the ErrorMessage to an icon image source.
    /// It uses default system icons for severities, but allows setting custom icons as well.
    /// </summary>
    public class SeverityToIconConverter : IValueConverter
    {
        /// <summary>
        /// Icon for the <see cref="ErrorSeverity.Info"/>.
        /// </summary>
        public Icon Info { get; set; } = SystemIcons.Information;

        /// <summary>
        /// Icon for the <see cref="ErrorSeverity.Warning"/>.
        /// </summary>
        public Icon Warning { get; set; } = SystemIcons.Warning;

        /// <summary>
        /// Icon for the <see cref="ErrorSeverity.Error"/>.
        /// </summary>
        public Icon Error { get; set; } = SystemIcons.Error;

        /// <summary>
        /// Icon for the <see cref="ErrorSeverity.Critical"/>.
        /// </summary>
        public Icon Critical { get; set; } = SystemIcons.Error;

        /// <summary>
        /// Converts the severity value to an image source.
        /// </summary>
        /// <param name="value">The Xomega severity enumeration value.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The image source for the specified severity.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Icon icon = ErrorSeverity.Info.Equals(value) ? Info :
                ErrorSeverity.Warning.Equals(value) ? Warning :
                ErrorSeverity.Error.Equals(value) ? Error :
                ErrorSeverity.Critical.Equals(value) ? Critical :
                SystemIcons.Question;

            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            return imageSource;
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
