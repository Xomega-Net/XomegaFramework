// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System.Web.UI.WebControls;
using System.Text.RegularExpressions;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// A class that provides data property binding for hyper link web controls.
    /// It sets the link text to the data property value formatted based on the DisplayString
    /// value format and keeps it in sync with the underlying property value.
    /// It can also update the NavigateUrl to replace the '{value}' placeholder with the property value.
    /// </summary>
    public class LinkPropertyBinding : WebPropertyBinding
    {
        /// <summary>
        ///  A static method to register the LinkPropertyBinding for HyperLink web controls.
        /// </summary>
        public static void Register()
        {
            Register(typeof(HyperLink), delegate(object obj)
            {
                HyperLink lnk = obj as HyperLink;
                return IsBindable(lnk) ? new LinkPropertyBinding(lnk) : null;
            });
        }

        /// <summary>
        /// Constructs a new link property binding for the given hyper link.
        /// </summary>
        /// <param name="lnk">The hyper link to be bound to the property.</param>
        protected LinkPropertyBinding(HyperLink lnk)
            : base(lnk)
        {
        }

        /// <summary>
        /// Overrides base method to prevent disabling the hyperlink as it is not editable.
        /// </summary>
        protected override void UpdateEditability()
        {
            // don't disable as it is not editable
        }

        /// <summary>
        /// Updates the text of a hyper link to the property value formatted
        /// according to the property's DisplayString format. Also updates the NavigateUrl
        /// to replace the '{value}' placeholder with the value of the data property.
        /// </summary>
        /// <param name="change">The property change.</param>
        protected override void UpdateElement(PropertyChange change)
        {
            base.UpdateElement(change - PropertyChange.Value);
            if (property == null) return;

            HyperLink lnk = (HyperLink)control;
            lnk.Text = property.DisplayStringValue;
            if (lnk.NavigateUrl != null)
            {
                lnk.NavigateUrl = lnk.NavigateUrl.Replace("{value}", property.EditStringValue);
                lnk.NavigateUrl = Regex.Replace(lnk.NavigateUrl, @"\[(.*?)\]", GetPropertyValue);
            }
        }

        /// <summary>
        /// Replace the URL placeholder {p:PropertyName} with the value of the corresponding
        /// property of the parent data object.
        /// </summary>
        /// <param name="m">The regex match that contains property name.</param>
        /// <returns>The property value to use.</returns>
        protected string GetPropertyValue(Match m)
        {
            string prop = m.Result("$1");
            DataObject parent = this.property.GetParent();
            if (parent != null && parent.HasProperty(prop))
                return parent[prop].EditStringValue;
            return m.Value;
        }
    }
}
