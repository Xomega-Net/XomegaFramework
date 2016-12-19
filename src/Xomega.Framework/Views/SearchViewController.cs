// Copyright (c) 2016 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base class for controllers of search views with a results grid and a criteria panel
    /// </summary>
    public abstract class SearchViewController : ViewController
    {
        #region Initialization/Activation

        /// <summary>
        /// Constructs a new controller for the given search view
        /// </summary>
        /// <param name="svcProvider">Service provider for the controller</param>
        /// <param name="view">Search view associated with the controller</param>
        public SearchViewController(IServiceProvider svcProvider, ISearchView view) : base(svcProvider, view)
        {

        }

        /// <summary>
        /// Activates the view controller and the view
        /// </summary>
        /// <param name="parameters">Parameters to activate the view with</param>
        /// <returns>True if the view was successfully activated, False otherwise</returns>
        public override bool Activate(NameValueCollection parameters)
        {
            if (!base.Activate(parameters)) return false;

            if (Criteria != null) Criteria.SetValues(parameters);
            if (List != null) List.RowSelectionMode = parameters[ViewParams.SelectionMode.Param];

            if (parameters[ViewParams.Action.Param] == ViewParams.Action.Search || Criteria == null)
                Search(false);

            // try to auto-select as appropriate and don't show the view if succeeded
            if (parameters[ViewParams.Action.Param] == ViewParams.Action.Select && AutoSelect())
                return false;

            return true;
        }

        #endregion

        #region Data objects

        /// <summary>
        /// Criteria data object for the view
        /// </summary>
        public CriteriaObject Criteria { get; set; }

        /// <summary>
        /// List data object for the view
        /// </summary>
        public DataListObject List { get; set; }

        #endregion

        #region Search

        /// <summary>
        /// Controls if criteria panel will automatically collapse/expand on Search/Reset.
        /// </summary>
        protected virtual bool AutoCollapseCriteria { get { return true; } }

        /// <summary>
        /// Perfroms the search with the current criteria and populates the list
        /// </summary>
        /// <param name="preserveSelection">A flag indicating whether or not to preserve selection.</param>
        /// <returns>True on success, false in case of errors.</returns>
        public abstract bool Search(bool preserveSelection);

        /// <summary>
        /// Search function exposed as an event handler for the Search button
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event argument</param>
        public virtual void Search(object sender, EventArgs e)
        {
            if (Search(true) && AutoCollapseCriteria)
                ((ISearchView)view).CriteriaCollapsed = true;
        }

        /// <summary>
        /// Resets current view to initial state
        /// </summary>
        /// <param name="sender">Command sender</param>
        /// <param name="e">Event arguments</param>
        public virtual void Reset(object sender, EventArgs e)
        {
            if (Criteria != null) Criteria.ResetData();
            if (List != null) List.ResetData();
            if (AutoCollapseCriteria)
                ((ISearchView)view).CriteriaCollapsed = false;
        }

        /// <summary>
        /// Default handler for saving or deleting of a child details view.
        /// </summary>
        /// <param name="obj">View being saved or deleted</param>
        /// <param name="e">Event arguments</param>
        protected override void OnChildChanged(object obj, EventArgs e)
        {
            Search(true);
        }

        #endregion

        #region Selection

        /// <summary>
        /// Occurs when the user confirms selection of currently selected rows
        /// </summary>
        public EventHandler<List<DataRow>> Selected;

        /// <summary>
        /// Automates row selection process
        /// </summary>
        /// <returns>True if automatic selection succeeded, false otherwise.</returns>
        public virtual bool AutoSelect()
        {
            if (List == null || Criteria != null && !Criteria.HasCriteria() || !Search(false)) return false;
            if (List.RowCount > 1 && AutoCollapseCriteria)
                ((ISearchView)view).CriteriaCollapsed = true;
            else if (List.RowCount == 0)
                ((ISearchView)view).CriteriaCollapsed = false;
            else if (List.RowCount == 1 && Selected != null)
            {
                List.SelectRow(0);
                Selected(this, List.SelectedRows);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Default handler for convirming selected records and closing the view.
        /// </summary>
        public virtual void Select(object sender, EventArgs e)
        {
            if (Selected != null && List != null)
            {
                Selected(this, List.SelectedRows);
                Hide();
            }
        }

        /// <summary>
        /// Default handler for closing of a child view.
        /// </summary>
        /// <param name="obj">View being closed</param>
        /// <param name="e">Event arguments</param>
        protected override void OnChildClosed(object obj, EventArgs e)
        {
            if (List != null)
            {
                List.ClearSelectedRows();
                List.FireCollectionChanged();
            }
            base.OnChildClosed(obj, e);
        }

        #endregion
    }
}