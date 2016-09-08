// Copyright (c) 2016 Xomega.Net. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Xomega.Framework.Properties;

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
                ctl.btn_Add.Click += delegate (object sender, EventArgs e)
                {
                    ListBox lbxSelection = (control as BasePickListControl).lbx_Selection;
                    if (property == null ||
                        property.ItemsProvider == null ||
                        lbxSelection.Items.Count > 0 && !property.IsMultiValued)
                        return;

                    List<string> values = new List<string>();
                    ListBox lbxItems = (control as BasePickListControl).lbx_Items;
                    foreach (object i in property.ItemsProvider(null))
                    {
                        string value = property.ValueToString(i, ValueFormat.EditString);

                        ListItem li = lbxSelection.Items.FindByValue(value);
                        if (li == null)
                        {
                            li = lbxItems.Items.FindByValue(value);
                            if (li != null && !li.Selected)
                                li = null;
                        }
                        if (li != null)
                        {
                            values.Add(value);
                            if (!property.IsMultiValued)
                                break;
                        }
                    }
                    property.SetValue(values.Count == 0 ? property.NullString : string.Join(",", values.ToArray()));
                };

                ctl.btn_Remove.Click += delegate (object sender, EventArgs e)
                {
                    if (property == null) return;

                    List<string> values = new List<string>();
                    ListBox lbxSelection = (control as BasePickListControl).lbx_Selection;
                    foreach (ListItem i in lbxSelection.Items)
                    {
                        if (i.Selected) continue;
                        values.Add(i.Value);
                    }
                    property.SetValue(values.Count == 0 ? property.NullString : string.Join(",", values.ToArray()));
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

        /// <summary>
        /// Static constructor that triggers registration of the binding.
        /// </summary>
        static BasePickListControl()
        {
            PropertyBinding.Register();
        }

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
        /// Removes items from the selection.
        /// </summary>
        public Button btn_Remove;
    }
}
