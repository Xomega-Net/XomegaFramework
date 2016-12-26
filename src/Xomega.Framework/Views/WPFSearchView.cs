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
                if (bind)
                {
                    OnControllerPropertyChanged(svController, new PropertyChangedEventArgs(SearchViewController.CriteriaCollapsedProperty));
                    svController.PropertyChanged += OnControllerPropertyChanged;
                }
                else svController.PropertyChanged -= OnControllerPropertyChanged;

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
            SearchViewController svController = sender as SearchViewController;
            if (CriteriaPanel != null && svController != null &&
                SearchViewController.CriteriaCollapsedProperty.Equals(e.PropertyName))
            {
                CriteriaPanel.IsExpanded = !svController.CriteriaCollapsed;
            }
        }
    }
}
