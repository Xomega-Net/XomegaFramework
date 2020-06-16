// Copyright (c) 2020 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xomega.Framework.Blazor
{
    /// <summary>
    /// Base class for data property bound components.
    /// </summary>
    public class XComponent : ComponentBase, IDisposable
    {
        /// <summary>
        /// The data property the component is bound to.
        /// </summary>
        [Parameter]
        public DataProperty Property { get; set; }

        /// <summary>
        /// The row in a data list object that the component is bound to when used in a list.
        /// </summary>
        [CascadingParameter]
        public DataRow Row { get; set; }

        /// <summary>
        /// Access key mnemonic to use for the component.
        /// </summary>
        [Parameter]
        public string AccessKey { get; set; }

        /// <summary>
        /// Additional attributes with their values to be specified on the component.
        /// They usually go on the main element, which depends on the component.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        /// <summary>
        /// Property changes that the component observes in order to update itself.
        /// This is based on which attributes of the property are used in component's rendering.
        /// </summary>
        protected PropertyChange ObservedChanges { get; set; }

        /// <summary>
        /// Shorthand to determine if the property is multivalue.
        /// </summary>
        protected bool IsMultiValue => Property != null && Property.IsMultiValued;

        /// <summary>
        /// Shorthand to determine if the property is required.
        /// </summary>
        protected bool IsRequired => Property != null && Property.Required;

        /// <summary>
        /// Shorthand to determine if the property is editable.
        /// </summary>
        protected bool IsEditable => Property != null && Property.Editable;

        /// <summary>
        /// Shorthand to determine if the property is visible.
        /// </summary>
        protected bool IsVisible => Property != null && Property.Visible;

        /// <summary>
        /// Shorthand to determine if the value of the property is null.
        /// </summary>
        protected bool IsNull => Property != null && Property.IsNull();

        /// <summary>
        /// Constructs a new property bound component.
        /// </summary>
        public XComponent()
        {
            ObservedChanges = PropertyChange.All;
        }

        /// <summary>
        /// Gets a CSS class string that combines the <c>class</c> attribute and property state.
        /// Derived components should typically use this value for the primary HTML element's 'class' attribute.
        /// </summary>
        protected string CssClass
        {
            get
            {
                string propertyState = Property?.GetStateString(ObservedChanges);
                if (AdditionalAttributes != null &&
                    AdditionalAttributes.TryGetValue("class", out var cls) &&
                    !string.IsNullOrEmpty(Convert.ToString(cls)))
                {
                    return $"{cls} {propertyState}";
                }

                return propertyState;
            }
        }

        /// <summary>
        /// Utility method to get a label for the markup based on the specified text,
        /// showing the current access key character.
        /// </summary>
        /// <param name="text">The label text.</param>
        /// <returns>Markup string to be used for the label.</returns>
        protected MarkupString GetLabel(string text)
        {
            if (AccessKey == null || text == null || !text.Contains(AccessKey))
                return (MarkupString)text;
            int idx = text.IndexOf(AccessKey);
            return (MarkupString)text.Remove(idx, 1).Insert(idx, $"<u>{AccessKey}</u>");
        }

        /// <inheritdoc/>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            if (Property != null)
            {
                Property.AsyncChange += OnPropertyChangeAsync;
            }
        }

        /// <summary>
        /// Handles async property change events by refreshing the component, if the change is
        /// obsereved by the component.
        /// </summary>
        /// <param name="sender">Sender of the property change.</param>
        /// <param name="e">Property change details.</param>
        /// <param name="token">Cancellation token.</param>
        protected async virtual Task OnPropertyChangeAsync(object sender, PropertyChangeEventArgs e, CancellationToken token)
        {
            if (e.Change.IncludesChanges(ObservedChanges))
            {
                await InvokeAsync(() =>
                {
                    StateHasChanged();
                });
            }
        }

        /// <summary>
        /// Updates the property with the specified value, and marks it as being edited.
        /// </summary>
        /// <param name="value">The new value for the property</param>
        protected async Task UpdateProperty(object value)
        {
            if (Property != null)
            {
                Property.Editing = true;
                await Property.SetValueAsync(value);
            }
        }

        /// <summary>
        /// Event callback that updates the property value when component's value changes.
        /// It can be used on the corresponding HTML elements in the actual components.
        /// </summary>
        /// <param name="e">Change event arguments.</param>
        protected virtual async Task OnValueChanged(ChangeEventArgs e)
        {
            await UpdateProperty(e.Value);
        }

        /// <summary>
        /// Event callback that sets the property as being edited when component gets focus.
        /// It can be used on the corresponding HTML elements in the actual components.
        /// </summary>
        /// <param name="e">Focus event arguments.</param>
        protected void OnFocus(FocusEventArgs e)
        {
            if (Property != null)
                Property.Editing = true;
        }

        /// <summary>
        /// Event callback that sets the property as not being edited when component loses focus.
        /// It can be used on the corresponding HTML elements in the actual components.
        /// </summary>
        /// <param name="e">Focus event arguments.</param>
        protected void OnBlur(FocusEventArgs e)
        {
            if (Property != null)
                Property.Editing = false;
        }

        /// <summary>
        /// Disposes the component and unbinds from the property.
        /// </summary>
        public void Dispose()
        {
            if (Property != null)
                Property.AsyncChange -= OnPropertyChangeAsync;
        }
    }
}
