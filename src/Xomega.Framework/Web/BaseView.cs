﻿// Copyright (c) 2016 Xomega.Net. All rights reserved.

using System;
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
        public abstract void Activate(NameValueCollection query);

        /// <summary>
        /// Query parameter indicating action to perform
        /// </summary>
        public const string QueryAction = "Action";

        /// <summary>
        /// Action to create a new object
        /// </summary>
        public const string ActionCreate = "Create";

        /// <summary>
        /// Navigate from the current view to the specified view.
        /// </summary>
        /// <param name="view">View to navigate to</param>
        /// <param name="query">Query parameters to pass</param>
        /// <param name="mode">Navigation mode (BaseView.ModePopup or BaseView.ModeInline)</param>
        public void NavigateTo(BaseView view, NameValueCollection query, string mode = null)
        {
            view.Activate(query);
            view.Show(mode);
        }

        #endregion

        #region Show/Hide with JavaScript

        /// <summary>Script to run code on document ready.</summary>
        protected string Script_OnDocumentReady = "$(document).ready(function() {{ {0} }});";

        /// <summary>Script to popup a modal dialog for a view.</summary>
        protected string Script_ModalDialog = "modalViewPopup('{0}', '{1}', '{2}', {3});";
        
        /// <summary>Script to udpate view visibility in a split panel.</summary>
        protected string Script_Splitter_OnViewVisibilityChange = "vSplitterPanel_OnViewVisibilityChange('{0}');";
        
        /// <summary>Script to split a panel vertically.</summary>
        protected string Script_Splitter = "vSplitterPanel('{0}');";

        /// <summary>Mode property that stores display mode that this view was shown with.</summary>
        protected string Mode
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
        /// <param name="mode">Navigation mode (BaseView.ModePopup or BaseView.ModeInline)</param>
        public virtual void Show(string mode = null)
        {
            Visible = true;
            UpdatePanel upl = WebUtil.FindParentUpdatePanel(this);
            if (upl != null) upl.Update();

            if (ModePopup.Equals(mode))
            {
                RegisterStartupScript("Dialog", Script_ModalDialog, "show", ViewPanel.ClientID, upl != null ? upl.ClientID : "", "false");
            }
            else if (ModeInline.Equals(mode))
            {
                RegisterStartupScript("Visible", Script_Splitter_OnViewVisibilityChange, upl.ClientID);
            }

            Mode = mode ?? "";
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

            string mode = Mode;
            if (mode == ModePopup)
            {
                RegisterStartupScript("Dialog", Script_ModalDialog, "hide", ViewPanel.ClientID, upl != null ? upl.ClientID : "", uplHost != null ? "true" : "false");
            }
            else if (mode == ModeInline)
            {
                RegisterStartupScript("Visible", Script_Splitter_OnViewVisibilityChange, upl.ClientID);
            }

            Mode = null;
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
        /// <param name="key">Unique key within the view to register the script under.</param>
        /// <param name="script">JavaScript text with placeholders.</param>
        /// <param name="args">Arguments for the placeholders.</param>
        protected void RegisterStartupScript(string key, string script, params object[] args)
        {
            if (!script.EndsWith(";")) script += ";";
            string formattedScript = args.Length == 0 ? script : string.Format(script, args);

            UpdatePanel upl = WebUtil.FindParentUpdatePanel(this);
            if (upl != null)
            {
                Control pnlView = ViewPanel;
                ScriptManager.RegisterStartupScript(
                    pnlView,
                    pnlView.GetType(),
                    pnlView.ClientID + "_" + key,
                    formattedScript,
                    true);
            }
            else
            {
                Page.ClientScript.RegisterStartupScript(
                    Page.GetType(),
                    Page.ClientID + "_" + key,
                    string.Format(Script_OnDocumentReady, formattedScript),
                    true);
            }
        }

        #endregion

        #region Handling child view updates

        /// <summary>
        /// Main update panel for the view
        /// </summary>
        protected UpdatePanel upl_Main;

        /// <summary>
        /// Default handler for saving or deleting of a child details view.
        /// </summary>
        /// <param name="obj">View being saved or deleted</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnChildChanged(object obj, EventArgs e) { }

        /// <summary>
        /// Default handler for saving of a child details view.
        /// </summary>
        /// <param name="obj">View being saved</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnChildSaved(object obj, EventArgs e)
        {
            OnChildChanged(obj, e);
            if (upl_Main != null) upl_Main.Update();
        }

        /// <summary>
        /// Default handler for deleting of a child details view.
        /// </summary>
        /// <param name="obj">View being deleted</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnChildDeleted(object obj, EventArgs e)
        {
            OnChildChanged(obj, e);
            if (upl_Main != null) upl_Main.Update();
        }

        #endregion
    }
}