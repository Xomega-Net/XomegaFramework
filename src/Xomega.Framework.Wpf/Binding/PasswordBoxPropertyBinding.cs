// Copyright (c) 2025 Xomega.Net. All rights reserved.

using System.Windows;
using System.Windows.Controls;

namespace Xomega.Framework.Binding
{
    /// <summary>
    /// A class that provides data property binding for password box WPF elements.
    /// </summary>
    public class PasswordBoxPropertyBinding : DataPropertyBinding
    {
        /// <summary>
        ///  A static method to register the PasswordBoxPropertyBinding for PasswordBox WPF elements.
        /// </summary>
        public static void Register()
        {
            Register(typeof(PasswordBox), delegate(object obj)
            {
                PasswordBox pb = obj as PasswordBox;
                return IsBindable(pb) ? new PasswordBoxPropertyBinding(pb) : null;
            });
        }

        /// <summary>
        /// Constructs a new password box property binding for the given password box.
        /// </summary>
        /// <param name="pwdBox">The password box to be bound to the property.</param>
        protected PasswordBoxPropertyBinding(PasswordBox pwdBox)
            : base(pwdBox)
        {
            pwdBox.PasswordChanged += OnPasswordChanged;
        }

        /// <summary>
        /// Remove any listeners when disposing
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            ((PasswordBox)element).PasswordChanged -= OnPasswordChanged;
        }

        private async void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (property == null) return;
            string newText = ((PasswordBox)element).Password;
            await UpdatePropertyAsync(newText);
        }

        /// <summary>
        /// Binds the text box to the given property.
        /// Sets the maximum text length to the property size if available.
        /// </summary>
        /// <param name="property">The data property to bind the text box to.</param>
        /// <param name="row">The data row context, if any.</param>
        public override void BindTo(DataProperty property, DataRow row)
        {
            base.BindTo(property, row);
            ((PasswordBox)element).MaxLength = (property != null && !property.IsMultiValued && property.Size > 0) ? property.Size : 0;
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
                ((PasswordBox)element).Password = property.GetStringValue(property.Editable ? ValueFormat.EditString : ValueFormat.DisplayString, row);
            }
        }
    }
}
