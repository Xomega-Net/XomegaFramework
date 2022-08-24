// Copyright (c) 2022 Xomega.Net. All rights reserved.

using System.Web.UI.WebControls;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// A class that provides data property binding for check box web controls.
    /// </summary>
    public class CheckBoxPropertyBinding : WebPropertyBinding
    {
        /// <summary>
        ///  A static method to register the CheckBoxPropertyBinding for CheckBox web controls.
        /// </summary>
        public static void Register()
        {
            Register(typeof(CheckBox), delegate(object obj)
            {
                CheckBox tb = obj as CheckBox;
                return IsBindable(tb) ? new CheckBoxPropertyBinding(tb) : null;
            });
        }

        /// <summary>
        /// Constructs a new check box property binding for the given check box.
        /// </summary>
        /// <param name="checkBox">The check box to be bound to the property.</param>
        protected CheckBoxPropertyBinding(CheckBox checkBox)
            : base(checkBox)
        {
            // check box sets values on selection change, since unchecked control posts no value
            // and we have to use selection change to detect this as opposed to no post at all
            PostedValue = null;
            checkBox.CheckedChanged += delegate
            {
                if (property != null && control.Page != null && !PreventModelUpdate)
                {
                    PostedValue = control.Page.Request.Form[control.UniqueID];
                    UpdateProperty(PostedValue != null);
                }
            };
        }

        /// <summary>
        /// Overrides handling of the property change to prevent posting value if property
        /// value got changed outside of the control.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Property change event arguments.</param>
        protected override void OnPropertyChange(object sender, PropertyChangeEventArgs e)
        {
            // prevent posting value if property value got changed outside of control 
            if (e.Change == PropertyChange.Value && PostedValue != "false") PreventModelUpdate = true;

            base.OnPropertyChange(sender, e);
        }

        /// <summary>
        /// Updates the check box's Checked state based on the boolean property value.
        /// </summary>
        /// <param name="change">The property change.</param>
        protected override void UpdateElement(PropertyChange change)
        {
            // let the base handle everything except value change
            base.UpdateElement(change - PropertyChange.Value);
            if (property == null) return;

            object val = property.GetValue(ValueFormat.Internal, row);
            bool check =  val is bool b && b;
            if (change.IncludesValue())
            {
                ((CheckBox)control).Checked = check;
            }
            // Update property value after binding to make it in sync with the checkbox.
            // Otherwise if the checkbox stays unchecked it will not post a false value
            // and the property value will remain whatever it was, e.g. null.
            if (change == PropertyChange.All && !check)
            {
                PostedValue = "false";
            }
        }
    }
}
