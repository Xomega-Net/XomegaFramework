// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System.Collections;
using System.Web.UI.WebControls;
using Xomega.Framework.Properties;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// A class that provides data property binding for text box web controls.
    /// It sets the editable text to the data property value formatted based on the EditString
    /// value format and keeps it in sync with the underlying property value.
    /// </summary>
    public class TextPropertyBinding : WebPropertyBinding
    {
        /// <summary>
        ///  A static method to register the TextBoxPropertyBinding for TextBox web controls.
        /// </summary>
        public static void Register()
        {
            Register(typeof(TextBox), delegate(object obj)
            {
                TextBox tb = obj as TextBox;
                return IsBindable(tb) ? new TextPropertyBinding(tb) : null;
            });
        }

        /// <summary>
        /// Constructs a new text box property binding for the given text box.
        /// </summary>
        /// <param name="textBox">The text box to be bound to the property.</param>
        protected TextPropertyBinding(TextBox textBox)
            : base(textBox)
        {
        }

        /// <summary>Script to invoke autocomplete functionality on a control.</summary>
        protected string Script_AutoComplete = @"if (xomegaControls && typeof xomegaControls._autoComplete === 'function')
            xomegaControls._autoComplete({{controlId:'{0}',items:[{1}],multivalue:{2},delimiter:'{3}'}});";

        /// <summary>
        /// Sets the maximum text length to the property size if available.
        /// </summary>
        protected override void OnPropertyBound()
        {
            base.OnPropertyBound();
            ((TextBox)control).MaxLength = (property != null && !property.IsMultiValued && property.Size > 0) ? property.Size : 0;

            // use jQuery UI autocomplete widget for enum properties
            if (property is EnumProperty && property.ItemsProvider != null)
            {
                string values = "";
                IEnumerable items = property.ItemsProvider(null, row);
                foreach (object i in items)
                {
                    if (values.Length > 0) values += ",";
                    values += string.Format("{{value:'{0}',editValue:'{1}'}}",
                        property.ValueToString(i, ValueFormat.DisplayString),
                        property.ValueToString(i, ValueFormat.EditString));
                }

                WebUtil.RegisterStartupScript(control, "AutoComplete", Script_AutoComplete,
                    control.ClientID,
                    values,
                    property.IsMultiValued ? "true" : "false",
                    property.DisplayListSeparator);
            }
        }
    }
}
