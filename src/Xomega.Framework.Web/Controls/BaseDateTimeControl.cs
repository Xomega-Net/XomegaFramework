// Copyright (c) 2022 Xomega.Net. All rights reserved.

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Xomega.Framework.Properties;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// Base class for DateTime control.
    /// </summary>
    public partial class BaseDateTimeControl : UserControl
    {
        #region PropertyBinding

        /// <summary>
        /// Property binding class for the control.
        /// </summary>
        public class PropertyBinding : WebPropertyBinding
        {
            /// <summary>
            /// Registers a binding creator in the factory.
            /// </summary>
            public static void Register()
            {
                Register(typeof(BaseDateTimeControl), delegate (object obj)
                {
                    var ctl = obj as BaseDateTimeControl;
                    return IsBindable(ctl) ? new PropertyBinding(ctl) : null;
                });
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            protected PropertyBinding(BaseDateTimeControl ctl) : base(ctl) { }

            /// <summary>
            /// OnPropertyBound event handler.
            /// </summary>
            protected override void OnPropertyBound()
            {
                base.OnPropertyBound();
                (control as BaseDateTimeControl).OnPropertyBound(property as DateTimeProperty, row);
            }
        }

        /// <summary>
        /// Control's OnPropertyBound event handler extension.
        /// </summary>
        /// <param name="prop">Property object.</param>
        /// <param name="row">The data row context, if in a list.</param>
        public virtual void OnPropertyBound(DateTimeProperty prop, DataRow row)
        {
            if (BaseBinding.Create(txt_DateTime) is BasePropertyBinding pb)
                pb.BindTo(prop, row);
        }

        #endregion

        /// <summary>
        /// Static constructor that triggers registration of the binding.
        /// </summary>
        static BaseDateTimeControl()
        {
            PropertyBinding.Register();
        }

        /// <summary>
        /// Css class for the property bound textbox.
        /// </summary>
        public string TextCssClass { get; set; }

        /// <summary>
        /// Textbox that will be bound to the property's value.
        /// </summary>
        protected TextBox txt_DateTime;

        /// <summary>
        /// Control initialization handler.
        /// </summary>
        /// <param name="e">EventArgs object.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            txt_DateTime.CssClass = TextCssClass;
        }
    }
}
