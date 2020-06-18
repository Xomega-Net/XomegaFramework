// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System;
using System.Globalization;
using System.Windows.Controls;
using Xomega.Framework.Properties;

namespace Xomega.Framework.Binding
{
    /// <summary>
    /// A class that provides data property binding for date picker WPF elements.
    /// It sets the selected date to the data property value 
    /// and keeps it in sync with the underlying property value.
    /// </summary>
    public class DatePickerPropertyBinding : DataPropertyBinding
    {
        /// <summary>
        ///  A static method to register the DatePickerPropertyBinding for DatePicker WPF elements.
        /// </summary>
        public static void Register()
        {
            Register(typeof(DatePicker), delegate(object obj)
            {
                DatePicker dp = obj as DatePicker;
                return IsBindable(dp) ? new DatePickerPropertyBinding(dp) : null;
            });
        }

        /// <summary>
        /// Constructs a new date picker property binding for the given date picker.
        /// </summary>
        /// <param name="datePicker">The date picker to be bound to the property.</param>
        protected DatePickerPropertyBinding(DatePicker datePicker)
            : base(datePicker)
        {
            datePicker.SelectedDateChanged += OnSelectedDateChanged;
        }

        private async void OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (property == null) return;
            DateTime? newDate = ((DatePicker)element).SelectedDate;
            await UpdatePropertyAsync(newDate);
        }

        /// <summary>
        /// Remove any listeners when disposing
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            ((DatePicker)element).SelectedDateChanged -= OnSelectedDateChanged;
        }

        /// <summary>
        /// Binds the date picker to the given property.
        /// Sets the long format on the date picker if the property has current culture's long date format
        /// </summary>
        /// <param name="property">The data property to bind the date picker to.</param>
        /// <param name="row">The data row context, if any.</param>
        public override void BindTo(DataProperty property, DataRow row)
        {
            base.BindTo(property, row);
            DateTimeProperty dtp = property as DateTimeProperty;
            if (dtp != null)
            {
                ((DatePicker)element).SelectedDateFormat = DateTimeFormatInfo.CurrentInfo.LongDatePattern.Equals(dtp.Format) ? 
                    DatePickerFormat.Long : DatePickerFormat.Short;
            }
        }

        /// <summary>
        /// Updates the selected date of the date picker.
        /// </summary>
        /// <param name="change">The property change.</param>
        protected override void UpdateElement(PropertyChange change)
        {
            base.UpdateElement(change);
            if (property == null) return;

            if (change.IncludesValue())
            {
                DateTimeProperty dtp = property as DateTimeProperty;
                if (dtp != null) ((DatePicker)element).SelectedDate = dtp.Value;
                else ((DatePicker)element).Text = property.GetStringValue(property.Editable ? ValueFormat.EditString : ValueFormat.DisplayString, row);
            }
        }
    }
}
