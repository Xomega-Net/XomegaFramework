// Copyright (c) 2025 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections;

namespace Xomega.Framework.Blazor
{
    /// <summary>
    /// Base class for components that have an associated list of options to select the value(s).
    /// </summary>
    public class XOptionsComponent<T> : XComponent
    {
        /// <summary>
        /// Injected instance of JavaScript runtime for client-side calls.
        /// </summary>
        [Inject] protected IJSRuntime JSRuntime { get; set; }

        /// <summary>
        /// Items for the list of options based on the current property values.
        /// </summary>
        protected IEnumerable<T> Items => IsEditable ? AvailableItems : SelectedItems;

        /// <summary>
        /// A list of possible values for the property.
        /// </summary>
        protected IList<T> AvailableItems { get; set; } = new List<T>();

        /// <summary>
        /// Get a list of possible values for the property.
        /// </summary>
        protected async Task GetAvailableItems(object arg, CancellationToken token = default)
        {
            if (Property?.AsyncItemsProvider != null)
            {
                var items = await Property.AsyncItemsProvider(arg, Row, token);
                if (items != null)
                    AvailableItems = items.Cast<T>().ToList();
            }
        }

        /// <inheritdoc/>
        protected async override Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            await GetAvailableItems(null);
            if (Property != null)
                Property.AsyncChange += OnPropertyChangeAsync;
        }

        private async Task OnPropertyChangeAsync(object sender, PropertyChangeEventArgs e, CancellationToken token)
        {
            await InvokeAsync(async () => await OnPropertyChangeAsync(e, token));
        }

        /// <summary>
        /// Updates available items when those change on the property.
        /// </summary>
        /// <param name="e">Property change details.</param>
        /// <param name="token">Cancellation token.</param>
        protected virtual async Task OnPropertyChangeAsync(PropertyChangeEventArgs e, CancellationToken token)
        {
            if (e.Change.IncludesItems() && Equals(Row, e.Row))
                await GetAvailableItems(null, token);
        }

        /// <summary>
        /// A list of selected/current value(s) of the property.
        /// </summary>
        protected IList<T> SelectedItems => GetSelectedItems(ValueFormat.Internal).Cast<T>().ToList();

        /// <summary>
        /// A list of selected/current value(s) of the property in the specified format.
        /// </summary>
        protected IList GetSelectedItems(ValueFormat format) => (IsNull ? new ArrayList() :
            IsMultiValue ? Property.GetValue(format, Row) as IList :
            new ArrayList() { Property.GetValue(format, Row) }) ?? new ArrayList();

        /// <summary>
        /// Utility method to determine if the specified item should be selected in a list of options.
        /// </summary>
        /// <param name="value">Current value of the property.</param>
        /// <param name="item">The item to check.</param>
        /// <returns>True, if the item should be selected, false otherwise.</returns>
        protected bool IsSelected(object value, object item) =>
            IsMultiValue ? value is IList list && list.Contains(item) : item.Equals(value);

        /// <summary>
        /// Custom template for displaying item options.
        /// </summary>
        [Parameter] public RenderFragment<T> ItemDisplayTemplate { get; set; }

        /// <summary>
        /// Actual template for displaying item options, which could be custom or a default.
        /// </summary>
        public RenderFragment<T> Template => ItemDisplayTemplate ?? RenderValue;

        /// <summary>
        /// Default template for displaying item options, which shows items formatted
        /// with the <see cref="ValueFormat.DisplayString"/> format.
        /// </summary>
        /// <param name="val">The item to render.</param>
        /// <returns>The render fragment.</returns>
        protected RenderFragment RenderValue(T val) => (b) =>
        {
            b.AddContent(1, Property.ValueToString(val, ValueFormat.DisplayString));
        };

        /// <summary>
        /// Disposes the component and unbinds from the property.
        /// </summary>
        public override void Dispose()
        {
            if (property != null)
                property.AsyncChange -= OnPropertyChangeAsync;
            base.Dispose();
        }
    }
}