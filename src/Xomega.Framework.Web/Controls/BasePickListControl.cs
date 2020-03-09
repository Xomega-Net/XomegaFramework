// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// Base class for PickList control.
    /// </summary>
    public partial class BasePickListControl : UserControl
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
                Register(typeof(BasePickListControl), delegate (object obj)
                {
                    var ctl = obj as BasePickListControl;
                    return IsBindable(ctl) ? new PropertyBinding(ctl) : null;
                });
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            protected PropertyBinding(BasePickListControl ctl) : base(ctl)
            {
                ctl.btn_AddAll.Click += delegate (object sender, EventArgs e)
                {
                    if (property == null || property.ItemsProvider == null) return;
                    List<object> values = new List<object>();
                    foreach (object val in property.ItemsProvider(null)) values.Add(val);
                    property.SetValue(values);
                };

                ctl.btn_Add.Click += delegate (object sender, EventArgs e)
                {
                    ListBox lbxSelection = (control as BasePickListControl).lbx_Items;
                    string selection = lbxSelection.Page.Request.Form[lbxSelection.UniqueID];
                    if (property == null || string.IsNullOrEmpty(selection)) return;

                    if (!property.IsMultiValued && property.IsNull())
                        property.SetValue(selection.Split(',')[0]);
                    else if (property.IsMultiValued)
                    {
                        IList sel = property.ResolveValue(selection, ValueFormat.Internal) as IList;
                        IList val = property.InternalValue as IList;
                        if (val != null && sel != null)
                            foreach (object v in val)
                                if (!sel.Contains(v)) sel.Add(v);
                        property.SetValue(sel);
                    }
                };

                ctl.btn_Remove.Click += delegate (object sender, EventArgs e)
                {
                    ListBox lbxSelection = (control as BasePickListControl).lbx_Selection;
                    string selection = lbxSelection.Page.Request.Form[lbxSelection.UniqueID];
                    if (property == null || string.IsNullOrEmpty(selection)) return;

                    if (!property.IsMultiValued && !property.IsNull())
                        property.SetValue(null);
                    else if (property.IsMultiValued)
                    {
                        IList sel = property.ResolveValue(selection, ValueFormat.Internal) as IList;
                        IList val = property.InternalValue as IList;
                        if (val != null && sel != null)
                            foreach (object v in sel) val.Remove(v);
                        property.SetValue(val);
                    }
                };

                ctl.btn_RemoveAll.Click += delegate (object sender, EventArgs e)
                {
                    if (property != null)
                        property.SetValue(null);
                };
            }

            /// <summary>
            /// Handles OnPropertyBound event.
            /// </summary>
            protected override void OnPropertyBound()
            {
                base.OnPropertyBound();

                BasePickListControl ctl = control as BasePickListControl;
                ctl.lbx_Items.SelectionMode = property.IsMultiValued ? ListSelectionMode.Multiple : ListSelectionMode.Single;
                ctl.lbx_Selection.SelectionMode = ListSelectionMode.Multiple;
                ctl.btn_AddAll.Visible = ctl.btn_RemoveAll.Visible = property.IsMultiValued;
            }

            /// <summary>
            /// Overrides the base method to hide all but selection list when not editable.
            /// </summary>
            protected override void UpdateEditability()
            {
                var lbxItems = (control as BasePickListControl).lbx_Items;
                var pnlButtons = (control as BasePickListControl).pnl_Buttons;
                lbxItems.Visible = pnlButtons.Visible = property.Editable;

                var lbxSelection = (control as BasePickListControl).lbx_Selection;
                if (property.Editable) lbxSelection.Attributes.Remove("disabled");
                else lbxSelection.Attributes.Add("disabled", "disabled");
            }

            /// <summary>
            /// Extends the base method to place the required class on the wrapper element.
            /// </summary>
            protected override void UpdateRequired()
            {
                base.UpdateRequired();
                var pnlWrapper = (control as BasePickListControl).pnl_Wrapper;
                pnlWrapper.CssClass = WebUtil.AddOrRemoveClass(pnlWrapper.CssClass, "required", property.Required);
            }

            /// <summary>
            /// Overrides the base method to place the validation tooltip and class on the selection list.
            /// </summary>
            protected override void UpdateValidation()
            {
                var lbxSelection = (control as BasePickListControl).lbx_Selection;
                ErrorList errors = property.ValidationErrors;
                bool markAsInvalid = errors != null && errors.Errors.Count > 0 && property.Visible && property.Editable;
                lbxSelection.CssClass = WebUtil.AddOrRemoveClass(lbxSelection.CssClass, "invalid", markAsInvalid);
                lbxSelection.ToolTip = markAsInvalid ? errors.ErrorsText : null;
            }

            /// <summary>
            /// Updates the control and fills the items and selection listboxes.
            /// </summary>
            /// <param name="change">The property change.</param>
            protected override void UpdateElement(PropertyChange change)
            {
                base.UpdateElement(change);

                if (change.IncludesItems() || change.IncludesValue())
                {
                    // update selection list
                    ListBox lbxSelection = (control as BasePickListControl).lbx_Selection;
                    lbxSelection.Items.Clear();

                    IEnumerable values = property.IsMultiValued ?
                        property.InternalValue as IEnumerable :
                        new object[] { property.InternalValue };

                    if (values != null) foreach (object i in values)
                    {
                        lbxSelection.Items.Add(new ListItem(
                            property.ValueToString(i, ValueFormat.DisplayString),
                            property.ValueToString(i, ValueFormat.EditString)));
                    }

                    // update items list
                    ListBox lbxItems = (control as BasePickListControl).lbx_Items;
                    lbxItems.Items.Clear();

                    IEnumerable items = property.ItemsProvider != null ? property.ItemsProvider(null) : null;
                    if (items != null) foreach (object i in items)
                    {
                        string value = property.ValueToString(i, ValueFormat.EditString);
                        ListItem li = lbxSelection.Items.FindByValue(value);
                        if (li == null)
                        {
                            lbxItems.Items.Add(new ListItem(
                                property.ValueToString(i, ValueFormat.DisplayString),
                                value));
                        }
                    }
                }
            }
        }

        #endregion

        #region Static constructor

        /// <summary>
        /// Static constructor that triggers registration of the binding.
        /// </summary>
        static BasePickListControl()
        {
            PropertyBinding.Register();
        }

        #endregion

        #region Controls

        /// <summary>
        /// Wrapper element.
        /// </summary>
        public Panel pnl_Wrapper;

        /// <summary>
        /// Buttons container.
        /// </summary>
        public Panel pnl_Buttons;

        /// <summary>
        /// List of possible items.
        /// </summary>
        public ListBox lbx_Items;

        /// <summary>
        /// List of selected items.
        /// </summary>
        public ListBox lbx_Selection;

        /// <summary>
        /// Adds items to the selection.
        /// </summary>
        public Button btn_Add;

        /// <summary>
        /// Adds all items to the selection.
        /// </summary>
        public Button btn_AddAll;

        /// <summary>
        /// Removes items from the selection.
        /// </summary>
        public Button btn_Remove;

        /// <summary>
        /// Removes items from the selection.
        /// </summary>
        public Button btn_RemoveAll;

        #endregion

        #region Properties

        /// <summary>
        /// Number of visible rows.
        /// </summary>
        public int Rows { get; set; }

        /// <summary>
        /// Default number of visible rows.
        /// </summary>
        public static int DefaultRows = 5;

        #endregion

        #region Events

        /// <summary>
        /// Control initialization handler.
        /// </summary>
        /// <param name="e">EventArgs object.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            lbx_Items.Rows = lbx_Selection.Rows = Rows > 0 ? Rows : DefaultRows;
        }

        #endregion
    }
}
