// Copyright (c) 2016 Xomega.Net. All rights reserved.

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
        /// <summary>Collapsible criteria panel</summary>
        protected virtual Expander CriteriaPanel { get; }

        /// <summary>Button to run the search</summary>
        protected virtual Button SearchButton { get; }

        /// <summary>Button to reset the view</summary>
        protected virtual Button ResetButton { get; }

        /// <summary>A panel that shows applied criteria</summary>
        protected virtual FrameworkElement AppliedCriteriaPanel { get; }

        /// <summary>Grid with search results</summary>
        protected virtual ItemsControl ResultsGrid { get; }

        /// <summary>Button to reset the view</summary>
        protected virtual Button SelectButton { get; }

        /// <summary>Binds the view to its controller</summary>
        public override void BindTo(ViewController controller)
        {
            bool bind = controller != null;
            SearchViewController svController = (bind ? controller : this.Controller) as SearchViewController;
            if (svController != null)
            {
                if (SearchButton != null)
                {
                    if (bind) SearchButton.Click += svController.Search;
                    else SearchButton.Click -= svController.Search;
                }
                if (ResetButton != null)
                {
                    if (bind) ResetButton.Click += svController.Reset;
                    else ResetButton.Click -= svController.Reset;
                }
                if (SelectButton != null)
                {
                    if (bind) SelectButton.Click += svController.Select;
                    else SelectButton.Click -= svController.Select;

                    if (bind && controller.Params != null)
                        SelectButton.Visibility = (controller.Params[ViewParams.SelectionMode.Param] == null) ? Visibility.Hidden : Visibility.Visible;
                }

                // subscribe to property change events on the controller and the data list object
                if (bind)
                {
                    svController.PropertyChanged += OnControllerPropertyChanged;
                    if (svController.List != null)
                        svController.List.PropertyChanged += OnListPropertyChanged;
                }
                else
                {
                    svController.PropertyChanged -= OnControllerPropertyChanged;
                    if (svController.List != null)
                        svController.List.PropertyChanged += OnListPropertyChanged;
                }
                OnControllerPropertyChanged(svController, new PropertyChangedEventArgs(SearchViewController.CriteriaCollapsedProperty));
                OnListPropertyChanged(bind ? svController.List : null, new PropertyChangedEventArgs(DataListObject.AppliedCriteriaProperty));

                BindDataObject(CriteriaPanel, bind ? svController.Criteria : null);
                BindDataObject(ResultsGrid, bind ? svController.List : null);
            }
            base.BindTo(controller);
        }

        /// <summary>
        /// Handles CriteriaCollapsed property change to update the state of Criteria panel
        /// </summary>
        /// <param name="sender">Controller that sent the event</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnControllerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (CriteriaPanel != null && SearchViewController.CriteriaCollapsedProperty.Equals(e.PropertyName))
            {
                SearchViewController svController = sender as SearchViewController;
                CriteriaPanel.IsExpanded = svController != null ? !svController.CriteriaCollapsed : true;
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
                AppliedCriteriaPanel.DataContext = (list != null) ? list.AppliedCriteria : null;
            }
        }
    }
}
