// Copyright (c) 2026 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using System.Collections.Specialized;
using System.ComponentModel;
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
            }
            Model = viewModel;
            // refresh the view if binding to a different model (e.g. master-details)
            if (Model != null)
            {
                Model.OnPropertyChanged(new PropertyChangedEventArgs(ViewModel.ViewTitleProperty));
                StateHasChanged();
            }
        }

        /// <summary>
        /// Boolean parameter for whether to activate the view model from the query string.
        /// Used on a top-level view within a page.
        /// </summary>
        [Parameter]
        public bool ActivateFromQuery { get; set; }

        /// <inheritdoc/>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            // workaround for Blazor Server calling this method more than once.
            if (Model == null || Model.Activated && Model.Params.Count > 0 && !ActivateFromQuery)
                return;

            var parameters = new NameValueCollection();
            if (ActivateFromQuery)
            {
                var qry = QueryHelpers.ParseQuery(Navigation.ToAbsoluteUri(Navigation.Uri).Query);
                foreach (var key in qry.Keys)
                {
                    foreach (var val in qry[key])
                        parameters.Add(key, val);
                }
            }

            await Model.ActivateAsync(parameters);
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

        /// <summary>
        /// Utility method to allow adding the specified name-value collection
        /// to the given URI as a query string.
        /// </summary>
        /// <param name="uri">The URI to add to.</param>
        /// <param name="nvc">The names/values to add to the URI as a query string.</param>
        /// <returns>The URI with the query string added to it.</returns>
        public static string AddQueryString(string uri, NameValueCollection nvc)
        {
            string res = uri;
            foreach (var key in nvc.AllKeys)
            {
                var vals = nvc.GetValues(key);
                if (vals == null) continue;
                foreach (string v in vals)
                    res = QueryHelpers.AddQueryString(res, key, v);
            }
            return res;
        }

        #endregion

        /// <summary>
        /// Utility shorthand method to return disabled class for a given state.
        /// </summary>
        /// <param name="enabled">True for enabled state, false for disabled.</param>
        /// <returns>The disabled class, if not enabled, empty string otherwise.</returns>
        public static string DisabledIfNot(bool? enabled) => enabled ?? false ? "" : "disabled";

        #region Show/Close

        /// <inheritdoc/>
        public virtual async Task<bool> CanDeleteAsync(CancellationToken token = default)
            => await Task.FromResult(true);

        /// <summary>
        /// The mode from the view model that it was activated with.
        /// </summary>
        protected virtual string Mode => Model?.Params?[ViewParams.Mode.Param];

        /// <summary>
        /// The outer class to be used for the view, which usually defines the view layout within a parent container.
        /// </summary>
        [Parameter] public string Class { get; set; }

        /// <summary>
        /// The Bootstrap size of the modal dialog for this view, where applicable, which can be overridden in subclasses.
        /// </summary>
        protected virtual string ModalSize => "modal-xl";

        /// <summary>
        /// The class to use for the view's main div.
        /// </summary>
        protected virtual string UpperClass => Mode == ViewParams.Mode.Popup ? "modal" : Mode == ViewParams.Mode.Inline ? $"d-flex border-start {Class}" : "d-flex";

        /// <summary>
        /// The class to use for the view's middle main div.
        /// </summary>
        protected virtual string MiddleClass => Mode == ViewParams.Mode.Popup ? $"modal-dialog {ModalSize}" : "d-flex w-100";

        /// <summary>
        /// The class to use for the view's inner main div.
        /// </summary>
        protected virtual string LowerClass => Mode == ViewParams.Mode.Popup ? $"modal-content" : "flex-column";

        /// <summary>
        /// The class to use for the view's footer, if applicable.
        /// </summary>
        protected virtual string FooterClass => FooterVisible ? "modal-footer" : "d-none";

        /// <summary>
        /// Returns whether to show the footer.
        /// </summary>
        protected virtual bool FooterVisible => (Model?.CloseAction?.Visible ?? false);

        /// <summary>
        /// Indicates if the view is visible.
        /// </summary>
        [Parameter] public bool Visible { get; set; }

        /// <summary>
        /// The preferred number of Bootstrap columns (1–11 out of 12) this view's <b>own content</b>
        /// should occupy within the parent's row when displayed as an inline child view.
        /// When this view has open inline children, its total span in the parent's row is automatically
        /// widened so that own content still gets this many columns; see <see cref="EffectivePreferredColumns"/>.
        /// Views with no preference (0) share the remaining space equally by view count.
        /// </summary>
        [Parameter] public int PreferredColumns { get; set; }

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

		/// <summary>
		/// Determines if the view or any of its child views are modified.
		/// </summary>
		/// <returns>True if the view is modified, false otherwise.</returns>
		public virtual bool IsModified() => ChildViews.Any(v => v != null && v.IsModified());


		/// <summary>
		/// Asynchronously determines if the view can be closed. If the view is modified,
		/// then prompts the user to confirm discarding unsaved changes.
		/// The method can be overridden in subclasses to customize the confirmation prompt.
		/// </summary>
		/// <param name="token">Cancellation token.</param>
		/// <returns>True, if the view can be closed, false otherwise.</returns>
		public virtual async Task<bool> CanCloseAsync(CancellationToken token = default)
		{
			if (IsModified())
			{
				var msg = Model.GetString(Messages.View_UnsavedMessage);
				return await JSRuntime.InvokeAsync<bool>("confirm", token, msg);
			}
			return true;
		}

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
            await DisposeAsync(token);
        }

        /// <summary>
        /// Utility function to show/hide the view as a popup window.
        /// Invokes a corresponding JavaScript function, which can be overridden in subclasses.
        /// </summary>
        /// <param name="show">True to show the view, false to hide it.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Asynchronous task for the function.</returns>
        protected virtual async Task Popup(bool show, CancellationToken token = default)
        {
            // this requires importing xfk-blazor.js in the Host.cshtml.
            // We could use modules from net50 instead, but we target netstandard2.1 for backward compatibility.
            await JSRuntime.InvokeVoidAsync("xfk.modalViewPopup", token, show, MainPanel);
        }

        /// <summary>
        /// Utility function to show/hide the view as an inline panel in the master-detail layout.
        /// Does nothing by default, but can be overridden in subclasses.
        /// </summary>
        /// <param name="show">True to show the view, false to hide it.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Asynchronous task for the function.</returns>
        protected virtual async Task Inline(bool show, CancellationToken token = default) => await Task.CompletedTask;

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
        public virtual async Task DisposeAsync(CancellationToken token)
        {
            BindTo(null);
            foreach (var cv in ChildViews)
            {
                if (cv != null)
                    await cv.CloseAsync(token);
            }
        }

        #endregion

        #region View event handling

        /// <summary>
        /// Provides the base method for handling view events.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">View event</param>
        /// <param name="token">Cancellation token.</param>
        protected virtual async Task OnViewEventsAsync(object sender, ViewEvent e, CancellationToken token = default)
        {
            if (e.IsChild() && (e.IsOpened(false) || e.IsClosed(false)))
                Model.OpenInlineViews = OpenInlineViews;
            await InvokeAsync(() => StateHasChanged());
        }

        /// <summary>
        /// Handles property change for errors to update the Errors panel
        /// </summary>
        /// <param name="sender">Model that sent the event</param>
        /// <param name="e">Event arguments</param>
        protected virtual async void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ViewModel.ViewTitleProperty && TitleComponent != null && Model == sender)
            {
                TitleComponent.SetTitle(Model?.ViewTitle);
                await TitleComponent.Update();
            }
		}

		#endregion

		#region Child views and Layout

		/// <summary>
		///  An array of child views for the current view that are used to properly layout this view.
		/// </summary>
		protected virtual BlazorView[] ChildViews => new BlazorView[0];

        /// <summary>
        /// The number of open inline child views and the current view.
        /// </summary>
        protected int OpenInlineViews => (Visible ? 1 : 0) + ChildViews.Where(v => v?.Mode == ViewParams.Mode.Inline).Sum(v => v.OpenInlineViews);

        /// <summary>
        /// The effective Bootstrap column span this view and its open inline children should occupy
        /// in the direct parent's row. When <see cref="PreferredColumns"/> is set, own content keeps
        /// that many columns; the total span grows to accommodate any open preferred grandchildren.
        /// Computed as <c>PreferredColumns × 12 / selfCol</c>, where <c>selfCol</c> is what this
        /// view's own content would receive within its own row (using the same allocation logic as
        /// <see cref="GetViewCol"/>). Returns 0 when no preference is set.
        /// </summary>
        protected int EffectivePreferredColumns
        {
            get
            {
                if (PreferredColumns <= 0) return 0;

                // Compute what fraction of this view's own row is left for own content
                // after open preferred children have taken their effective share.
                // This mirrors the mainCol calculation in GetViewCol.
                var inlineChildren = ChildViews.Where(v => v?.Mode == ViewParams.Mode.Inline
                                                        && (v.Visible || v.OpenInlineViews > 0));
                int preferredEffectiveSum = inlineChildren.Where(v => v.PreferredColumns > 0)
                                                          .Sum(v => v.EffectivePreferredColumns);
                int unpreferredChildViews = inlineChildren.Where(v => v.PreferredColumns == 0)
                                                          .Sum(v => v.OpenInlineViews);
                int unpreferredViews = 1 + unpreferredChildViews;
                int remaining = Math.Max(unpreferredViews, 12 - preferredEffectiveSum);
                int selfCol = remaining / unpreferredViews;

                // Own content occupies PreferredColumns within the parent's row and selfCol within
                // this view's own row, so the total span scales proportionally.
                return selfCol > 0 ? PreferredColumns * 12 / selfCol : PreferredColumns;
            }
        }

        /// <summary>
        /// Breakpoint widths for the view and all its open inline children
        /// </summary>
        protected BreakpointWidths bpWidths;

        /// <summary>
        /// Breakpoint widths for the view only
        /// </summary>
        protected BreakpointWidths selfWidths;

        /// <summary>
        /// Gets a string of Bootstrap column classes for various breakpoints that would allow to properly display
        /// the given child view at each breakpoint given the current number of open inline views.
        /// Each inline child's <see cref="EffectivePreferredColumns"/> is reserved for that child's subtree,
        /// so the child's own content always occupies its <see cref="PreferredColumns"/> columns regardless
        /// of how many grandchildren it has open. The parent and any children without a preference share
        /// the remaining columns equally by view count.
        /// </summary>
        /// <param name="childView">The child view, for which to return the column classes.</param>
        /// <returns>A string of Bootstrap column classes for various breakpoints for this view.</returns>
        protected string GetViewCol(BlazorView childView)
        {
            if (bpWidths == null)
            {
                bpWidths = new BreakpointWidths(Mode == ViewParams.Mode.Popup ?
                    BreakpointWidths.ModalDefaults : BreakpointWidths.GridDefaults);
            }
            selfWidths = new BreakpointWidths(bpWidths);

            int totalViews = OpenInlineViews;
            if (totalViews <= 1) return "d-flex w-100"; // no visible children

            int currentView = childView?.OpenInlineViews ?? 1;

            // Sum effective columns pre-allocated by preferred inline children.
            // Each child's effective span accounts for its own open grandchildren recursively.
            var inlineChildren = ChildViews.Where(v => v?.Mode == ViewParams.Mode.Inline
                                                    && (v.Visible || v.OpenInlineViews > 0));
            int preferredEffectiveSum = inlineChildren.Where(v => v.PreferredColumns > 0)
                                                      .Sum(v => v.EffectivePreferredColumns);

            // Self (1 view) plus any unpreferred children share the remaining columns by view count.
            int unpreferredViews = 1 + inlineChildren.Where(v => v.PreferredColumns == 0)
                                                      .Sum(v => v.OpenInlineViews);
            int remaining = 12 - preferredEffectiveSum;
            int colPerView = remaining >= unpreferredViews ? remaining / unpreferredViews : 0;

            // When preferred children consume all available columns, the main view and any
            // unpreferred children have no room left, so they are hidden entirely, effectively
            // drilling into the preferred child's details.
            if (colPerView <= 0 && (childView == null || childView.PreferredColumns <= 0))
                return "d-none";

            // Parent always gets colPerView; preferred child gets its effective total span;
            // unpreferred child gets colPerView scaled by its subtree depth.
            int mainCol = colPerView;
            int col = childView == null ? mainCol
                    : childView.PreferredColumns > 0 ? childView.EffectivePreferredColumns
                    : colPerView * currentView;

            // calculate the point to hide the main view based on its column width
            Breakpoint hidePt = mainCol < 4 ? Breakpoint.xxl : mainCol < 6 ? Breakpoint.xl : Breakpoint.lg;

            string res = $"col-{hidePt}-{col} ";

            if (childView == null)
            {
                res += $"d-none d-{hidePt}-flex";
                selfWidths.ApplyColumns(hidePt, 0, col);
            }
            else
            {
                int bkCol = 12 * currentView / (totalViews - 1);
                res += $"col-{bkCol}";
                BreakpointWidths childWidths = new BreakpointWidths(bpWidths);
                childWidths.ApplyColumns(hidePt, bkCol, col);
                childView.bpWidths = childWidths;
            }

            return res;
        }

        /// <summary>
        /// Default field width used for calculating the optimal number of columns to layout the view panel at each breakpoint.
        /// </summary>
        protected int DefaultFieldWidth = 150;

        /// <summary>
        /// Gets a string of Bootstrap classes for various breakpoints that lays out the fields of this view
        /// in the optimal number of columns at each breakpoint, taking into account the specified maximum number of columns
        /// to lay out fields in, and the default width of the fields.
        /// </summary>
        /// <param name="maxCol">The specified maximum number of columns to lay out fields.</param>
        /// <returns>A string of Bootstrap classes for various breakpoints for the view's field columns.</returns>
        protected string GetRowCol(int maxCol) => GetRowCol(maxCol, DefaultFieldWidth);

        /// <summary>
        /// Gets a string of Bootstrap classes for various breakpoints that lays out the fields of this view
        /// in the optimal number of columns at each breakpoint, taking into account the specified maximum number of columns
        /// to lay out fields in and the preferred width of the fields.
        /// </summary>
        /// <param name="maxCol">The specified maximum number of columns to lay out fields.</param>
        /// <param name="fldWidth">The preferred width of the fields.</param>
        /// <returns>A string of Bootstrap classes for various breakpoints for the view's field columns.</returns>
        protected string GetRowCol(int maxCol, int fldWidth) => selfWidths?.GetCols(maxCol, fldWidth)?.ToRowColsClass();

        /// <summary>
        /// Gets a string of Bootstrap column classes for various breakpoints that lays out this view in the parent panel,
        /// taking into account the specified maximum number of columns for the parent panel, the number of columns used
        /// for this view, and the default width of the fields in this view.
        /// </summary>
        /// <param name="maxCol">Maximum number of columns for the parent panel.</param>
        /// <param name="fldCol">The number of columns used for fields in this view.</param>
        /// <returns>A string of Bootstrap column classes for various breakpoints that lays out this view in the parent panel.</returns>
        protected string GetPanelCol(int maxCol, int fldCol) => GetPanelCol(maxCol, fldCol, DefaultFieldWidth);

        /// <summary>
        /// Gets a string of Bootstrap column classes for various breakpoints that lays out this view in the parent panel,
        /// taking into account the specified maximum number of columns for the parent panel, the number of columns used
        /// for this view, and the preferred width of the fields in this view.
        /// </summary>
        /// <param name="maxCol">Maximum number of columns for the parent panel.</param>
        /// <param name="fldCol">The number of columns used for fields in this view.</param>
        /// <param name="fldWidth">The preferred width of the fields in this view.</param>
        /// <returns>A string of Bootstrap column classes for various breakpoints that lays out this view in the parent panel.</returns>
        protected string GetPanelCol(int maxCol, int fldCol, int fldWidth) => selfWidths?.GetCols(maxCol, fldCol * fldWidth)?.ToColClass();

        #endregion
    }
}