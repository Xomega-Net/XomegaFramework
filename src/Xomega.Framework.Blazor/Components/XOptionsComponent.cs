// Copyright (c) 2020 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xomega.Framework.Blazor
{
    /// <summary>
    /// Base class for components that have an associated list of options to select the value(s).
    /// </summary>
    public class XOptionsComponent : XComponent
    {
        /// <summary>
        /// Injected instance of JavaScript runtime for client-side calls.
        /// </summary>
        [Inject] protected IJSRuntime JSRuntime { get; set; }

        /// <summary>
        /// Items for the list of options based on the current property values.
        /// </summary>
        protected IEnumerable Items => IsEditable ? AvailableItems : SelectedItems;

        /// <summary>
        /// A list of possible values for the property.
        /// </summary>
        protected IEnumerable AvailableItems { get; set; } = new ArrayList();

        /// <summary>
        /// Get a list of possible values for the property.
        /// </summary>
        protected async Task GetAvailableItems(CancellationToken token = default)
        {
            if (Property?.AsyncItemsProvider != null)
                AvailableItems = await Property.AsyncItemsProvider(null, Row, token);
        }

        /// <inheritdoc/>
        protected async override Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            await GetAvailableItems();
        }

        /// <inheritdoc/>
        protected async override Task OnPropertyChangeAsync(object sender, PropertyChangeEventArgs e, CancellationToken token)
        {
            if (e.Change.IncludesItems())
                await GetAvailableItems(token);
            await base.OnPropertyChangeAsync(sender, e, token);
        }

        /// <summary>
        /// A list of selected/current value(s) of the property.
        /// </summary>
        protected IList SelectedItems => (IsNull ? new ArrayList() :
            IsMultiValue ? Property.InternalValue as IList :
            new ArrayList() { Property.InternalValue }) ?? new ArrayList();

        /// <summary>
        /// Utility method to determin if the specified item should be selected in a list of options.
        /// </summary>
        /// <param name="value">Current value of the property.</param>
        /// <param name="item">The item to check.</param>
        /// <returns>True, if the item should be selected, false otherwise.</returns>
        protected bool IsSelected(object value, object item) =>
            IsMultiValue ? value is IList list && list.Contains(item) : item.Equals(value);

        /// <summary>
        /// Custom template for displaying item options.
        /// </summary>
        [Parameter]
        public RenderFragment<object> ItemDisplayTemplate { get; set; }

        /// <summary>
        /// Actual template for displaying item options, which could be custom or a default.
        /// </summary>
        public RenderFragment<object> Template => ItemDisplayTemplate ?? RenderValue;

        /// <summary>
        /// Default template for displaying item optins, which shows items formatted
        /// with the <see cref="ValueFormat.DisplayString"/> format.
        /// </summary>
        /// <param name="val">The item to render.</param>
        /// <returns>The render fragment.</returns>
        protected RenderFragment RenderValue(object val) => (b) =>
        {
            b.AddContent(1, Property.ValueToString(val, ValueFormat.DisplayString));
        };

        /// <summary>
        /// Utility method to get correct selected values from the specified multivalue select element.
        /// It calls a custom JavaScript function to get selected values as a workaround for an issue
        /// with Blazor that doesn't currently support multivalue select elements.
        /// For details see https://github.com/dotnet/aspnetcore/issues/5519
        /// </summary>
        /// <param name="selectElement">Select element to get the values from.</param>
        /// <returns>A list of selected values for the specified element.</returns>
        protected virtual async Task<List<string>> GetSelectedValues(ElementReference selectElement)
            => (await JSRuntime.InvokeAsync<List<string>>("xomegaControls.getSelectedValues", selectElement)).ToList();
    }
}
