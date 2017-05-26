using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Xomega.Framework;
using Xomega.Framework.Views;

namespace AdventureWorks.Client.Wpf
{
    /// <summary>
    /// Interaction logic for AppliedCriteria.xaml
    /// </summary>
    public partial class AppliedCriteria : UserControl, IAppliedCriteriaPanel
    {
        public AppliedCriteria()
        {
            InitializeComponent();
        }

        public void BindTo(List<FieldCriteriaSetting> settings)
        {
            DataContext = settings;
        }
    }

    [ValueConversion(typeof(object), typeof(string))]
    public class AppliedCriteriaConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<FieldCriteriaSetting> criteria = value as List<FieldCriteriaSetting>;
            if (criteria != null) return FieldCriteriaSetting.ToString(criteria);
            else return value != null ? value.ToString() : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
