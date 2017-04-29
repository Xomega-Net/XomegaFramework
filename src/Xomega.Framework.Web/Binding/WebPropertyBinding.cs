// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// A base class for providing bindings between data properties and various web controls.
    /// A data property binding is responsible for making sure that the state of the web control
    /// is in sync with the state of the underlying data property.
    /// Web property bindings are created when a control is bound to a specific data object using
    /// <see cref="BindToObject(Control, DataObject)"/> method, which uses attribute <see cref="AttrProperty"/>
    /// to get the name of the data object property to bind the control to, and also possibly
    /// a path to a child object from the attribute <see cref="AttrChildObject"/> to locate that property.
    /// Web property bindings are created via a factory design pattern. A <c>PropertyBindingCreator</c>
    /// callback can be registered for any particular type of web controls. If no binding
    /// is registered for a given type, the one for the  base type will be used.
    /// </summary>
    public class WebPropertyBinding : BasePropertyBinding
    {
        #region Static registration

        /// <summary>
        /// A static constructor that registers Xomega framework web property bindings.
        /// </summary>
        static WebPropertyBinding()
        {
            Register();
            LabelPropertyBinding.Register();
            TextPropertyBinding.Register();
            LinkPropertyBinding.Register();
            ListPropertyBinding.Register();
            CheckBoxPropertyBinding.Register();
            GridViewBinding.Register();
        }

        /// <summary>
        ///  A static catch-all method to register WebPropertyBinding for all bindable web controls.
        /// </summary>
        private static void Register()
        {
            Register(typeof(WebControl), delegate(object obj)
            {
                WebControl ctl = obj as WebControl;
                return IsBindable(ctl) ? new WebPropertyBinding(ctl) : null;
            });
        }

        /// <summary>
        /// Checks if a control is property bindable.
        /// </summary>
        /// <param name="ctl">Control to check.</param>
        /// <returns>Whether or not the control is property bindable.</returns>
        public static bool IsBindable(Control ctl)
        {
            return ctl != null;
        }

        /// <summary>A shutdown hook that Dotfuscator can set the Teardown attribute for.</summary>
        static void InstrumentationShutDown() { }

        #endregion

        #region Binding to data objects

        /// <summary>
        /// An attribute set on the web control to indicate the property name that it should be bound to.
        /// It can be either hardcoded or databound to a static string for validation by ASP.NET compiler.
        /// </summary>
        public static string AttrProperty = "Property";

        /// <summary>
        /// An attribute set on the web control to indicate a dot-delimited path to a child object.
        /// It can be either hardcoded or databound to a static string for validation by ASP.NET compiler.
        /// </summary>
        public static string AttrChildObject = "ChildObject";

        /// <summary>
        /// An attribute set on the web control to indicate the ID of the label control for the current control.
        /// </summary>
        public static string AttrLabelId = "LabelID";

        /// <summary>
        /// Binds a control or all of its child controls (e.g. for a containing panel)
        /// to the given data object (or its child object)
        /// as specified by the controls' Property and ChildObject attributes.
        /// </summary>
        /// <param name="ctl">Web control to bind to the given data object.</param>
        /// <param name="obj">Data object to bind the web control to.</param>
        public static void BindToObject(Control ctl, DataObject obj)
        {
            BindToObject(ctl, obj, false);
        }

        /// <summary>
        /// Binds a control or all of its child controls (e.g. for a containing panel)
        /// to the given data object (or its child object)
        /// as specified by the controls' Property and ChildObject attributes.
        /// </summary>
        /// <param name="ctl">Control to bind to the given data object.</param>
        /// <param name="obj">Data object to bind the control to.</param>
        /// <param name="bindCurrentRow">For list objects specifies whether to bind the whole list or just the current row.</param>
        public static void BindToObject(Control ctl, DataObject obj, bool bindCurrentRow)
        {
            if (obj == null || ctl == null) return;

            AttributeCollection attr = GetControlAttributes(ctl);
            string childPath = attr != null ? attr[AttrChildObject] : null;
            DataObject cObj = FindChildObject(obj, childPath);
            obj = cObj as DataObject;
            string propertyName = attr != null ? attr[AttrProperty] : null;
            if (obj != null && propertyName != null)
            {
                WebPropertyBinding binding = Create(ctl) as WebPropertyBinding;
                if (binding != null) binding.BindTo(obj[propertyName]);
                // remove attributes that are no longer needed to minimize HTML
                attr.Remove(AttrChildObject);
                attr.Remove(AttrProperty);
            }
            else if (cObj is DataListObject && !bindCurrentRow) BindToList(ctl, (DataListObject)cObj);
            else foreach (Control c in ctl.Controls)
                    BindToObject(c, obj, bindCurrentRow);
        }

        /// <summary>
        /// Binds the specified control to the given data object list.
        /// </summary>
        /// <param name="ctl">Control to bind to the given data object list.</param>
        /// <param name="list">Data object list to bind the control to.</param>
        public static void BindToList(Control ctl, DataListObject list)
        {
            if (list == null || ctl == null) return;

            IListBindable listBinding = Create(ctl) as IListBindable;
            if (listBinding != null) listBinding.BindTo(list);
        }

        #endregion

        /// <summary>
        /// The control that is bound to the data property.
        /// </summary>
        protected Control control;

        /// <summary>
        /// The label control associated with the current control.
        /// </summary>
        protected WebControl label;

        /// <summary>
        /// The string value posted to the control and subsequently to the property.
        /// Null value indicates no postback for the control, even though IsPostBack
        /// may be set to true, e.g. when control was added with a web part during the post.
        /// </summary>
        protected string PostedValue { set; get; }

        /// <summary>
        /// Constructs a base data property web binding for the given web control.
        /// </summary>
        protected WebPropertyBinding(Control ctl)
        {
            control = ctl;

            if (ctl.Page != null)
                PostedValue = ctl.Page.Request.Form[control.UniqueID];

            // defer posting value until all controls are property bound
            ctl.Load += delegate {
                if (property != null && PostedValue != null) UpdateProperty(PostedValue);
            };
            ctl.Unload += delegate { Dispose(); };
        }

        /// <summary>
        /// Accesses the collection of attributes on the control.
        /// </summary>
        /// <param name="ctl">Control.</param>
        /// <returns>AttributeCollection object.</returns>
        public static AttributeCollection GetControlAttributes(Control ctl)
        {
            return ctl is WebControl ? (ctl as WebControl).Attributes :
                ctl is UserControl ? (ctl as UserControl).Attributes :
                null;
        }

        /// <summary>
        /// Retrieves the value of an attribute with the given key.
        /// </summary>
        /// <param name="key">Attribute's key.</param>
        /// <returns>Attribute's value.</returns>
        protected string GetControlAttribute(string key)
        {
            AttributeCollection attr = GetControlAttributes(control);
            return attr != null ? attr[key] : null;
        }

        /// <summary>
        /// Sets the attribute with the given key and value.
        /// </summary>
        /// <param name="key">Attribute's key.</param>
        /// <param name="value">Attribute's value.</param>
        protected void SetControlAttribute(string key, string value)
        {
            AttributeCollection attr = GetControlAttributes(control);
            if (attr != null)
                attr[key] = value;
        }

        /// <summary>
        /// Associates the current web control with the label that is stored in the control's
        /// attribute <see cref="AttrProperty"/>, which can be statically set in the ASPX.
        /// Default implementation sets the current element as the target for the label and also sets
        /// the label text on the data property from the corresponding label control if not already set.
        /// </summary>
        protected override void SetLabel()
        {
            string lblId = GetControlAttribute(AttrLabelId);
            if (string.IsNullOrEmpty(lblId) || control.NamingContainer == null ||
                (label = control.NamingContainer.FindControl(lblId) as WebControl) == null) return;

            // if it is a label, set its target to this control unless it's already set
            Label lbl = label as Label;
            if (lbl != null)
            {
                lbl.DataBind();
                if (string.IsNullOrEmpty(lbl.AssociatedControlID))
                    lbl.AssociatedControlID = control.ID;
            }

            // set property label if it is not already set and if associated label is present
            string lblTxt = null;
            if (label is ITextControl) lblTxt = "" + ((ITextControl)label).Text;
            else if (label is HyperLink) lblTxt = ((HyperLink)label).Text;
            SetPropertyLabel(lblTxt);
        }

        /// <summary>
        /// A method to update the label text and set it on the property if needed.
        /// Strip off HTML tags, e.g. <u>L</u> used for underlining the access key.
        /// </summary>
        /// <param name="lblText">The label text from the label control.</param>
        protected override void SetPropertyLabel(string lblText)
        {
            // strip off HTML tags, e.g. <u>L</u> used for underlining the access key
            lblText = Regex.Replace(lblText, @"<(\w+)>(.*?)</\1>", "$2");
            base.SetPropertyLabel(lblText);
        }

        /// <summary>
        /// Updates editability of the control based on editability of the property.
        /// Default behavior just disables the control, but subclasses can make it read-only instead
        /// or handle it in a different way.
        /// </summary>
        protected override void UpdateEditability()
        {
            var ctl = control as WebControl;
            if (ctl != null)
                ctl.Enabled = property.Editable;
        }

        /// <summary>
        /// Updates visibility of the control based on the visibility of the property.
        /// </summary>
        protected override void UpdateVisibility()
        {
            control.Visible = property.Visible;
            if (label != null) label.Visible = property.Visible;
        }

        /// <summary>
        /// (Un)sets a 'required' CssClass of the control and the label (if any)
        /// based on the Required flag of the data property. Subclasses can handle it in a different way.
        /// </summary>
        protected override void UpdateRequired()
        {
            var ctl = control as WebControl;
            if (ctl != null)
                ctl.CssClass = WebUtil.AddOrRemoveClass(ctl.CssClass, "required", property.Required);
            if (label != null && property != null && property.Editable)
                label.CssClass = WebUtil.AddOrRemoveClass(label.CssClass, "required", property.Required);
        }

        /// <summary>
        /// (Un)sets a 'invalid' CssClass of the control based on the validation status
        /// of the data property. The default implementation sets control's tooltip
        /// to a combined error text from all property validation errors.
        /// </summary>
        protected override void UpdateValidation()
        {
            var ctl = control as WebControl;
            if (ctl == null) return;
            ErrorList errors = property.ValidationErrors == null ? null : property.ValidationErrors;
            ctl.CssClass = WebUtil.AddOrRemoveClass(ctl.CssClass, "invalid", false);
            ctl.ToolTip = null;
            if (errors != null && errors.Errors.Count > 0 && property.Visible && property.Editable)
            {
                ctl.CssClass = WebUtil.AddOrRemoveClass(ctl.CssClass, "invalid", true);
                ctl.ToolTip = errors.ErrorsText;
            }
        }

        /// <summary>
        /// Overrides handling of the property change to always update the control
        /// and also trigger stop editing if the control actually changed the property value.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Property change event arguments.</param>
        protected override void OnPropertyChange(object sender, PropertyChangeEventArgs e)
        {
            // prevent posting value if property value got changed
            // outside of control (e.g. via cascading selection)
            if (e.Change == PropertyChange.Value) PostedValue = null;

            // change base behavior to always update the control
            // e.g. to refresh a required dropdown list
            bool b = PreventElementUpdate;
            PreventElementUpdate = false;
            base.OnPropertyChange(sender, e);
            PreventElementUpdate = b;

            // stop the editing (and hence trigger the editing end event),
            // if the posted value actually changed the property value
            if (property == sender && PreventElementUpdate && 
                e.Change == PropertyChange.Value && !Equals(e.OldValue, e.NewValue))
                property.Editing = false;
        }

        /// <summary>
        /// Updates the text of a text control to the property value formatted
        /// according to the property's EditString format if editable or DisplayString if not editable.
        /// </summary>
        /// <param name="change">The property change.</param>
        protected override void UpdateElement(PropertyChange change)
        {
            base.UpdateElement(change);
            if (property == null) return;

            ITextControl txtCtl = control as ITextControl;
            if (change.IncludesValue() && txtCtl != null)
            {
                txtCtl.Text = control is IEditableTextControl && property.Editable ?
                    property.EditStringValue : property.DisplayStringValue;
            }
        }
    }
}
