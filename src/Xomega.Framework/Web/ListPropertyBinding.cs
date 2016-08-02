// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Xomega.Framework.Properties;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// A class that provides data property binding for ListControl web controls
    /// (ListBox, CheckBoxList, RadioButtonList or DropDownList).
    /// It allows displaying the list of available items provided by the property,
    /// and keeps the selected items in sync with the current property values.
    /// </summary>
    public class ListPropertyBinding : WebPropertyBinding
    {
        /// <summary>
        ///  A static method to register the list property binding for ListControl web control.
        /// </summary>
        public static void Register()
        {
            Register(typeof(ListControl), delegate(object obj)
            {
                ListControl sel = obj as ListControl;
                return IsBindable(sel) ? new ListPropertyBinding(sel) : null;
            });
        }

        /// <summary>
        /// Constructs a new list property binding for the given ListControl element.
        /// </summary>
        /// <param name="list">The list control to be bound to the data property.</param>
        protected ListPropertyBinding(ListControl list)
            : base(list)
        {
            bool isMultiVal = control is ListBox || control is CheckBoxList;
            // ListBox and CheckBoxList post values on selection change, since deselecting all has no post value
            // and we have to use selection change to detect this as opposed to no post at all
            if (isMultiVal)
            {
                PostedValue = null;
                list.SelectedIndexChanged += delegate
                {
                    if (isMultiVal && property != null && control.Page != null)
                    {
                        PostedValue = control.Page.Request.Form[control.UniqueID] ?? "";
                        // for CheckBoxList each item is posted individually, so we just reconstruct it
                        // from the selected item, since LoadPostData should have happened by now.
                        if (control is CheckBoxList)
                        {
                            List<string> newValues = new List<string>();
                            foreach (ListItem item in list.Items)
                                if (item.Selected && !newValues.Contains(item.Value)) newValues.Add(item.Value);
                            PostedValue = string.Join(",", newValues.ToArray());
                        }
                        UpdateProperty(PostedValue);
                    }
                };
            }
        }

        /// <summary>
        /// Sets the selection mode based on whether or not the property is multivalued.
        /// </summary>
        protected override void OnPropertyBound()
        {
            base.OnPropertyBound();

            ListBox lb = control as ListBox;
            if (lb != null)
                lb.SelectionMode = property.IsMultiValued ? ListSelectionMode.Multiple : ListSelectionMode.Single;
        }

        /// <summary>
        /// Updates the property with the given value from the element.
        /// Overrides the base property to prevent setting values that are not in the list
        /// similar to the <see cref="EnumProperty.CascadingPropertyChange"/>
        /// </summary>
        /// <param name="value">The value to set on the data property.</param>
        protected override void UpdateProperty(object value)
        {
            EnumProperty enumProp = property as EnumProperty;
            if (enumProp != null && enumProp.FilterFunc != null)
            {
                object internalValue = enumProp.ResolveValue(value, ValueFormat.Internal);
                if (enumProp.IsMultiValued)
                {
                    List<Header> Values = internalValue as List<Header>;
                    if (Values != null) value = Values.Where(enumProp.FilterFunc).ToList();
                }
                else if (internalValue is Header && !enumProp.FilterFunc((Header)internalValue)) value = null;
            }

            base.UpdateProperty(value);
        }

        /// <summary>
        /// Updates the list control based on the given property change.
        /// Updates the list of possible items based on the values provided by the property
        /// and inserts a blank item for a drop down list if the property is editable and not required.
        /// </summary>
        /// <param name="change">The property change.</param>
        protected override void UpdateElement(PropertyChange change)
        {
            // let the base handle everything except value change
            base.UpdateElement(change - PropertyChange.Value);
            ListControl list = control as ListControl;

            if (property == null || list == null) return;

            object val = property.InternalValue;
            IList lst = val == null ? new ArrayList() : val as IList;
            ListBox lb = control as ListBox;
            bool isMultiVal = lst != null && (control is CheckBoxList
                || lb != null && lb.SelectionMode == ListSelectionMode.Multiple);
            DropDownList dl = control as DropDownList;

            if (change.IncludesItems() || isMultiVal && change.IncludesEditable() ||
                dl != null && (change.IncludesRequired() || change.IncludesValue()))
            {
                IEnumerable src = null;
                if (isMultiVal && !property.Editable) src = lst;
                else if (property.ItemsProvider != null) src = property.ItemsProvider(null);
                list.Items.Clear();
                if (dl != null && !property.Required)
                    list.Items.Add(new ListItem("", property.NullString));
                if (dl != null && property.Required && property.IsNull())
                    list.Items.Add(new ListItem("Select Value...", ""));
                if (src != null)
                {
                    foreach (object i in src)
                    {
                        ListItem li = new ListItem(property.ValueToString(i, ValueFormat.DisplayString),
                            property.ValueToString(i, ValueFormat.EditString));
                        list.Items.Add(li);
                    }
                }
            }
            if (change.IncludesValue() || change.IncludesItems() || !isMultiVal && change.IncludesRequired())
            {
                foreach (ListItem li in list.Items) li.Selected = false;
                IEnumerable values = isMultiVal ? lst : new object[] { (lst != null && lst.Count > 0) ? lst[0] : property.InternalValue };
                foreach (object i in values)
                {
                    ListItem li = list.Items.FindByValue(property.ValueToString(i, ValueFormat.EditString));
                    if (li == null) // add value not in list to avoid data binding exceptions
                    {
                        li = new ListItem(property.ValueToString(i, ValueFormat.DisplayString),
                            property.ValueToString(i, ValueFormat.EditString));
                        list.Items.Add(li);
                    }
                    li.Selected = true;
                }
            }
        }
    }
}
