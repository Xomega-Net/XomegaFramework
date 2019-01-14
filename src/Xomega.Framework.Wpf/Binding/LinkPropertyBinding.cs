// Copyright (c) 2019 Xomega.Net. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Xomega.Framework.Binding
{
    /// <summary>
    /// A class that provides data property binding for hyperlink WPF elements.
    /// It sets the inline text to the data property value formatted based on the DisplayString
    /// value format and keeps it in sync with the underlying property value.
    /// </summary>
    public class LinkPropertyBinding : DataPropertyBinding
    {
        /// <summary>
        ///  A static method to register the TextBlockPropertyBinding for TextBlock WPF elements.
        /// </summary>
        public static void Register()
        {
            Register(typeof(Hyperlink), delegate(object obj)
            {
                Hyperlink lnk = obj as Hyperlink;
                return IsBindable(lnk) ? new LinkPropertyBinding(lnk) : null;
            });
        }

        /// <summary>
        /// Constructs a new link property binding for the given hyperlink.
        /// </summary>
        /// <param name="link">The hyperlink to be bound to the property.</param>
        protected LinkPropertyBinding(Hyperlink link) : base(link)
        {
        }

        /// <summary>
        /// Updates the text of the hyperlink to the property value formatted
        /// according to the property's DisplayString format.
        /// </summary>
        /// <param name="change">The property change.</param>
        protected override void UpdateElement(PropertyChange change)
        {
            base.UpdateElement(change);
            if (property == null) return;

            if (change.IncludesValue())
            {
                Hyperlink lnk = (Hyperlink)element;
                lnk.Inlines.Clear();
                lnk.Inlines.Add(property.DisplayStringValue);
            }
        }

        /// <summary>
        /// Overrides base method to prevent disabling the hyperlink as it is not editable.
        /// </summary>
        protected override void UpdateEditability()
        {
            // don't disable as it is not editable
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
