// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System.Windows;
using System.Windows.Controls.Primitives;

namespace Xomega.Framework.Binding
{
    /// <summary>
    /// A class that provides data property binding for toggle button WPF elements
    /// (checkboxes, radio buttons etc.).
    /// </summary>
    public class ToggleButtonPropertyBinding : DataPropertyBinding
    {
        /// <summary>
        ///  A static method to register the ToggleButtonPropertyBinding for toggle button WPF elements.
        /// </summary>
        public static void Register()
        {
            Register(typeof(ToggleButton), delegate(object obj)
            {
                ToggleButton tb = obj as ToggleButton;
                return IsBindable(tb) ? new ToggleButtonPropertyBinding(tb) : null;
            });
        }

        /// <summary>
        /// Constructs a new toggle button property binding for the given toggle button.
        /// </summary>
        /// <param name="tglButton">The toggle button to be bound to the property.</param>
        protected ToggleButtonPropertyBinding(ToggleButton tglButton)
            : base(tglButton)
        {
            tglButton.Checked += OnStateChanged;
            tglButton.Unchecked += OnStateChanged;
            tglButton.Indeterminate += OnStateChanged;
        }

        /// <summary>
        /// Remove any listeners when disposing
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            ToggleButton tglButton = (ToggleButton)element;
            tglButton.Checked -= OnStateChanged;
            tglButton.Unchecked -= OnStateChanged;
            tglButton.Indeterminate -= OnStateChanged;
        }

        /// <summary>
        /// Updates the property when the state of the toggle button is changed.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void OnStateChanged(object sender, RoutedEventArgs e)
        {
            UpdateProperty(((ToggleButton)element).IsChecked);
        }

        /// <summary>
        /// Updates the toggle button based on the given property change.
        /// If the property is not required the toggle button will allow three states
        /// to support an indeterminate state when the property value is not set.
        /// Otherwise turns the toggle button on or off based on the Boolean value of the property.
        /// </summary>
        /// <param name="change">The property change.</param>
        protected override void UpdateElement(PropertyChange change)
        {
            base.UpdateElement(change);
            if (property == null) return;

            if (change.IncludesRequired())
            {
                ((ToggleButton)element).IsThreeState = !property.Required;
            }
            if (change.IncludesValue())
            {
                bool? check = null;
                object val = property.InternalValue;
                if (val is bool) check = (bool)val;
                else check = val as bool?;
                ((ToggleButton)element).IsChecked = check;
            }
        }
    }
}
