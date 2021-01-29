// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System.Web.UI.WebControls;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// A class that provides data property binding for label web controls.
    /// It sets the editable text to the data property value formatted based on the EditString
    /// value format and keeps it in sync with the underlying property value.
    /// </summary>
    public class LabelPropertyBinding : WebPropertyBinding
    {
        /// <summary>
        ///  A static method to register the LabelPropertyBinding for Label web controls.
        /// </summary>
        public static void Register()
        {
            Register(typeof(Label), delegate(object obj)
            {
                Label lb = obj as Label;
                return IsBindable(lb) ? new LabelPropertyBinding(lb) : null;
            });
        }

        /// <summary>
        /// Constructs a new label property binding for the given label.
        /// </summary>
        /// <param name="label">The label to be bound to the property.</param>
        protected LabelPropertyBinding(Label label)
            : base(label)
        {
        }

        /// <summary>
        /// Do nothing on editability as the label is not editable.
        /// </summary>
        protected override void UpdateEditability()
        {
        }
    }
}
