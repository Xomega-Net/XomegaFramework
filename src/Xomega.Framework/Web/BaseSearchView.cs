// Copyright (c) 2016 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
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
        protected CriteriaObject criteriaObj;

        /// <summary>
        /// List data object for the view
        /// </summary>
        protected DataListObject listObj;

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

        #region Controls

        /// <summary>
        /// Panel with criteria
        /// </summary>
        protected BaseCollapsiblePanel ucl_Criteria;

        /// <summary>
        /// Results grid
        /// </summary>
        protected GridView grd_Results;

        /// <summary>
        /// Select button
        /// </summary>
        protected Control btn_Select;

        #endregion

        #region Binding

        /// <summary>
        /// Binds criteria panel and results grid to the criteria and list objects respectively
        /// </summary>
        protected override void BindObjects()
        {
            if (criteriaObj != null && ucl_Criteria != null)
            {
                ucl_Criteria.DataBind();
                WebUtil.BindToObject(ucl_Criteria, criteriaObj);
            }
            if (listObj != null && grd_Results != null)
                WebUtil.BindToList(grd_Results, listObj);
        }

        #endregion

        #region Initialization/Activation

        /// <summary>
        /// Activates search view and runs the search if needed
        /// </summary>
        /// <param name="query">Parameters to activate the view with</param>
        /// <returns>True if the view was successfully activated, False otherwise</returns>
        public override bool Activate(NameValueCollection query)
        {
            if (!base.Activate(query)) return false;
            InitObjects(true);

            if (criteriaObj != null) criteriaObj.SetValues(query);
            if (listObj != null) listObj.RowSelectionMode = query[QuerySelectionMode];

            BindObjects(); // bind before search to output criteria labels properly
            if (query[QueryAction] == ActionSearch) Search(false);
            // try to auto-select as appropriate and don't show the view if succeeded
            if (query[QueryAction] == ActionSelect && AutoSelect()) return false;
            // make Select button visible only in Select mode
            if (btn_Select != null) btn_Select.Visible = (query[QueryAction] == ActionSelect);
            return true;
        }

        /// <summary>
        /// Resets current view to initial state
        /// </summary>
        /// <param name="sender">Command sender</param>
        /// <param name="e">Event arguments</param>
        protected virtual void Reset(object sender, EventArgs e)
        {
            if (criteriaObj != null) criteriaObj.ResetData();
            if (listObj != null) listObj.ResetData();
            if (AutoCollapseCriteria) ucl_Criteria.Collapsed = false;
        }

        #endregion

        #region Search

        /// <summary>
        /// Action to initiate search on activation
        /// </summary>
        public const string ActionSearch = "search";

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
        protected virtual void Search(object sender, EventArgs e)
        {
            if (Search(true) && ucl_Criteria != null && AutoCollapseCriteria)
                ucl_Criteria.Collapsed = true;
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
        /// Action to initiate search on activation
        /// </summary>
        public const string ActionSelect = "select";

        /// <summary>
        /// Query parameter indicating selection mode to set, if any
        /// </summary>
        public const string QuerySelectionMode = "_selection";

        /// <summary>
        /// Automates row selection process
        /// </summary>
        /// <returns>True if automatic selection succeeded, false otherwise.</returns>
        public virtual bool AutoSelect()
        {
            if (listObj == null || criteriaObj != null && !criteriaObj.HasCriteria() || !Search(false)) return false;
            if (listObj.RowCount > 1 && ucl_Criteria != null && AutoCollapseCriteria)
                ucl_Criteria.Collapsed = true;
            else if (listObj.RowCount == 0 && ucl_Criteria != null)
                ucl_Criteria.Collapsed = false;
            else if (listObj.RowCount == 1 && Selected != null)
            {
                listObj.SelectRow(0);
                Selected(this, listObj.SelectedRows);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Default handler for convirming selected records and closing the view.
        /// </summary>
        protected virtual void Select(object sender, EventArgs e)
        {
            if (Selected != null && listObj != null)
            {
                Selected(this, listObj.SelectedRows);
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
            if (listObj != null)
            {
                listObj.ClearSelectedRows();
                listObj.FireCollectionChanged();
            }
            base.OnChildClosed(obj, e);
        }

        #endregion
    }
}
