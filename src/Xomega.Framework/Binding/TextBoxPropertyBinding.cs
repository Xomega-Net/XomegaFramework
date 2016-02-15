// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System.Windows;
using System.Windows.Controls;

namespace Xomega.Framework.Binding
{
    /// <summary>
    /// A class that provides data property binding for text box WPF elements.
    /// It sets the editable text to the data property value formatted based on the EditString
    /// value format and keeps it in sync with the underlying property value.
    /// </summary>
    public class TextBoxPropertyBinding : DataPropertyBinding
    {
        /// <summary>
        ///  A static method to register the TextBoxPropertyBinding for TextBox WPF elements.
        /// </summary>
        public static void Register()
        {
            Register(typeof(TextBox), delegate(object obj)
            {
                TextBox tb = obj as TextBox;
                return IsBindable(tb) ? new TextBoxPropertyBinding(tb) : null;
            });
        }

        /// <summary>
        /// Constructs a new text box property binding for the given text box.
        /// </summary>
        /// <param name="textBox">The text box to be bound to the property.</param>
        protected TextBoxPropertyBinding(TextBox textBox)
            : base(textBox)
        {
            textBox.TextChanged += delegate
            {
                if (property == null) return;
                string curText = property.Editable ? property.EditStringValue : property.DisplayStringValue;
                string newText = ((TextBox)element).Text;
                // Only update if the text really changed, since the event may not be user-triggered
                // and this may cause a value change (e.g. for time boxes where seconds are not displayed)
                // which would make the property modified.
                if (newText != curText) UpdateProperty(newText);
            };
        }

        /// <summary>
        /// Binds the text box to the given property.
        /// Sets the maximum text length to the property size if available.
        /// </summary>
        /// <param name="property">The data property to bind the text box to.</param>
        public override void BindTo(DataProperty property)
        {
            base.BindTo(property);
            ((TextBox)element).MaxLength = (property != null && !property.IsMultiValued && property.Size > 0) ? property.Size : 0;
        }

        /// <summary>
        /// Updates the text of the text box to the property value formatted
        /// according to the property's EditString format if editable or DisplayString if not editable.
        /// </summary>
        /// <param name="change">The property change.</param>
        protected override void UpdateElement(PropertyChange change)
        {
            base.UpdateElement(change);
            if (property == null) return;

            if (change.IncludesValue())
            {
                ((TextBox)element).Text = property.Editable ? property.EditStringValue : property.DisplayStringValue;
            }
        }
    }
}
