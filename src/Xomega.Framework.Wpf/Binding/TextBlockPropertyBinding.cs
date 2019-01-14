// Copyright (c) 2019 Xomega.Net. All rights reserved.

using System.Windows;
using System.Windows.Controls;

namespace Xomega.Framework.Binding
{
    /// <summary>
    /// A class that provides data property binding for textblock WPF elements.
    /// It sets the text to the data property value formatted based on the DisplayString
    /// value format and keeps it in sync with the underlying property value.
    /// </summary>
    public class TextBlockPropertyBinding : DataPropertyBinding
    {
        /// <summary>
        ///  A static method to register the TextBlockPropertyBinding for TextBlock WPF elements.
        /// </summary>
        public static void Register()
        {
            Register(typeof(TextBlock), delegate(object obj)
            {
                TextBlock tb = obj as TextBlock;
                return IsBindable(tb) ? new TextBlockPropertyBinding(tb) : null;
            });
        }

        /// <summary>
        /// Constructs a new textblock property binding for the given text block.
        /// </summary>
        /// <param name="textBlock">The text block to be bound to the property.</param>
        protected TextBlockPropertyBinding(TextBlock textBlock) : base(textBlock)
        {
        }

        /// <summary>
        /// Updates the text of the text block to the property value formatted
        /// according to the property's DisplayString format.
        /// </summary>
        /// <param name="change">The property change.</param>
        protected override void UpdateElement(PropertyChange change)
        {
            base.UpdateElement(change);
            if (property == null) return;

            if (change.IncludesValue())
            {
                ((TextBlock)element).Text = property.DisplayStringValue;
            }
        }

        /// <summary>
        /// Overrides base method to do nothing, since the element is not editable
        /// </summary>
        /// <seealso cref="Property.RequiredProperty"/>
        protected override void UpdateRequired()
        {
        }
    }
}
