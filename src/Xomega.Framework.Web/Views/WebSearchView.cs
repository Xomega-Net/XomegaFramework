// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Xomega.Framework.Views;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// Base class for search views with criteria and results list
    /// </summary>
    public abstract class WebSearchView : WebView
    {
        #region Data objects

        private string criteriaKey { get { return "criteria:" + UniqueID + Request.CurrentExecutionFilePath; } }

        private CriteriaObject criteriaObj
        {
            get { return Session[criteriaKey] as CriteriaObject; }
            set
            {
                if (value == null) Session.Remove(criteriaKey);
                Session[criteriaKey] = value;
            }
        }

        private string listKey { get { return "criteria:" + UniqueID + Request.CurrentExecutionFilePath; } }

        private DataListObject listObj
        {
            get { return Session[listKey] as DataListObject; }
            set
            {
                if (value == null) Session.Remove(listKey);
                Session[listKey] = value;
            }
        }

        #endregion

        #region Controls

        /// <summary>
        /// Panel with criteria
        /// </summary>
        protected Control ucl_Criteria;

        /// <summary>
        /// Panel with applied criteria
        /// </summary>
        protected IAppliedCriteriaPanel ucl_AppliedCriteria;

        /// <summary>
        /// Results grid
        /// </summary>
        protected GridView grd_Results;

        /// <summary>
        /// Search button
        /// </summary>
        protected Button btn_Search;

        /// <summary>
        /// Refresh button - same as search, but w/o criteria
        /// </summary>
        protected Button btn_Refresh;

        /// <summary>
        /// Reset button
        /// </summary>
        protected Button btn_Reset;

        /// <summary>
        /// Select button
        /// </summary>
        protected Button btn_Select;

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the already constructed view model for each request
        /// </summary>
        /// <param name="e">Standard event arguments</param>
        protected override void OnLoad(EventArgs e)
        {
            SearchViewModel svm = Model as SearchViewModel;
            if (svm != null)
            {
                CriteriaObject crit = criteriaObj;
                if (crit != null) svm.Criteria = crit;
                DataListObject list = listObj;
                if (list != null) svm.List = list;
                ICollapsiblePanel collapsible = ucl_Criteria as ICollapsiblePanel;
                if (collapsible != null)
                    svm.CriteriaCollapsed = collapsible.Collapsed;
            }
            base.OnLoad(e);
        }

        #endregion

        #region Binding

        /// <summary>
        /// Binds the view to its model, or unbinds the current model if null is passed.
        /// </summary>
        /// <param name="viewModel">Model to bind the view to</param>
        public override void BindTo(ViewModel viewModel)
        {
            bool bind = viewModel != null;
            SearchViewModel svm = (bind ? viewModel : this.Model) as SearchViewModel;
            if (svm != null)
            {
                if (btn_Search != null)
                {
                    if (bind) btn_Search.Click += svm.Search;
                    else btn_Search.Click -= svm.Search;
                }
                if (btn_Refresh != null)
                {
                    if (bind) btn_Refresh.Click += svm.Search;
                    else btn_Refresh.Click -= svm.Search;
                }
                if (btn_Reset != null)
                {
                    if (bind) btn_Reset.Click += svm.Reset;
                    else btn_Reset.Click -= svm.Reset;
                }
                if (btn_Select != null)
                {
                    if (bind) btn_Select.Click += svm.Select;
                    else btn_Select.Click -= svm.Select;

                    if (bind && viewModel.Params != null)
                        btn_Select.Visible = (viewModel.Params[ViewParams.SelectionMode.Param] != null);
                }

                // subscribe to property change events on the data list object
                if (bind)
                {
                    if (svm.List != null)
                        svm.List.PropertyChanged += OnListPropertyChanged;
                    // persist the objects in session
                    criteriaObj = svm.Criteria;
                    listObj = svm.List;
                }
                else
                {
                    if (svm.List != null)
                        svm.List.PropertyChanged += OnListPropertyChanged;
                }
                OnModelPropertyChanged(svm, new PropertyChangedEventArgs(SearchViewModel.CriteriaCollapsedProperty));
                OnListPropertyChanged(bind ? svm.List : null, new PropertyChangedEventArgs(DataListObject.AppliedCriteriaProperty));

                if (svm.Criteria != null && ucl_Criteria != null)
                {
                    ucl_Criteria.DataBind();
                    WebPropertyBinding.BindToObject(ucl_Criteria, bind ? svm.Criteria : null);
                }
                if (svm.List != null && grd_Results != null)
                {
                    grd_Results.AutoGenerateSelectButton = ViewParams.SelectionMode.Single.Equals(svm.Params[ViewParams.SelectionMode.Param]);
                    WebPropertyBinding.BindToList(grd_Results, bind ? svm.List : null);
                }
            }
            base.BindTo(viewModel);
        }

        /// <summary>
        /// Handles CriteriaCollapsed property change to update the state of Criteria panel
        /// </summary>
        /// <param name="sender">Model that sent the event</param>
        /// <param name="e">Event arguments</param>
        protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnModelPropertyChanged(sender, e);
            ICollapsiblePanel collapsible = ucl_Criteria as ICollapsiblePanel;
            if (collapsible != null && SearchViewModel.CriteriaCollapsedProperty.Equals(e.PropertyName))
            {
                SearchViewModel svm = sender as SearchViewModel;
                collapsible.Collapsed = svm != null ? svm.CriteriaCollapsed : false;
            }
        }

        /// <summary>
        /// Handles AppliedCriteria property change on the list object to update the content of AppliedCriteria panel
        /// </summary>
        /// <param name="sender">Data list object that sent the event</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ucl_AppliedCriteria != null && DataListObject.AppliedCriteriaProperty.Equals(e.PropertyName))
            {
                DataListObject list = sender as DataListObject;
                ucl_AppliedCriteria.BindTo((list != null) ? list.AppliedCriteria : null);
            }
        }

        #endregion
    }
}
