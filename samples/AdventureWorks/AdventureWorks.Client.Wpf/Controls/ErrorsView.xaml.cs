using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Xomega.Framework;
using Xomega.Framework.Views;

namespace AdventureWorks.Client.Wpf
{
    /// <summary>
    /// Interaction logic for Errors.xaml
    /// </summary>
    public partial class ErrorsView : Window, IErrorPresenter
    {
        void IErrorPresenter.Show(ErrorList errorList)
        {
            if (errorList == null || errorList.Errors.Count == 0) return;
            lstErrors.ItemsSource = errorList.Errors;
            Show();
        }

        public ErrorsView()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    class SeverityToBrushConverter : IValueConverter
    {
        public Brush Warning { get; set; }
        public Brush Error { get; set; }
        public Brush Critical { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (ErrorSeverity.Warning.Equals(value)) return Warning;
            else if (ErrorSeverity.Error.Equals(value)) return Error;
            if (ErrorSeverity.Critical.Equals(value)) return Critical;
            else return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
