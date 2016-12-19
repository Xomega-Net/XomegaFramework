// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System.Collections;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;

namespace Xomega.Framework.Binding
{
    /// <summary>
    /// A class that provides data property binding for Selector WPF elements (ListBox or ComboBox).
    /// It allows displaying the list of available items provided by the property,
    /// and keeps the selected items in sync with the current property values.
    /// </summary>
    public class SelectorPropertyBinding : DataPropertyBinding
    {
        /// <summary>
        ///  A static method to register the selector property binding for Selector WPF element.
        /// </summary>
        public static void Register()
        {
            Register(typeof(Selector), delegate(object obj)
            {
                Selector sel = obj as Selector;
                return IsBindable(sel) ? new SelectorPropertyBinding(sel) : null;
            });
        }

        /// <summary>
        /// Constructs a new selector property binding for the given Selector element.
        /// </summary>
        /// <param name="selector">The selector element to be bound to the data property.</param>
        protected SelectorPropertyBinding(Selector selector) : base(selector)
        {
            selector.SelectionChanged += OnSelectionChanged;
            selector.AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(OnTextChanged));
        }

        /// <summary>
        /// Remove any listeners when disposing
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            ((Selector)element).SelectionChanged -= OnSelectionChanged;
            ((Selector)element).RemoveHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(OnTextChanged));
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PreventElementUpdate) return;

            ListBox lb = element as ListBox;
            if (lb != null && lb.SelectedItems.Count > 1)
                UpdateProperty(lb.SelectedItems);
            else UpdateProperty(((Selector)element).SelectedItem);
        }

        /// <summary>
        /// For editable combo boxes updates the property value whenever the text is changed.
        /// </summary>
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ComboBox cb = element as ComboBox;
            if (cb != null && element == sender)
                UpdateProperty(cb.Text);
        }

        /// <summary>
        /// A XAML string that represents the default list item template that allows formatting
        /// list items according to the property rules for the DisplayString value format.
        /// </summary>
        protected string defaultTemplate = @"<DataTemplate
            xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
            xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
            xmlns:xom='clr-namespace:Xomega.Framework;assembly=Xomega.Framework'>
            <TextBlock xom:DataPropertyItemBinding.ValueFormat='{x:Static xom:ValueFormat.DisplayString}'/></DataTemplate>";

        /// <summary>
        /// Binds the selector to the given property. Sets the selection mode based on
        /// whether or not the property is multivalued and the default list item template
        /// unless it has already been set in XAML.
        /// </summary>
        /// <param name="property">The data property to bind the framework element to.</param>
        public override void BindTo(DataProperty property)
        {
            // update selection mode before updating the values
            ListBox lb = element as ListBox;
            if (lb != null && property != null)
            {
                if (!property.IsMultiValued)
                    lb.SelectionMode = SelectionMode.Single;
                else if (lb.SelectionMode == SelectionMode.Single)
                    lb.SelectionMode = SelectionMode.Extended;
            }
            Selector sel = (Selector)element;
            if (property != null && sel.ItemTemplate == null && sel.ItemTemplateSelector == null)
                sel.ItemTemplate = XamlReader.Parse(defaultTemplate) as DataTemplate;
            base.BindTo(property);
            if (sel.DataContext == null) FixPopupRootMemoryLeak();
        }

        /// <summary>
        /// A WPF memory leak fix for popup root not resetting/clearing the data context
        /// </summary>
        /// <seealso href="https://connect.microsoft.com/VisualStudio/feedback/details/796702/wpf-combobox-memory-leak"/>
        protected virtual void FixPopupRootMemoryLeak()
        {
            if (element is ComboBox && VisualTreeHelper.GetChildrenCount(element) > 0)
            {
                FrameworkElement templateRoot = VisualTreeHelper.GetChild(element, 0) as FrameworkElement;
                Popup popup = templateRoot == null ? null : VisualTreeHelper.GetChild(templateRoot, 0) as Popup;
                if (popup != null)
                {
                    // get the popup root private field via reflection
                    object sw = popup.GetType().GetField("_popupRoot", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(popup);
                    FrameworkElement popupRoot = sw == null ? null :
                        sw.GetType().GetField("_value", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sw) as FrameworkElement;
                    if (popupRoot != null)
                        popupRoot.DataContext = null;
                }
            }
        }

        /// <summary>
        /// Updates the selector based on the given property change.
        /// Updates the list of possible items based on the values provided by the property
        /// and inserts a blank item for a combo box if the property is editable and not required.
        /// </summary>
        /// <param name="change">The property change.</param>
        protected override void UpdateElement(PropertyChange change)
        {
            base.UpdateElement(change);
            Selector sel = element as Selector;

            if (property == null || sel == null) return;

            object val = property.InternalValue;
            IList lst = val as IList;
            ListBox lb = element as ListBox;
            ComboBox cb = element as ComboBox;

            if (change.IncludesItems() ||
                lb != null && change.IncludesEditable() ||
                cb != null && change.IncludesRequired())
            {
                sel.Items.Clear();
                IEnumerable src = null;
                if (lb != null && !property.Editable && lst != null) src = lst;
                else if (property.ItemsProvider != null) src = property.ItemsProvider(null);

                // for non-required drop down lists add null string option
                if (cb != null && !cb.IsEditable && !property.Required)
                    sel.Items.Add(property.NullString);

                // add items explicitly. Don't use sel.ItemSource since src is not observable
                if (src != null) foreach (object item in src) sel.Items.Add(item);
            }
            if (change.IncludesValue() || change.IncludesItems())
            {
                if (lb != null && lst != null && lb.SelectionMode != SelectionMode.Single)
                {
                    lb.SelectedItems.Clear();
                    foreach (object item in lst) lb.SelectedItems.Add(item);
                }
                else if (lst != null && lst.Count > 0) sel.SelectedItem = lst[0];
                else sel.SelectedItem = val;

                if (cb != null && cb.IsEditable) cb.Text = property.EditStringValue;
            }
        }
    }
}
