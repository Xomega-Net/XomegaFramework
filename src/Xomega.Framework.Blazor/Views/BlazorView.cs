// Copyright (c) 2021 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xomega.Framework.Blazor.Components;
using Xomega.Framework.Views;

namespace Xomega.Framework.Blazor.Views
{
    /// <summary>
    /// Base class for blazor views within Xomega Framework.
    /// </summary>
    public class BlazorView : ComponentBase, IAsyncView
    {
        #region Initialization

        /// <summary>
        /// Injected instance of JavaScript runtime for client-side calls.
        /// </summary>
        [Inject] protected IJSRuntime JSRuntime { get; set; }

        /// <summary>
        /// Injected instance of the navigation service.
        /// </summary>
        [Inject] protected NavigationManager Navigation { get; set; }

        /// <summary>
        /// A reference to the title component for the view.
        /// </summary>
        protected ViewTitle TitleComponent { get; set; }

        /// <summary>
        /// A reference to the main panel element in the view used for client-side logic to show/hide the view.
        /// </summary>
        protected ElementReference MainPanel { get; set; }

        /// <summary>View model the view is bound to.</summary>
        public ViewModel Model { get; protected set; }

        /// <inheritdoc/>
        public virtual void BindTo(ViewModel viewModel)
        {
            bool bind = viewModel != null;
            ViewModel vm = bind ? viewModel : this.Model;
            if (vm != null)
            {
                if (bind)
                {
                    vm.AsyncViewEvents += OnViewEventsAsync;
                    vm.PropertyChanged += OnModelPropertyChanged;
                    vm.View = this;
                }
                else
                {
                    vm.AsyncViewEvents -= OnViewEventsAsync;
                    vm.PropertyChanged -= OnModelPropertyChanged;
                    vm.View = null;
                }
                OnModelPropertyChanged(bind ? vm : null, new PropertyChangedEventArgs(ViewModel.ViewTitleProperty));
            }
            Model = viewModel;
            // refresh the view if binding to a different model (e.g. master-details)
            if (Model != null) StateHasChanged();
        }

        /// <summary>
        /// Boolean parameter for whether or not to activate the view model from the query string.
        /// Used on a top-level view within a page.
        /// </summary>
        [Parameter]
        public bool ActivateFromQuery { get; set; }

        /// <inheritdoc/>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            if (ActivateFromQuery && Model != null && !Model.Activated)
            {
                var parameters = new NameValueCollection();
                var qry = QueryHelpers.ParseQuery(Navigation.ToAbsoluteUri(Navigation.Uri).Query);
                foreach (var key in qry.Keys)
                {
                    foreach (var val in qry[key])
                        parameters.Add(key, val);
                }
                await Model.ActivateAsync(parameters);
            }
        }

        private Func<Task> AfterRenderAction;

