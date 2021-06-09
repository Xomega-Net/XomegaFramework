// Copyright (c) 2021 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Popups;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xomega.Framework;
using Xomega.Framework.Blazor;

namespace Xomega._Syncfusion.Blazor
{
    /// <summary>
    /// Base class for Xomega Syncfusion option components.
    /// </summary>
    /// <typeparam name="T">The type of the option items.</typeparam>
    public class XSfOptionsComponent<T> : XOptionsComponent<T>
    {
        /// <summary>
        /// Gets the type of float label to use, if any.
        /// </summary>
        public FloatLabelType FloatLabelType => ShowLabel ? FloatLabelType.Always : FloatLabelType.Never;

        /// <summary>
        /// Allows setting tooltip position for the validation error tooltip.
        /// </summary>
        [Parameter] public Position TooltipPosition { get; set; } = Position.BottomCenter;

        /// <summary>
        /// CSS class used for components' validation errors.
        /// </summary>
        protected string ErrorClass => XSfComponent.ErrorClass;

        /// <summary>
        /// A tooltip handler that allows conditionally showing component's tooltip with errors as needed.
        /// </summary>
        /// <param name="args">Tooltip arguments.</param>
        protected void OnTooltipOpen(TooltipEventArgs args) => XSfComponent.ShowTooltipError(this, args);

        /// <summary>
        /// Whether component allows users to filter its options.
        /// </summary>
        [Parameter] public bool AllowFiltering { get; set; }

        /// <summary>
        /// The attribute of the option items to group the items by.
        /// </summary>
        [Parameter] public string GroupByAttribute { get; set; }

        /// <summary>
        /// A template to use for displaying group headers, when grouping is enabled.
        /// </summary>
        [Parameter] public RenderFragment<ComposedItemModel<T>> GroupTemplate { get; set; }

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            StateDescriptions = XSfComponent.GetStateDescriptions();
        }

        /// <inheritdoc/>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            await GetAvailableItems(null);

            if (ItemDisplayTemplate == null)
                ItemDisplayTemplate = RenderValue;

            if (!string.IsNullOrEmpty(GroupByAttribute) && GroupTemplate == null)
                GroupTemplate = RenderGroup;
        }

        /// <summary>
        /// Default template for rendering a group, which just outputs group's text.
        /// </summary>
        /// <param name="group">The group to render.</param>
        /// <returns>The render fragment for the group.</returns>
        protected RenderFragment RenderGroup(ComposedItemModel<T> group) => (b) =>
            b.AddContent(1, group?.Text);

        /// <summary>
        /// Gets the value for single-select or multi-select options components.
        /// </summary>
        /// <returns>The value(s) for the options component.</returns>
        protected object GetValue()
        {
            var sel = SelectedItems;
            return sel.Count > 0 ? sel[0] : default;
        }

        /// <summary>
        /// Gets a string value for single-select options components.
        /// </summary>
        /// <returns>A string value for the component.</returns>
        protected string GetStringValue()
        {
            var sel = GetSelectedItems(ValueFormat.Transport);
            return sel == null || sel.Count == 0 ? null : sel[0]?.ToString();
        }

        /// <summary>
        /// Async value change handler for the component that updates the property with the selected item.
        /// </summary>
        /// <typeparam name="TValue">The type of the item.</typeparam>
        /// <param name="args">Change event arguements.</param>
        protected async Task OnValueChanged<TValue>(ChangeEventArgs<TValue, T> args)
        {
            await UpdatePropertyAsync(args.ItemData);
        }

        /// <summary>
        /// Async handler for specifying a custom text value, which tries to resolve it into a typed value.
        /// </summary>
        /// <param name="args">Custom value arguments.</param>
        protected async Task OnCustomValue(CustomValueSpecifierEventArgs<T> args)
        {
            var val = await Property.ResolveValueAsync(args.Text, ValueFormat.Internal, Row);
            if (val is T item) args.Item = item;
        }
    }
}
