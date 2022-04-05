// Copyright (c) 2021 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xomega.Framework.Blazor.Controls
{
    /// <summary>
    /// Auto-complete component that binds to a data property (usually <see cref="Properties.EnumProperty"/>)
    /// and allows typing and selecting values using the property's <see cref="DataProperty.AsyncItemsProvider"/>.
    /// This component properly handles multi-value properties, allowing to select each individual value from a list.
    /// </summary>
    partial class XAutoComplete : IAsyncDisposable
    {
        /// <summary>
        /// JavaScript runtime.
        /// </summary>
        [Inject] protected IJSRuntime JsRuntime { get; set; }

        /// <summary>
        /// Reference to the input text DOM element.
        /// </summary>
        protected ElementReference Input { get; set; }

        /// <summary>
        /// Gets the maximum length for the text box element, if applicable.
        /// </summary>
        protected int? MaxLength => IsMultiValue ? null : Property?.Size;

        /// <summary>
        /// Gets the value for the text box element from the property.
        /// </summary>
        protected string Value => Property?.GetStringValue(IsEditable ? ValueFormat.EditString : ValueFormat.DisplayString, Row);

        /// <summary>
        /// A flag that keeps track of whether or not the auto-complete dropdown is visible.
        /// </summary>
        protected bool DropDownVisible { get; set; } = false;

        /// <summary>
        /// The index of the currently selected item in the dropdown list.
        /// </summary>
        protected int ActiveItemIndex { get; set; }

        /// <summary>
        /// The current text entered in the text element.
        /// </summary>
        protected string CurrentText { get; set; }

        /// <summary>
        /// A timer to delay the onBlur callback to allow preventing such execution
        /// when the users clicks on an item in the open dropdown, since onBlur is triggered before the click.
        /// </summary>
        protected System.Timers.Timer onBlurTimer;

        /// <summary>
        /// Sets up text element to prevent default actions.
        /// </summary>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender && Input.Id != null)
                await JsRuntime.InvokeVoidAsync("xfk.autoCompletePreventDefault", Input);
        }

        /// <summary>
        /// Cleans up text element's setup to prevent default actions.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (Input.Id != null)
                await JsRuntime.InvokeVoidAsync("xfk.autoCompletePreventDefault", Input, true);
        }

        /// <inheritdoc/>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            CurrentText = Property?.GetStringValue(ValueFormat.EditString, Row);
        }

        /// <summary>
        /// Sets the current text whenever the property value changes.
        /// </summary>
        protected override void OnPropertyChange(PropertyChangeEventArgs e)
        {
            base.OnPropertyChange(e);
            if (e.Change.IncludesValue())
                CurrentText = Property.GetStringValue(ValueFormat.EditString, Row);
        }

        /// <summary>
        /// Updates the current text and the list of values in the dropdown
        /// when the user changes anything in the text element.
        /// </summary>
        /// <param name="e">Change event arguments.</param>
        protected async Task OnInput(ChangeEventArgs e)
        {
            CurrentText = e.Value?.ToString();
            await UpdateListIfNeeded();
        }

        /// <summary>
        /// Handles KeyDown event from the user to either open the list of suggestions on Ctrl+Space or Alt+Down,
        /// close the dropdown list on Esc or moving Left/Right,
        /// or to navigate the open suggestion list with Up/Down arrows and selecting an item with Enter.
        /// </summary>
        /// <param name="args">Key event arguments.</param>
        protected async Task OnKeyDown(KeyboardEventArgs args)
        {
            if (DropDownVisible)
            {
                if (args.Code == "Escape" || args.Code == "ArrowLeft" || args.Code == "ArrowRight")
                    CloseDropDown();

                if (args.Code == "ArrowUp" && ActiveItemIndex > 0)
                    ActiveItemIndex -= 1;

                if (args.Code == "ArrowDown" && ActiveItemIndex < AvailableItems.Count - 1)
                    ActiveItemIndex += 1;

                if (args.Code == "Enter")
                    await SelectItem(ActiveItemIndex);
            }
            else
            {
                if (args.CtrlKey && args.Code == "Space" || args.AltKey && args.Code == "ArrowDown")
                {
                    await UpdateListIfNeeded();
                    ShowDropDown(); // always show, even if the term hasn't changed
                }
            }
        }

        /// <summary>
        /// Updates the list of suggestions for the current term, and shows the dropdown, if there are any suggestions.
        /// </summary>
        protected async Task UpdateListIfNeeded()
        {
            if (Property == null) return;
            var text = CurrentText ?? "";
            string term = text;
            if (IsMultiValue)
            {
                var pos = await JsRuntime.InvokeAsync<int>("xfk.getProperty", Input, "selectionStart");
                pos = Math.Min(pos, text.Length);
                int start = Property.ParseListSeparators.Select(s => text[0..pos].LastIndexOf(s)).Max() + 1;
                int end = Property.ParseListSeparators.Select(s => text.IndexOf(s, pos)).Min(i => i < 0 ? text.Length : i);
                term = text[start..end];
            }
            term = term.Trim();
            await GetAvailableItems(term);
            ActiveItemIndex = 0;
            ShowDropDown();
        }

        /// <summary>
        /// Selects the suggested item at the specified index, and inserts it in the text field at the current position.
        /// </summary>
        /// <param name="itemIdx">The index of the item to select.</param>
        protected async Task SelectItem(int itemIdx)
        {
            if (Property == null || itemIdx < 0 || itemIdx >= AvailableItems.Count) return;

            if (onBlurTimer != null) onBlurTimer.Stop(); // we lost focus by clicking on a dropdown item

            var text = CurrentText ?? "";

            if (IsMultiValue)
            {
                object val = await Property.ResolveValueAsync(AvailableItems[itemIdx], ValueFormat.EditString, Row);
                if (val is IList lst && lst.Count > 0) val = lst[0];
                var pos = await JsRuntime.InvokeAsync<int>("xfk.getProperty", Input, "selectionStart");
                pos = Math.Min(pos, text.Length);
                int start = Property.ParseListSeparators.Select(s => text[0..pos].LastIndexOf(s)).Max() + 1;
                int end = Property.ParseListSeparators.Select(s => text.IndexOf(s, pos)).Min(i => i < 0 ? text.Length : i);
                string str = val.ToString();
                var newVal = text[0..start] + str + text[end..text.Length];
                await UpdatePropertyAsync(newVal);
                var newPos = start + str.Length + 1; // position to the end of the entered value
                await JsRuntime.InvokeVoidAsync("xfk.setProperty", Input, "selectionStart", newPos);
                await JsRuntime.InvokeVoidAsync("xfk.setProperty", Input, "selectionEnd", newPos);
            }
            else
            {
                object val = await Property.ResolveValueAsync(AvailableItems[itemIdx], ValueFormat.EditString, Row);
                await UpdatePropertyAsync(val);
            }
            CloseDropDown();
        }

        /// <summary>
        /// Handles lost focus event for the text field by closing the dropdown and updating the property 
        /// with the current value, all done in a delayed manner to allow cancellation in case the focus
        /// was lost due to clicking on a suggested item in the dropdown.
        /// </summary>
        /// <param name="e">Focus event arguments.</param>
        protected void OnFocusOut(FocusEventArgs e)
        {
            // run OnBlur with a delay, since the user may have clicked on a dropdown item
            if (onBlurTimer == null)
            {
                onBlurTimer = new System.Timers.Timer(200);
                onBlurTimer.Elapsed += OnBlurTimer_Elapsed;
                onBlurTimer.AutoReset = false;
            }
            onBlurTimer.Start();
        }

        private void OnBlurTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            InvokeAsync(async () => {
                CloseDropDown();
                await UpdatePropertyAsync(CurrentText);
                Property?.SetEditing(false, Row);
            });
        }

        /// <summary>
        /// Shows dropdown if there are any suggestions.
        /// </summary>
        protected void ShowDropDown()
        {
            DropDownVisible = AvailableItems.Count > 0;
        }

        /// <summary>
        /// Closes the dropdown and resets the active item index.
        /// </summary>
        protected void CloseDropDown()
        {
            DropDownVisible = false;
            ActiveItemIndex = 0;
        }
    }
}