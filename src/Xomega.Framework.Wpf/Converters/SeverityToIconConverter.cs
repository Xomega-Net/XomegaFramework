// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Xomega.Framework.Converters
{
    /// <summary>
    /// A value converter that can convert a Severity value of the ErrorMessage to an icon brush from application resources.
    /// </summary>
    public class SeverityToIconConverter : IValueConverter
    {
        /// <summary>
        /// Icon key for the <see cref="ErrorSeverity.Info"/>.
        /// </summary>
        public string Info { get; set; } = "InformationIcon";

        /// <summary>
        /// Icon key for the <see cref="ErrorSeverity.Warning"/>.
        /// </summary>
        public string Warning { get; set; } = "WarningIcon";

        /// <summary>
        /// Icon key for the <see cref="ErrorSeverity.Error"/>.
        /// </summary>
        public string Error { get; set; } = "ErrorIcon";

        /// <summary>
        /// Icon key for the <see cref="ErrorSeverity.Critical"/>.
        /// </summary>
        public string Critical { get; set; } = "ErrorIcon";

        /// <summary>
        /// Converts the severity value to a drawing brush from application resources.
        /// </summary>
        /// <param name="value">The Xomega severity enumeration value.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The drawing bursh for the specified severity.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string icon = ErrorSeverity.Info.Equals(value) ? Info :
                ErrorSeverity.Warning.Equals(value) ? Warning :
                ErrorSeverity.Error.Equals(value) ? Error :
                ErrorSeverity.Critical.Equals(value) ? Critical :
                "QuestionIcon";

            return Application.Current.Resources[icon];
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