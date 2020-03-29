// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Xomega.Framework.Views;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// Base class for ASP.NET views.
    /// A view can contain other child views that can be shown or hidden dynamically.
    /// </summary>
    /// <remarks>
    /// As a convention, each view in a composition should be wrapped in a container panel that represents the view in it.
    /// </remarks>
    public class WebView : UserControl, IView, IAsyncView
    {

        #region Initialization

        /// <summary>
        /// The service provider for the view
        /// </summary>
        public IServiceProvider ServiceProvider { get { return WebDI.CurrentServiceScope?.ServiceProvider; } }

        /// <summary>
        /// Model the view is bound to. Should be pre-created by subclasses.
        /// </summary>
        public ViewModel Model { get; protected set; }

        /// <summary>
        /// Initializes the already constructed view model for each request
        /// </summary>
        /// <param name="e">Standard event arguments</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // call page's method for initial state here,
            // once the page has been set and viewstate loaded
            WebUtil.SetControlVisible(this, Visible);
            if (btn_Close != null)
                btn_Close.Click += Close;

            if (Model == null) return;
            // restore model data from view state
            Model.Params[ViewParams.Mode.Param] = Mode;
            Model.Params[ViewParams.QuerySource] = ParentSource;
            Model.Params[ViewParams.SelectionMode.Param] = SelectionMode;
            ErrorList viewErrors = Errors;
            if (viewErrors != null) Model.Errors = viewErrors;
            // bind to the model
            BindTo(Model);
        }

        #endregion

        #region View params stored in view state

        /// <summary>Mode property that stores display mode view model was activated with.</summary>
        protected string Mode
        {
            get { return ViewState["Mode"] as string; }
            set
            {
                if (value == null) ViewState.Remove("Mode");
                else ViewState["Mode"] = value;
            }
        }

        /// <summary>
        /// Stores the value passed in the source query parameter that can be used in parent callbacks
        /// to identify which link invoked this view when the parent has multiple links to the same view.
        /// </summary>
        protected string ParentSource
        {
            get { return ViewState["ParentSource"] as string; }
            set
            {
                if (value == null) ViewState.Remove("ParentSource");
                else ViewState["ParentSource"] = value;
            }
        }

        /// <summary>SelectionMode property that stores selection mode view model was activated with.</summary>
        protected string SelectionMode
        {
            get { return ViewState["SelectionMode"] as string; }
            set
            {
                if (value == null) ViewState.Remove("SelectionMode");
                else ViewState["SelectionMode"] = value;
            }
        }

        /// <summary>
        /// Generates a key for storing objects in the session for the current view.
        /// </summary>
        /// <param name="key">The object key to generate the session key for.</param>
        /// <returns>The generated unique session key.</returns>
        protected string GetSessionKey(string key)
        {
            return string.Format("{0}|{1}|{2}", Request.CurrentExecutionFilePath, UniqueID, key);
        }

        /// <summary>The key for persisting error list in the web session.</summary>
        private string ErrorsKey { get { return GetSessionKey("Errors"); } }

        /// <summary>The error list of the view model persisted in the web session.</summary>
        protected ErrorList Errors
        {
            get { return Session[ErrorsKey] as ErrorList; }
            set
            {
                if (value == null) Session.Remove(ErrorsKey);
                else Session[ErrorsKey] = value;
            }
        }
        #endregion

        #region Controls

        /// <summary>
        /// Main update panel for the view
        /// </summary>
        protected UpdatePanel upl_Main;

        /// <summary>
        /// The root view panel within the update panel
        /// </summary>
        protected WebControl pnl_View;

        /// <summary>
        /// Close button
        /// </summary>
        protected Label lbl_ViewTitle;

        /// <summary>
        /// Close button
        /// </summary>
        protected Button btn_Close;

        #endregion

        #region Binding

        /// <inheritdoc/>
        public virtual void BindTo(ViewModel model)
        {
            bool bind = model != null;
            ViewModel vm = bind ? model : this.Model;
            if (vm != null)
            {
                // display Close button only if the view is activated as a child (popup or inline)
                if (btn_Close != null && bind && vm.Params != null)
                    btn_Close.Visible = (vm.Params[ViewParams.Mode.Param] != null);

                if (bind)
                {
                    Mode = vm.Params[ViewParams.Mode.Param];
                    ParentSource = vm.Params[ViewParams.QuerySource];
                    SelectionMode = vm.Params[ViewParams.SelectionMode.Param];
                    vm.ViewEvents += OnViewEvents;
                    vm.AsyncViewEvents += OnViewEventsAsync;
                    vm.PropertyChanged += OnModelPropertyChanged;
                    vm.View = this;
                }
                else
                {
                    vm.ViewEvents -= OnViewEvents;
                    vm.AsyncViewEvents -= OnViewEventsAsync;
                    vm.PropertyChanged -= OnModelPropertyChanged;
                    vm.View = null;
                    Mode = null;
                    ParentSource = null;
                }
                OnModelPropertyChanged(bind ? vm : null, new PropertyChangedEventArgs(ViewModel.ErrorsProperty));
                OnModelPropertyChanged(bind ? vm : null, new PropertyChangedEventArgs(ViewModel.ViewTitleProperty));
            }
            Model = model;
        }

        /// <summary>
        /// Presenter of the current error list
        /// </summary>
        protected IErrorPresenter ucl_Errors;

        /// <summary>
        /// Handles property change for errors to update the Errors panel
        /// </summary>
        /// <param name="sender">Model that sent the event</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ViewModel vm = sender as ViewModel;
            if (e.PropertyName == ViewModel.ErrorsProperty)
            {
                if (vm != null) Errors = vm.Errors;
                ucl_Errors?.Show(vm?.Errors);
            }
            else if (e.PropertyName == ViewModel.ViewTitleProperty)
            {
                if (lbl_ViewTitle != null)
                    lbl_ViewTitle.Text = vm?.ViewTitle;
            }
        }

        #endregion

        #region Show/Close with JavaScript

        /// <inheritdoc/>
        public virtual bool CanDelete() => true;

        /// <inheritdoc/>
        public virtual async Task<bool> CanDeleteAsync(CancellationToken token = default)
            => await Task.FromResult(CanDelete());

        /// <summary>Script to popup a modal dialog for a view.</summary>
        protected string Script_ModalDialog = @"if (xomegaControls && typeof xomegaControls._modalViewPopup === 'function')
            xomegaControls._modalViewPopup('{0}', '{1}', '{2}', {3});";

        /// <summary>Script to udpate view visibility in a split panel.</summary>
        protected string Script_Splitter_OnViewVisibilityChange = @"if (xomegaControls && typeof xomegaControls._vSplitViewVisibilityChange === 'function')
            xomegaControls._vSplitViewVisibilityChange('{0}');";

        /// <summary>Script to split a panel vertically.</summary>
        protected string Script_Splitter = @"if (xomegaControls && typeof xomegaControls._vSplitViewPanel === 'function')
            xomegaControls._vSplitViewPanel('{0}');";

        /// <summary>Shortcut to get the main view panel</summary>
        protected virtual Control ViewPanel { get { return Controls[0]; } }

        /// <inheritdoc/>
        public virtual bool Show()
        {
            WebUtil.SetControlVisible(this, true);
            UpdatePanel upl = WebUtil.FindParentUpdatePanel(this);
            if (upl != null) upl.Update();

            switch (Mode)
            {
                case ViewParams.Mode.Popup:
                    RegisterStartupScript("Dialog", Script_ModalDialog, "show", ViewPanel.ClientID, upl != null ? upl.ClientID : "", "false");
                    break;
                case ViewParams.Mode.Inline:
                    RegisterStartupScript("Visible", Script_Splitter_OnViewVisibilityChange, upl.ClientID);
                    break;
            }
            return true;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ShowAsync(CancellationToken token = default)
            => await Task.FromResult(Show());

        /// <inheritdoc/>
        public bool CanClose() => true;

        /// <inheritdoc/>
        public virtual async Task<bool> CanCloseAsync(CancellationToken token = default)
            => await Task.FromResult(CanClose());

        /// <inheritdoc/>
        public void Close()
        {
            DoClose();
            Model.FireEvent(ViewEvent.Closed);
            Dispose();
        }

        // The actual closing routine.
        private void DoClose()
        {
            WebUtil.SetControlVisible(this, false);
            UpdatePanel upl = WebUtil.FindParentUpdatePanel(this), uplHost = null;
            if (upl != null)
            {
                upl.Update();
                uplHost = WebUtil.FindParentUpdatePanel(upl);
                if (uplHost != null) uplHost.Update();
            }

            switch (Mode)
            {
                case ViewParams.Mode.Popup:
                    RegisterStartupScript("Dialog", Script_ModalDialog, "hide", ViewPanel.ClientID, upl != null ? upl.ClientID : "", uplHost != null ? "true" : "false");
                    break;
                case ViewParams.Mode.Inline:
                    RegisterStartupScript("Visible", Script_Splitter_OnViewVisibilityChange, upl.ClientID);
                    break;
            }
        }

        /// <inheritdoc/>
        public virtual async Task CloseAsync(CancellationToken token = default)
        {
            DoClose();
            await Model.FireEventAsync(ViewEvent.Closed, token);
            Dispose();
        }

        /// <summary>
        /// Default handler for closing the view.
        /// </summary>
        protected virtual void Close(object sender, EventArgs e)
        {
            if (!CanClose() || Model == null) return;
            Close();
        }

        /// <summary>
        /// Disposes the view by unbinding it from the model
        /// </summary>
        public override void Dispose()
        {
            BindTo(null);
        }

        /// <summary>
        /// Sets up a splitter to split the view for master-details layout.
        /// </summary>
        public virtual void Split()
        {
            RegisterStartupScript("vSplitterPanel", Script_Splitter, ViewPanel.ClientID);
        }

        /// <summary>
        /// Utility function to register a startup script for the current view.
        /// </summary>
        /// <param name="key">Unique key to use with the view panel ID when registering the script.</param>
        /// <param name="script">JavaScript text with placeholders.</param>
        /// <param name="args">Arguments for the placeholders.</param>
        protected void RegisterStartupScript(string key, string script, params object[] args)
        {
            WebUtil.RegisterStartupScript(ViewPanel, key, script, args);
        }

        #endregion

        #region Handling child view updates

        /// <summary>
        /// Subscribes to child views' events
        /// </summary>
        /// <param name="children">Child views</param>
        protected virtual void SubscribeToChildEvents(params WebView[] children)
        {
            if (Model == null) return;
            foreach (WebView child in children)
                if (child != null) Model.SubscribeToChildEvents(child.Model);
        }

        /// <summary>
        /// Provides the base method for handling view events
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">View event</param>
        protected virtual void OnViewEvents(object sender, ViewEvent e)
        {
            if (upl_Main != null) upl_Main.Update();
        }

        /// <summary>
        /// Provides the base method for handling view events
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">View event</param>
        /// <param name="token">Cancellation token.</param>
        protected virtual async Task OnViewEventsAsync(object sender, ViewEvent e, CancellationToken token = default)
        {
            if (upl_Main != null) upl_Main.Update();
            await Task.CompletedTask;
        }

        #endregion
    }
}