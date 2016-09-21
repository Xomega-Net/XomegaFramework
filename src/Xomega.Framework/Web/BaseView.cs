// Copyright (c) 2016 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.UI;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// Base class for ASP.NET views.
    /// A view can contain other child views that can be shown or hidden dynamically.
    /// </summary>
    /// <remarks>
    /// As a convention, each view in a composition should be wrapped in a container panel that represents the view in it.
    /// </remarks>
    public abstract class BaseView : UserControl
    {
        #region Initialization/Activation

        /// <summary>
        /// Close button
        /// </summary>
        protected Control btn_Close;

        /// <summary>
        /// Initializes view data objects
        /// </summary>
        /// <param name="createNew">If true, creates new objects</param>
        protected abstract void InitObjects(bool createNew);

        /// <summary>
        /// Initializes the view for each request
        /// </summary>
        /// <param name="e">Standard event arguments</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            InitObjects(false);
            BindObjects();
        }

        /// <summary>
        /// Binds view to the data objects
        /// </summary>
        protected virtual void BindObjects()
        {
            // implemented by subclasses
        }

        /// <summary>
        /// Activates the view
        /// </summary>
        /// <param name="query">Parameters to activate the view with</param>
        /// <returns>True if the view was successfully activated, False otherwise</returns>
        public virtual bool Activate(NameValueCollection query)
        {
            ParentSource = query[QuerySource];
            // display Close button only if the view is activated as a child (popup or inline)
            if (btn_Close != null) btn_Close.Visible = (Mode != null);
            return true;
        }

        /// <summary>
        /// Query parameter indicating action to perform
        /// </summary>
        public const string QueryAction = "_action";

        /// <summary>
        /// Action to create a new object
        /// </summary>
        public const string ActionCreate = "create";

        /// <summary>
        /// Navigate from the current view to the specified view.
        /// </summary>
        /// <param name="view">View to navigate to</param>
        /// <param name="query">Query parameters to pass</param>
        /// <param name="mode">Navigation mode (BaseView.ModePopup or BaseView.ModeInline)</param>
        public void NavigateTo(BaseView view, NameValueCollection query, string mode = null)
        {
            view.Mode = mode;
            if (view.Activate(query))
                view.Show();
        }

        /// <summary>
        /// Query parameter indicating specific source link on the parent that invoked this view
        /// </summary>
        public const string QuerySource = "_source";

        /// <summary>
        /// Stores the value passed in the source query parameter that can be used in parent callbacks
        /// to identify which link invoked this view when the parent has multiple links to the same view.
        /// </summary>
        public string ParentSource
        {
            get { return ViewState["ParentSource"] as string; }
            set
            {
                if (value == null) ViewState.Remove("ParentSource");
                else ViewState["ParentSource"] = value;
            }
        }
        #endregion

        #region Show/Hide with JavaScript

        /// <summary>
        /// Occurs when the view is closed
        /// </summary>
        public event EventHandler Closed;

        /// <summary>Script to popup a modal dialog for a view.</summary>
        protected string Script_ModalDialog = "if (typeof modalViewPopup === 'function') modalViewPopup('{0}', '{1}', '{2}', {3});";

        /// <summary>Script to udpate view visibility in a split panel.</summary>
        protected string Script_Splitter_OnViewVisibilityChange = "if (typeof vSplitterPanel_OnViewVisibilityChange === 'function') vSplitterPanel_OnViewVisibilityChange('{0}');";

        /// <summary>Script to split a panel vertically.</summary>
        protected string Script_Splitter = "if (typeof vSplitterPanel === 'function') vSplitterPanel('{0}');";

        /// <summary>Mode property that stores display mode that this view was shown with.</summary>
        public string Mode
        {
            get { return ViewState["Mode"] as string; }
            set
            {
                if (value == null) ViewState.Remove("Mode");
                else ViewState["Mode"] = value;
            }
        }

        /// <summary>Mode to open views in a popup dialog.</summary>
        public const string ModePopup = "popup";

        /// <summary>Mode to open views inline as master-details.</summary>
        public const string ModeInline = "inline";

        /// <summary>Shortcut to get the main view panel</summary>
        protected virtual Control ViewPanel { get { return Controls[0]; } }

        /// <summary>Shows the view in the specified mode.</summary>
        public virtual void Show()
        {
            Visible = true;
            UpdatePanel upl = WebUtil.FindParentUpdatePanel(this);
            if (upl != null) upl.Update();

            switch (Mode)
            {
                case ModePopup:
                    RegisterStartupScript("Dialog", Script_ModalDialog, "show", ViewPanel.ClientID, upl != null ? upl.ClientID : "", "false");
                    break;
                case ModeInline:
                    RegisterStartupScript("Visible", Script_Splitter_OnViewVisibilityChange, upl.ClientID);
                    break;
            }
        }

        /// <summary>Hides the view.</summary>
        public virtual void Hide()
        {
            Visible = false;
            UpdatePanel upl = WebUtil.FindParentUpdatePanel(this), uplHost = null;
            if (upl != null)
            {
                upl.Update();
                uplHost = WebUtil.FindParentUpdatePanel(upl);
                if (uplHost != null) uplHost.Update();
            }

            switch (Mode)
            {
                case ModePopup:
                    RegisterStartupScript("Dialog", Script_ModalDialog, "hide", ViewPanel.ClientID, upl != null ? upl.ClientID : "", uplHost != null ? "true" : "false");
                    break;
                case ModeInline:
                    RegisterStartupScript("Visible", Script_Splitter_OnViewVisibilityChange, upl.ClientID);
                    break;
            }

            Mode = null;
            if (Closed != null) Closed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Default handler for closing the view.
        /// </summary>
        protected virtual void Close(object sender, EventArgs e)
        {
            Hide();
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
        /// Main update panel for the view
        /// </summary>
        protected UpdatePanel upl_Main;

        /// <summary>
        /// Subscribes to child view's events
        /// </summary>
        /// <param name="child">Child view</param>
        protected virtual void SubscribeToChildEvents(BaseView child)
        {
            child.Closed += OnChildClosed;
            BaseDetailsView detailsView = child as BaseDetailsView;
            if (detailsView != null)
            {
                detailsView.Saved += OnChildSaved;
                detailsView.Deleted += OnChildDeleted;
            }
            BaseSearchView searchView = child as BaseSearchView;
            if (searchView != null)
            {
                searchView.Selected += OnChildSelection;
            }
        }

        /// <summary>
        /// Default handler for processing selection of a child search view.
        /// </summary>
        /// <param name="searchView">Search view where selection took place</param>
        /// <param name="selectedRows">Selected rows</param>
        protected virtual void OnChildSelection(object searchView, List<DataRow> selectedRows) { }

        /// <summary>
        /// Default handler for saving or deleting of a child details view.
        /// </summary>
        /// <param name="detailsView">View being saved or deleted</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnChildChanged(object detailsView, EventArgs e) { }

        /// <summary>
        /// Default handler for saving of a child details view.
        /// </summary>
        /// <param name="detailsView">View being saved</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnChildSaved(object detailsView, EventArgs e)
        {
            OnChildChanged(detailsView, e);
            if (upl_Main != null) upl_Main.Update();
        }

        /// <summary>
        /// Default handler for deleting of a child details view.
        /// </summary>
        /// <param name="detailsView">View being deleted</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnChildDeleted(object detailsView, EventArgs e)
        {
            OnChildChanged(detailsView, e);
            if (upl_Main != null) upl_Main.Update();
        }

        /// <summary>
        /// Default handler for closing of a child view.
        /// </summary>
        /// <param name="childView">View being closed</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnChildClosed(object childView, EventArgs e)
        {
            if (upl_Main != null) upl_Main.Update();
        }

        #endregion
    }
}