        /// <inheritdoc/>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (AfterRenderAction != null && MainPanel.Id != null)
            {
                var action = AfterRenderAction;
                AfterRenderAction = null; // clear before call
                await action();
            }
        }

        #endregion

        #region Selection support

        /// <summary>
        /// Helper function returning the class for selected rows.
        /// </summary>
        /// <param name="list">List object of the row.</param>
        /// <param name="row">The data row.</param>
        /// <returns>Row's CSS class based on whether or not the row is selected.</returns>
        protected virtual string SelectedClass(DataListObject list, DataRow row) => row.Selected ? "selected" : "";

        /// <summary>
        /// Helper function returning the class for selectable lists.
        /// </summary>
        /// <param name="list">List object to check.</param>
        /// <returns>Row's CSS class based on whether or not the list allows selection.</returns>
        protected virtual string SelectableClass(DataListObject list)
            => list?.RowSelectionMode != null ? "selectable" : "";

        /// <summary>
        /// Handles row selection on click for the specified list object, if the latter is selectable.
        /// </summary>
        /// <param name="list">The context list object.</param>
        /// <param name="row">The row that was clicked.</param>
        protected virtual void RowClicked(DataListObject list, DataRow row)
        {
            if (SelectableClass(list) != "")
                list.ToggleSelection(row);
        }

        #endregion

        #region Show/Close

        /// <inheritdoc/>
        public virtual async Task<bool> CanDeleteAsync(CancellationToken token = default)
            => await Task.FromResult(true);

        /// <summary>
        /// The mode from the view model that it was activated with.
        /// </summary>
        protected virtual string Mode => Model?.Params?[ViewParams.Mode.Param];

        /// <summary>
        /// Indicates if the view is visible.
        /// </summary>
        [Parameter]
        public bool Visible { get; set; } = true;

        /// <inheritdoc/>
        public virtual async Task<bool> ShowAsync(CancellationToken token = default)
        {
            Visible = true;
            await InvokeAsync(() => { StateHasChanged(); });
            switch (Mode)
            {
                case ViewParams.Mode.Popup:
                    AfterRenderAction = async () => await Popup(true, token);
                    break;
                case ViewParams.Mode.Inline:
                    AfterRenderAction = async () => await Inline(true, token);
                    break;
            }
            return true;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> CanCloseAsync(CancellationToken token = default)
            => await Task.FromResult(true);

        /// <summary>
        /// Text for the Close button that can be overridden in subclasses.
        /// </summary>
        protected virtual string CloseText => "Close";

        /// <summary>
        /// Determines whether or not the Select button is visible, which can be overridden in subclasses.
        /// Displays Close button only if the view is activated as a child (popup or inline).
        /// </summary>
        protected virtual bool CloseVisible => Mode != null;

        /// <inheritdoc/>
        public virtual async Task CloseAsync(CancellationToken token = default)
        {
            Visible = false;
            switch (Mode)
            {
                case ViewParams.Mode.Popup:
                    await Popup(false, token);
                    break;
                case ViewParams.Mode.Inline:
                    await Inline(false, token);
                    break;
            }
            if (Model != null)
                await Model.FireEventAsync(ViewEvent.Closed, token);
            Dispose();
        }

        /// <summary>
        /// Utility function to show/hide the view as a popup window.
        /// Invokes a corresponding JavaScript function from the XomegaJS package, which can be overridden in subclasses.
        /// </summary>
        /// <param name="show">True to show the view, false to hide it.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Asynchrounous task for the function.</returns>
        protected virtual async Task Popup(bool show, CancellationToken token = default)
            => await JSRuntime.InvokeVoidAsync("xomegaControls._modalViewPopup", token, show ? "show" : "hide", MainPanel);


        /// <summary>
        /// Utility function to show/hide the view as an inline panel in the master-detail layout.
        /// Invokes a corresponding JavaScript function from the XomegaJS package, which can be overridden in subclasses.
        /// </summary>
        /// <param name="show">True to show the view, false to hide it.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Asynchrounous task for the function.</returns>
        protected virtual async Task Inline(bool show, CancellationToken token = default)
            => await JSRuntime.InvokeVoidAsync("xomegaControls.vSplitViewVisibilityChange", token, MainPanel, null, show);

        /// <summary>
        /// Sets up a splitter to split the view for master-details layout.
        /// Invokes a corresponding JavaScript function from the XomegaJS package, which can be overridden in subclasses.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Asynchrounous task for the function.</returns>
        public virtual async Task Split(CancellationToken token = default)
            => await JSRuntime.InvokeVoidAsync("xomegaControls.vSplitViewPanel", token, MainPanel);

        /// <summary>
        /// Default handler for closing the view.
        /// </summary>
        protected virtual async Task OnCloseAsync(MouseEventArgs e)
        {
            // prevent double execution from dialogs
            if (!Visible || !await CanCloseAsync() || Model == null) return;
            await CloseAsync();
        }

        /// <inheritdoc/>
        public virtual void Dispose() => BindTo(null);

        #endregion

        #region View event handling

        /// <summary>
        /// Provides the base method for handling view events.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">View event</param>
        /// <param name="token">Cancellation token.</param>
        protected virtual async Task OnViewEventsAsync(object sender, ViewEvent e, CancellationToken token = default)
            => await InvokeAsync(() => StateHasChanged());

        /// <summary>
        /// Handles property change for errors to update the Errors panel
        /// </summary>
        /// <param name="sender">Model that sent the event</param>
        /// <param name="e">Event arguments</param>
        protected virtual async void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ViewModel.ViewTitleProperty && TitleComponent != null && sender is ViewModel vm)
            {
                TitleComponent.SetTitle(vm.ViewTitle);
                await TitleComponent.Update();
            }
        }

        #endregion
    }
}