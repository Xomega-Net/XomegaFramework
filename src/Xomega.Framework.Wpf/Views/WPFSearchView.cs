﻿// Copyright (c) 2025 Xomega.Net. All rights reserved.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base class for WPF search views that may contain a results grid and a criteria panel
    /// </summary>
    public class WPFSearchView : WPFView
    {
        /// <summary>Criteria panel bound to the criteria object</summary>
        protected virtual FrameworkElement CriteriaPanel { get; }

        /// <summary>Collapsible criteria panel</summary>
        protected virtual ICollapsiblePanel CriteriaExpander { get; }

        /// <summary>Button to run the search</summary>
        protected virtual Button SearchButton { get; }

        /// <summary>Button to reset the view</summary>
        protected virtual Button ResetButton { get; }

        /// <summary>A panel that shows applied criteria</summary>
        protected virtual IAppliedCriteriaPanel AppliedCriteriaPanel { get; }

        /// <summary>Grid with search results</summary>
        protected virtual ItemsControl ResultsGrid { get; }

        /// <summary>Button to reset the view</summary>
        protected virtual Button SelectButton { get; }

        /// <summary>
        /// Configures button commands after initialization
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (SearchButton != null)
                SearchButton.Click += Search;
            if (ResetButton != null)
                ResetButton.Click += Reset;
            if (SelectButton != null)
                SelectButton.Click += Select;
        }

        /// <summary>
        /// Binds the view to its model, or unbinds the current model if null is passed.
        /// </summary>
        public override void BindTo(ViewModel viewModel)
        {
            bool bind = viewModel != null;
            if ((viewModel ?? Model) is SearchViewModel svm)
            {
                if (SelectButton != null && bind && viewModel.Params != null)
                    SelectButton.Visibility = (viewModel.Params[ViewParams.SelectionMode.Param] == null) ? Visibility.Hidden : Visibility.Visible;

                // subscribe to property change events on the data list object
                if (svm.List != null)
                {
                    if (bind) svm.List.PropertyChanged += OnListPropertyChanged;
                    else svm.List.PropertyChanged -= OnListPropertyChanged;
                }
                OnModelPropertyChanged(bind ? svm : null, new PropertyChangedEventArgs(SearchViewModel.CriteriaCollapsedProperty));
                OnListPropertyChanged(bind ? svm.List : null, new PropertyChangedEventArgs(nameof(DataListObject.AppliedCriteria)));

                BindDataObject(CriteriaPanel, bind && svm.List != null ? svm.List.CriteriaObject : null);
                BindDataObject(ResultsGrid, bind ? svm.List : null);

                // recalculate applied criteria with updated labels after binding
                if (svm.List != null && svm.List.CriteriaObject != null && svm.List.AppliedCriteria != null)
                    svm.List.AppliedCriteria = svm.List.CriteriaObject.GetCriteriaDisplays(false);
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
            if (CriteriaExpander != null && SearchViewModel.CriteriaCollapsedProperty.Equals(e.PropertyName))
            {
                CriteriaExpander.Collapsed = sender is SearchViewModel svm ? svm.CriteriaCollapsed : false;
            }
        }

        /// <summary>
        /// Handles AppliedCriteria property change on the list object to update the content of AppliedCriteria panel
        /// </summary>
        /// <param name="sender">Data list object that sent the event</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (AppliedCriteriaPanel != null && nameof(DataListObject.AppliedCriteria).Equals(e.PropertyName))
            {
                AppliedCriteriaPanel.BindTo(sender is DataListObject list ? list.AppliedCriteria : null);
            }
        }

        #region Event handlers

        /// <summary>
        /// Default handler for searching that delegates the action to the view model.
        /// </summary>
        protected virtual async void Search(object sender, EventArgs e)
        {
            if (Model is SearchViewModel svm)
            {
                // store enabled state of the search button to restore at the end
                // in case when a subclass overrides search and manages the button's state
                bool enabled = SearchButton?.IsEnabled ?? false;
                try
                {
                    if (SearchButton != null)
                        SearchButton.IsEnabled = false;
                    if (IsAsync) await svm.SearchAsync(false);
                    else svm.Search(sender, e);
                }
                finally
                {
                    if (SearchButton != null)
                        SearchButton.IsEnabled = enabled;
                }
            }
        }

        /// <summary>
        /// Default handler for resetting that delegates the action to the view model.
        /// </summary>
        protected virtual void Reset(object sender, EventArgs e)
        {
            if (Model is SearchViewModel svm) svm.Reset(sender, e);
        }

        /// <summary>
        /// Default handler for selecting that delegates the action to the view model.
        /// </summary>
        protected virtual async void Select(object sender, EventArgs e)
        {
            if (Model is SearchViewModel svm)
            {
                if (IsAsync) await svm.SelectAsync();
                else svm.Select(sender, e);
            }
        }

        #endregion
    }
}
