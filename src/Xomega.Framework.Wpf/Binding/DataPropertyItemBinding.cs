// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System.Windows;
using System.Windows.Controls;

namespace Xomega.Framework
{
    /// <summary>
    /// A generic binding that allows displaying property items (possible values)
    /// formatted by the data property according to the specified value format
    /// (typically <see cref="ValueFormat.DisplayString"/>) based on the property configuration.
    /// For example, if property values and possible items are of type <c>Header</c>
    /// and the property is configured to display its values as a combination of ID and Name,
    /// then the list of possible items (and not just the property value) will be also displayed like this.
    /// This class is used internally by the <see cref="Binding.SelectorPropertyBinding"/> class
    /// but can also be used in the XAML by application developers when a custom list item template
    /// is being used.
    /// </summary>
    /// <example>
    /// Below is the default list item data template used by Xomega framework. Developers can enhance it
    /// in XAML to provide a more sophisticated template for list items.
    /// <DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
    /// xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    /// xmlns:xfk='clr-namespace:Xomega.Framework;assembly=Xomega.Framework'
    /// xmlns:xom='clr-namespace:Xomega.Framework;assembly=Xomega.Framework.Wpf'>
    /// <TextBlock xom:DataPropertyItemBinding.ValueFormat='{x:Static xfk:ValueFormat.DisplayString}'/></DataTemplate>
    /// </example>
    public class DataPropertyItemBinding
    {
        /// <summary>
        /// Gets the value format dependency property that is set on the given text block if any.
        /// </summary>
        /// <param name="obj">The text block element.</param>
        /// <returns>The value format dependency property that is set on the given text block if any.</returns>
        public static ValueFormat GetValueFormat(TextBlock obj)
        {
            return (ValueFormat)obj.GetValue(ValueFormatProperty);
        }

        /// <summary>
        /// Sets the value format dependency property on a text block.
        /// </summary>
        /// <param name="obj">The text block element to set the property on.</param>
        /// <param name="value">The value format to set on the text block.</param>
        public static void SetValueFormat(TextBlock obj, ValueFormat value)
        {
            obj.SetValue(ValueFormatProperty, value);
        }

        /// <summary>
        /// A ValueFormat dependency property that needs to be set on a TextBlock inside a list item template
        /// to make it display the string for the current item formatted according to the property's rules.
        /// </summary>
        public static readonly DependencyProperty ValueFormatProperty = DependencyProperty.RegisterAttached(
            "Bind", typeof(ValueFormat), typeof(DataPropertyItemBinding), new PropertyMetadata(OnValueFormatChanged));

        /// <summary>
        /// A callback that is triggered when a value format dependency properties is set.
        /// </summary>
        /// <param name="d">The dependency object the property has been set on.</param>
        /// <param name="e">Event arguments.</param>
        public static void OnValueFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBlock el)) return;

            OnDataContextChanged(el, new DependencyPropertyChangedEventArgs());
            el.DataContextChanged += OnDataContextChanged;
        }

        /// <summary>
        /// A handler of the data context change event on a text block that has a value format dependency property set,
        /// which updates the text of the text block to be formatted according to the specified value format.
        /// </summary>
        /// <param name="sender">Event sender, which should be a text block.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is TextBlock tb)) return;

            DataPropertyBinding binding = GetBinding(tb);
            if (binding != null && binding.BoundProperty != null)
                tb.Text = binding.BoundProperty.ValueToString(tb.DataContext, GetValueFormat(tb));
        }

        /// <summary>
        /// Gets the data property binding from a dependency object or its parent.
        /// </summary>
        /// <param name="obj">Dependency object.</param>
        /// <returns>Data property binding or null if not found.</returns>
        private static DataPropertyBinding GetBinding(DependencyObject obj)
        {
            if (obj == null) return null;
            DataPropertyBinding b = obj.GetValue(Property.BindingProperty) as DataPropertyBinding;
            return b;
        }
    }
}
