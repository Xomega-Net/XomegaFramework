// Copyright (c) 2023 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Popups;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xomega.Framework;
using Xomega.Framework.Blazor;
using Xomega.Framework.Lookup;

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
        public FloatLabelType FloatLabelType => ShowLabel ? FloatLabel ? FloatLabelType.Auto : FloatLabelType.Always : FloatLabelType.Never;

        /// <summary>
        /// Indicates whether the label floats above the control on focus or always stays above the control.
        /// </summary>
        [Parameter] public bool FloatLabel { get; set; }

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
        /// Custom template for displaying the value.
        /// </summary>
        [Parameter] public RenderFragment<T> ValueDisplayTemplate { get; set; }

        /// <summary>
        /// Actual template for displaying the value, which could be custom or default to the item template.
        /// </summary>
        public RenderFragment<T> ValueTemplate => ValueDisplayTemplate ?? Template;

        /// <summary>
        /// The attribute of the option items to group the items by.
        /// </summary>
        [Parameter] public string GroupByAttribute { get; set; }

        /// <summary>
        /// A template to use for displaying group headers, when grouping is enabled.
        /// </summary>
        [Parameter] public RenderFragment<ComposedItemModel<T>> GroupTemplate { get; set; }

        /// <summary>
        /// Whether component allows custom values that are not in the lookup table
        /// </summary>
        [Parameter] public bool? AllowCustom { get; set; }

        /// <summary>
        /// Effective flag on whether component allows custom values that defaults to the property lookup validation configuration
        /// </summary>
        protected bool AllowCustomValue => AllowCustom != null ? AllowCustom.Value :
                                           Property?.LookupValidation == LookupValidationType.None;


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
        /// <param name="args">Change event arguments.</param>
        protected virtual async Task OnValueChanged<TValue>(ChangeEventArgs<TValue, T> args)
        {
            // update property only from the user update, since SF would often send null value on its own here
            if (ShouldUpdateValue(args))
            {
                await UpdatePropertyAsync(args.ItemData);
            }
        }

        /// <summary>
        /// Determines when the component should update the property value when control's value changes
        /// </summary>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="args">Change event args</param>
        /// <returns>True to update the value, false otherwise.</returns>
        protected virtual bool ShouldUpdateValue<TValue>(ChangeEventArgs<TValue, T> args) => args.IsInteracted;

        /// <summary>
        /// Async handler for filtering the list using the property's AsyncItemsProvider.
        /// </summary>
        /// <param name="args">Arguments that provide the query and return results.</param>
        protected async Task OnActionComplete(ActionCompleteEventArgs<T> args)
        {
            if (AllowFiltering && Property?.AsyncItemsProvider != null)
            {
                string text = args.Query?.Queries?.Where?.First()?.value?.ToString();
                var items = await Property.AsyncItemsProvider(text, Row, CancellationToken.None);
                if (items != null)
                    args.Result = items.Cast<T>();
            }
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
