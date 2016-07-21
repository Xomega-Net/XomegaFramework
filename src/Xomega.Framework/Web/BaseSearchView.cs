// Copyright (c) 2016 Xomega.Net. All rights reserved.

using System;
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// Base class for search views with criteria and results list
    /// </summary>
    public abstract class BaseSearchView : BaseView
    {
        #region Data objects

        /// <summary>
        /// Criteria data object for the view
        /// </summary>
        protected DataObject criteria;

        /// <summary>
        /// List data object for the view
        /// </summary>
        protected DataListObject list;

        /// <summary>
        /// Retrieves the criteria object possibly creating a new one along the way.
        /// </summary>
        /// <typeparam name="T">Criteria object type</typeparam>
        /// <param name="createNew">Whether or not to create a new criteria object</param>
        /// <returns>The criteria object for the view</returns>
        protected virtual T GetCriteria<T>(bool createNew) where T : class, new()
        {
            return WebUtil.GetCachedObject<T>("criteria:" + UniqueID + Request.CurrentExecutionFilePath, createNew);
        }

        /// <summary>
        /// Retrieves the list object possibly creating a new one along the way.
        /// </summary>
        /// <typeparam name="T">List object type</typeparam>
        /// <param name="createNew">Whether or not to create a new list object</param>
        /// <returns>The list object for the view</returns>
        protected virtual T GetList<T>(bool createNew) where T : class, new()
        {
            return WebUtil.GetCachedObject<T>("list:" + UniqueID + Request.CurrentExecutionFilePath, createNew);
        }

        #endregion

        #region Binding

        /// <summary>
        /// Panel with criteria
        /// </summary>
        protected CollapsiblePanelBase ucl_Criteria;

        /// <summary>
        /// Results grid
        /// </summary>
        protected GridView grd_Results;

        /// <summary>
        /// Binds criteria panel and results grid to the criteria and list objects respectively
        /// </summary>
        protected override void BindObjects()
        {
            if (criteria != null && ucl_Criteria != null)
            {
                ucl_Criteria.DataBind();
                WebUtil.BindToObject(ucl_Criteria, criteria);
            }
            if (list != null && grd_Results != null)
                WebUtil.BindToList(grd_Results, list);
        }

        #endregion

        #region Initialization/Activation

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
        /// Activates search view and runs the search if needed
        /// </summary>
        /// <param name="query">Parameters to activate the view with</param>
        public override void Activate(NameValueCollection query)
        {
            InitObjects(true);

            if (criteria != null) criteria.SetValues(query);
            if (list != null) list.RowSelectionMode = query[QuerySelectionMode];
            if (query[QueryAction] == ActionSearch)
            {
                Search();
            }

            BindObjects();
        }

        /// <summary>
        /// Resets current view to initial state
        /// </summary>
        /// <param name="sender">Command sender</param>
        /// <param name="e">Event arguments</param>
        protected virtual void Reset(object sender, EventArgs e)
        {
            if (criteria != null) criteria.ResetData();
            if (list != null) list.ResetData();
            if (AutomateCriteriaPanel) ucl_Criteria.Collapsed = false;
        }

        #endregion

        #region Search

        /// <summary>
        /// Action to initiate search on activation
        /// </summary>
        public const string ActionSearch = "Search";

        /// <summary>
        /// Controls if criteria panel will automatically collapse/expand on Search/Reset.
        /// </summary>
        protected virtual bool AutomateCriteriaPanel { get { return true; } }

        /// <summary>
        /// Perfroms the search with the current criteria and populates the list
        /// </summary>
        /// <returns>True on success, false in case of errors.</returns>
        public abstract bool Search();

        /// <summary>
        /// Search function exposed as an event handler for the Search button
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event argument</param>
        protected virtual void Search(object sender, EventArgs e)
        {
            if (Search()) {
                if (AutomateCriteriaPanel) ucl_Criteria.Collapsed = true;
            }
        }

        /// <summary>
        /// Default handler for saving or deleting of a child details view.
        /// </summary>
        /// <param name="obj">View being saved or deleted</param>
        /// <param name="e">Event arguments</param>
        protected override void OnChildChanged(object obj, EventArgs e)
        {
            Search();
        }

        #endregion

        #region Selection

        /// <summary>
        /// Query parameter indicating selection mode to set, if any
        /// </summary>
        public const string QuerySelectionMode = "SelectionMode";

        /// <summary>
        /// Default handler for closing of a child view.
        /// </summary>
        /// <param name="obj">View being closed</param>
        /// <param name="e">Event arguments</param>
        protected override void OnChildClosed(object obj, EventArgs e)
        {
            if (list != null)
            {
                list.ClearSelectedRows();
                list.FireCollectionChanged();
            }
            base.OnChildClosed(obj, e);
        }

        #endregion
    }
}
