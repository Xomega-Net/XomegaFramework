// Copyright (c) 2016 Xomega.Net. All rights reserved.

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
        /// <summary>Collapsible criteria panel</summary>
        protected virtual FrameworkElement CriteriaPanel { get; }

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
        /// Binds the view to its model, or unbinds the current model if null is passed.
        /// </summary>
        public override void BindTo(ViewModel viewModel)
        {
            bool bind = viewModel != null;
            SearchViewModel svm = (bind ? viewModel : this.Model) as SearchViewModel;
            if (svm != null)
            {
                if (SearchButton != null)
                {
                    if (bind) SearchButton.Click += svm.Search;
                    else SearchButton.Click -= svm.Search;
                }
                if (ResetButton != null)
                {
                    if (bind) ResetButton.Click += svm.Reset;
                    else ResetButton.Click -= svm.Reset;
                }
                if (SelectButton != null)
                {
                    if (bind) SelectButton.Click += svm.Select;
                    else SelectButton.Click -= svm.Select;

                    if (bind && viewModel.Params != null)
                        SelectButton.Visibility = (viewModel.Params[ViewParams.SelectionMode.Param] == null) ? Visibility.Hidden : Visibility.Visible;
                }

                // subscribe to property change events on the data list object
                if (bind)
                {
                    if (svm.List != null)
                        svm.List.PropertyChanged += OnListPropertyChanged;
                }
                else
                {
                    if (svm.List != null)
                        svm.List.PropertyChanged += OnListPropertyChanged;
                }
                OnModelPropertyChanged(bind ? svm : null, new PropertyChangedEventArgs(SearchViewModel.CriteriaCollapsedProperty));
                OnListPropertyChanged(bind ? svm.List : null, new PropertyChangedEventArgs(DataListObject.AppliedCriteriaProperty));

                BindDataObject(CriteriaPanel, bind ? svm.Criteria : null);
                BindDataObject(ResultsGrid, bind ? svm.List : null);
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
                SearchViewModel svm = sender as SearchViewModel;
                CriteriaExpander.Collapsed = svm != null ? svm.CriteriaCollapsed : false;
            }
        }

        /// <summary>
        /// Handles AppliedCriteria property change on the list object to update the content of AppliedCriteria panel
        /// </summary>
        /// <param name="sender">Data list object that sent the event</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (AppliedCriteriaPanel != null && DataListObject.AppliedCriteriaProperty.Equals(e.PropertyName))
            {
                DataListObject list = sender as DataListObject;
                AppliedCriteriaPanel.BindTo(list != null ? list.AppliedCriteria : null);
            }
        }
    }
}
