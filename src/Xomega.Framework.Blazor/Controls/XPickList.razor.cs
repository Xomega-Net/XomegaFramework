// Copyright (c) 2021 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;

namespace Xomega.Framework.Blazor.Controls
{
    /// <summary>
    /// Pick list component bound to a multi-value data property that allows selecting multiple values
    /// by moving items from a list of available items to a list of selected items and vice versa.
    /// </summary>
    public partial class XPickList
    {
        /// <summary>
        /// Resource manager used to get text resources.
        /// </summary>
        [Inject] private ResourceManager ResManager { get; set; }

        /// <summary>
        /// The number of rows to display in the selection lists.
        /// </summary>
        [Parameter] public int Rows { get; set; } = 6;

        /// <summary>
        /// A reference to the select control for the available items.
        /// </summary>
        protected ElementReference AvailableList { get; set; }

        /// <summary>
        /// A reference to the select control for the currently selected items.
        /// </summary>
        protected ElementReference SelectedList { get; set; }

        private List<string> HighlightedValues;

        private bool IsHighlighted(string value)
            => HighlightedValues != null && HighlightedValues.Contains(value);

        /// <summary>
        /// Gets a string resource for the specified key.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <returns>The localized string resource.</returns>
        protected string GetString(string key)
        {
            var resMgr = ResManager ?? Messages.ResourceManager;
            return resMgr.GetString(key);
        }

        private string AvailableLabel => string.Format(GetString(Messages.PickList_Available), Label);
        private string SelectedLabel => IsEditable ? string.Format(GetString(Messages.PickList_Selected), Label) : Label;

        /// <summary>
        /// Handles clicking on a button to add all available items to the list of selected items.
        /// </summary>
        protected async Task AddAll(MouseEventArgs e)
        {
            if (Property?.AsyncItemsProvider == null) return;
            List<object> values = new List<object>();
            foreach (object val in AvailableItems) values.Add(val);
            await UpdatePropertyAsync(values);
            Property.SetEditing(false, Row);
        }

        /// <summary>
        /// Handles clicking on a button to add highlighted available items to the list of selected items.
        /// </summary>
        protected async Task Add(MouseEventArgs e)
        {
            HighlightedValues = await GetSelectedValues(AvailableList);
            if (Property == null || HighlightedValues.Count() == 0) return;

            if (!IsMultiValue && IsNull)
                await UpdatePropertyAsync(HighlightedValues[0]);
            else if (IsMultiValue)
            {
                IList sel = Property.ResolveValue(HighlightedValues, ValueFormat.Internal) as IList;
                if (Property.InternalValue is IList val && sel != null)
                    foreach (object v in val)
                        if (!sel.Contains(v)) sel.Add(v);
                await UpdatePropertyAsync(sel);
            }
            Property.SetEditing(false, Row);
        }

        /// <summary>
        /// Handles clicking on a button to remove highlighted selected items from the property.
        /// </summary>
        protected async Task Remove(MouseEventArgs e)
        {
            HighlightedValues = await GetSelectedValues(SelectedList);
            if (Property == null || HighlightedValues.Count() == 0) return;

            if (!IsMultiValue && !IsNull)
                await UpdatePropertyAsync(null);
            else if (IsMultiValue)
            {
                IList val = Property.InternalValue as IList;
                if (val != null && Property.ResolveValue(HighlightedValues, ValueFormat.Internal) is IList sel)
                    foreach (object v in sel) val.Remove(v);
                await UpdatePropertyAsync(val);
            }
            Property.SetEditing(false, Row);
        }

        /// <summary>
        /// Handles clicking on a button to remove all selected items from the property.
        /// </summary>
        protected async Task RemoveAll(MouseEventArgs e)
        {
            await UpdatePropertyAsync(null);
            Property.SetEditing(false, Row);
        }
    }
}