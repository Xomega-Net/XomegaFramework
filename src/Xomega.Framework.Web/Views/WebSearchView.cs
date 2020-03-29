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
    /// Base class for search views with criteria and results list
    /// </summary>
    public abstract class WebSearchView : WebView
    {
        #region Data objects

        private string ListKey { get { return GetSessionKey("listObj"); } }

        private DataListObject ListObj
        {
            get { return Session[ListKey] as DataListObject; }
            set
            {
                if (value == null) Session.Remove(ListKey);
                Session[ListKey] = value;
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
        /// Initializes the already constructed view model for each request.
        /// </summary>
        /// <param name="e">Standard event arguments</param>
        protected override void OnLoad(EventArgs e)
        {
            // restore view model state
            if (Model is SearchViewModel svm)
            {
                DataListObject list = ListObj;
                if (list != null) svm.List = list;
                if (ucl_Criteria is ICollapsiblePanel collapsible)
                    svm.CriteriaCollapsed = collapsible.Collapsed;
            }

            // wire up event handlers for buttons
            if (btn_Search != null)
                btn_Search.Click += Search;
            if (btn_Refresh != null)
                btn_Refresh.Click += Refresh;
            if (btn_Reset != null)
                btn_Reset.Click += Reset;
            if (btn_Select != null)
                btn_Select.Click += Select;

            base.OnLoad(e);
        }

        #endregion

        #region Binding

        /// <inheritdoc/>
        public override void BindTo(ViewModel viewModel)
        {
            bool bind = viewModel != null;
            if ((viewModel ?? Model) is SearchViewModel svm)
            {
                if (btn_Select != null && bind && svm.Params != null)
                    btn_Select.Visible = (svm.Params[ViewParams.SelectionMode.Param] != null);

                // subscribe to property change events on the data list object
                if (bind)
                {
                    if (svm.List != null)
                        svm.List.PropertyChanged += OnListPropertyChanged;
                    // persist the object in session
                    ListObj = svm.List;
                }
                else
                {
                    if (svm.List != null)
                        svm.List.PropertyChanged -= OnListPropertyChanged;
                }
                OnModelPropertyChanged(svm, new PropertyChangedEventArgs(SearchViewModel.CriteriaCollapsedProperty));
                OnListPropertyChanged(bind ? svm.List : null, new PropertyChangedEventArgs(DataListObject.AppliedCriteriaProperty));

                if (svm.List != null && grd_Results != null)
                {
                    if (svm.List.CriteriaObject != null && ucl_Criteria != null)
                    {
                        ucl_Criteria.DataBind();
                        WebPropertyBinding.BindToObject(ucl_Criteria, bind ? svm.List.CriteriaObject : null);
                        if (svm.List.AppliedCriteria != null) // recalculate applied criteria with updated labels after binding
                            svm.List.AppliedCriteria = svm.List.CriteriaObject.GetFieldCriteriaSettings();
                    }
                    if (grd_Results != null)
                    {
                        grd_Results.AutoGenerateSelectButton = ViewParams.SelectionMode.Single.Equals(svm.Params[ViewParams.SelectionMode.Param]);
                        WebPropertyBinding.BindToList(grd_Results, bind ? svm.List : null);
                    }
                }
            }
            base.BindTo(viewModel);
        }

        /// <summary>
        /// Handles CriteriaCollapsed property change to update the state of Criteria panel.
        /// </summary>
        /// <param name="sender">Model that sent the event</param>
        /// <param name="e">Event arguments</param>
        protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnModelPropertyChanged(sender, e);
            if (ucl_Criteria is ICollapsiblePanel collapsible && SearchViewModel.CriteriaCollapsedProperty.Equals(e.PropertyName))
            {
                if (sender is SearchViewModel svm)
                    collapsible.Collapsed = svm.CriteriaCollapsed;
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
                ucl_AppliedCriteria.BindTo((sender is DataListObject list) ? list.AppliedCriteria : null);
            }
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// Default handler for searching that delegates the action to the view model.
        /// </summary>
        protected virtual void Search(object sender, EventArgs e)
        {
            if (Page.IsAsync)
                Page.RegisterAsyncTask(new PageAsyncTask(async (ct) => await SearchAsync(ct)));
            else if (Model is SearchViewModel svm)
                svm.Search(sender, e);
        }

        /// <summary>
        /// Default handler for searching that delegates the action to the view model.
        /// </summary>
        protected virtual async Task SearchAsync(CancellationToken token = default)
        {
            if (Model is SearchViewModel svm)
                await svm.SearchAsync(token);
        }

        /// <summary>
        /// Default handler for refreshing that delegates the action to the view model.
        /// </summary>
        protected virtual void Refresh(object sender, EventArgs e) => Search(sender, e);

        /// <summary>
        /// Default handler for resetting that delegates the action to the view model.
        /// </summary>
        protected virtual void Reset(object sender, EventArgs e)
        {
            if (Model is SearchViewModel svm)
                svm.Reset(sender, e);
        }

        /// <summary>
        /// Default handler for selectinging that delegates the action to the view model.
        /// </summary>
        protected virtual void Select(object sender, EventArgs e)
        {
            if (Page.IsAsync)
                Page.RegisterAsyncTask(new PageAsyncTask(async (ct) => await SelectAsync(ct)));
            else if (Model is SearchViewModel svm)
                svm.Select(sender, e);
        }

        /// <summary>
        /// Default handler for selectinging that delegates the action to the view model.
        /// </summary>
        protected virtual async Task SelectAsync(CancellationToken token = default)
        {
            if (Model is SearchViewModel svm)
                await svm.SelectAsync(token);
        }

        #endregion
    }
}
