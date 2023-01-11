// Copyright (c) 2022 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace Xomega.Framework.Blazor
{
    /// <summary>
    /// Base class for data property bound components.
    /// </summary>
    public class XComponent : ComponentBase, IDisposable
    {
        /// <summary>
        /// The base property for the component.
        /// </summary>
        protected BaseProperty property;

        /// <summary>
        /// The data property the component is bound to.
        /// </summary>
        [Parameter] public DataProperty Property { get => property as DataProperty; set => property = value; }

        /// <summary>
        /// The row in a data list object that the component is bound to when used in a list.
        /// </summary>
        [CascadingParameter] public DataRow Row { get; set; }

        /// <summary>
        /// Blazor edit context associated with the current component.
        /// </summary>
        [CascadingParameter] public EditContext EditContext { get; set; }

        /// <summary>
        /// Whether or not to show component label.
        /// </summary>
        [Parameter] public bool ShowLabel { get; set; } = true;

        /// <summary>
        /// Custom placeholder for the component.
        /// </summary>
        [Parameter] public string Placeholder { get; set; }

        /// <summary>
        /// Placeholder text to use based on the custom placeholder or label, if applicable.
        /// </summary>
        protected string PlaceholderText => Placeholder ?? (ShowLabel ? Label : Property?.NullString);

        /// <summary>
        /// Access key mnemonic to use for the component.
        /// </summary>
        [Parameter] public string AccessKey { get; set; }

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
        protected bool IsEditable => property != null && property.GetEditable(Row);

        /// <summary>
        /// Shorthand to determine if the property is visible.
        /// </summary>
        protected bool IsVisible => property != null && property.Visible;

        /// <summary>
        /// Shorthand to determine if the value of the property is null.
        /// </summary>
        protected bool IsNull => Property != null && Property.IsNull(Row);

        /// <summary>
        /// Shorthand to get the text for property label.
        /// </summary>
        protected virtual string Label => property?.Label;

        /// <summary>
        /// Shorthand to get the text for property's validation errors.
        /// </summary>
        public string ErrorsText => IsEditable && IsVisible ? Property?.GetValidationErrors(Row)?.ErrorsText : "";

        /// <summary>
        /// The Bootstrap class for the component error.
        /// </summary>
        public string ErrorsClass => string.IsNullOrEmpty(ErrorsText) ? "" : "invalid-feedback";

        /// <summary>
        /// Constructs a new property bound component.
        /// </summary>
        public XComponent()
        {
            ObservedChanges = PropertyChange.All;
        }

        /// <summary>
        /// The property state descriptions used to generate component's CSS classes.
        /// </summary>
        protected PropertyStateDescription StateDescriptions = new PropertyStateDescription()
        {
            Valid = "is-valid",
            Invalid = "is-invalid",
        };

        /// <summary>
        /// The top-level class to use for the component, usually defining its layout in the parent container.
        /// </summary>
        [Parameter] public string Class { get; set; }

        /// <summary>
        /// Gets a CSS class string for the property state based on the currently observed changes.
        /// </summary>
        protected string StateClass => StateDescriptions.GetStateDescription(Property, ObservedChanges, Row);

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

        /// <summary>
        /// Internal auto-generated ID that can be used to associate labels to controls.
        /// </summary>
        protected string ControlId { get; set; } = "xc_" + Guid.NewGuid().ToString("N");

        /// <inheritdoc/>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            if (property != null)
            {
                property.Change += OnPropertyChange;
                if (AccessKey == null)
                    AccessKey = property.AccessKey;
            }
        }

        /// <summary>
        /// Handles property change events.
        /// </summary>
        /// <param name="sender">Sender of the property change.</param>
        /// <param name="e">Property change details.</param>
        protected void OnPropertyChange(object sender, PropertyChangeEventArgs e)
        {
            InvokeAsync(() => OnPropertyChange(e));
        }

        /// <summary>
        /// Handles property change events by refreshing the component, if the change is observed by the component.
        /// </summary>
        /// <param name="e">Property change details.</param>
        protected virtual void OnPropertyChange(PropertyChangeEventArgs e)
        {
            if (e.Change.IncludesChanges(ObservedChanges))
                StateHasChanged();

            if (EditContext != null)
            {
                if (e.Change.IncludesValidation())
                    EditContext.NotifyValidationStateChanged();
                if (e.Change.IncludesValue())
                    EditContext.NotifyFieldChanged(EditContext.Field(Property.Name));
            }
        }

        /// <summary>
        /// Updates the property with the specified value, and marks it as being edited.
        /// </summary>
        /// <param name="value">The new value for the property</param>
        protected async Task UpdatePropertyAsync(object value)
        {
            if (Property != null)
            {
                Property.SetEditing(true, Row);
                await Property.SetValueAsync(value, Row);
            }
        }

        /// <summary>
        /// Event callback that updates the property value when component's value changes.
        /// It can be used on the corresponding HTML elements in the actual components.
        /// </summary>
        /// <param name="e">Change event arguments.</param>
        protected virtual async Task OnValueChanged(ChangeEventArgs e)
        {
            await UpdatePropertyAsync(e.Value);
        }

        /// <summary>
        /// Event callback that sets the property as being edited when component gets focus.
        /// It can be used on the corresponding HTML elements in the actual components.
        /// </summary>
        /// <param name="e">Focus event arguments.</param>
        protected void OnFocus(FocusEventArgs e)
        {
            if (Property != null)
                Property.SetEditing(true, Row);
        }

        /// <summary>
        /// Event callback that sets the property as not being edited when component loses focus.
        /// It can be used on the corresponding HTML elements in the actual components.
        /// </summary>
        /// <param name="e">Focus event arguments.</param>
        protected void OnBlur(FocusEventArgs e)
        {
            if (Property != null)
                Property.SetEditing(false, Row);
        }

        /// <summary>
        /// Disposes the component and unbinds from the property.
        /// </summary>
        public virtual void Dispose()
        {
            if (property != null)
                property.Change -= OnPropertyChange;
        }
    }
}
