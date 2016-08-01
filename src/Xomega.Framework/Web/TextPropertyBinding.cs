﻿// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Web.UI;
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
        protected string Script_AutoComplete = "if (typeof autocomplete === 'function') autocomplete({{controlID:{0},items:{1},multivalue:{2}}});";

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
                List<string> values = new List<string>();
                IEnumerable items = property.ItemsProvider(null);
                foreach (object i in items)
                {
                    values.Add(property.ValueToString(i, ValueFormat.EditString));
                }

                WebUtil.RegisterStartupScript(control, "AutoComplete", Script_AutoComplete,
                    "'" + control.ClientID + "'",
                    "['" + string.Join("','", values) + "']",
                    property.IsMultiValued ? "true" : "false");
            }
        }
    }
}
