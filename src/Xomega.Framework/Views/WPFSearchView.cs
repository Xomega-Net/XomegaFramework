// Copyright (c) 2016 Xomega.Net. All rights reserved.

using System.Windows;
using System.Windows.Controls;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base class for WPF search views that may contain a results grid and a criteria panel
    /// </summary>
    public class WPFSearchView : WPFView, ISearchView
    {
        /// <summary>Collapsible criteria panel</summary>
        public virtual Expander CriteriaPanel { get; }

        /// <summary>Button to run the search</summary>
        public virtual Button SearchButton { get; }

        /// <summary>Button to reset the view</summary>
        public virtual Button ResetButton { get; }

        /// <summary>Grid with search results</summary>
        public virtual ItemsControl ResultsGrid { get; }

        /// <summary>Button to reset the view</summary>
        public virtual Button SelectButton { get; }

        /// <summary>Binds the view to its controller</summary>
        public override void BindTo(ViewController controller)
        {
            base.BindTo(controller);
            SearchViewController svController = Controller as SearchViewController;
            if (svController != null)
            {
                if (SearchButton != null)
                    SearchButton.Click += svController.Search;
                if (ResetButton != null)
                    ResetButton.Click += svController.Reset;
                if (CriteriaPanel != null)
                    CriteriaPanel.DataContext = svController.Criteria;
                if (ResultsGrid != null)
                    ResultsGrid.ItemsSource = svController.List;
            }
        }

        /// <summary>
        /// Activates the view
        /// </summary>
        /// <returns>True if the view was successfully activated, False otherwise</returns>
        public override bool Activate()
        {
            if (Controller != null && Controller.Params != null && SelectButton != null)
                SelectButton.Visibility = (Controller.Params[ViewParams.SelectionMode.Param] == null) ? Visibility.Hidden : Visibility.Visible;
            return true;
        }

        /// <summary>
        /// Shows whether criteria panel is collapsed. Null if there is no criteria panel
        /// </summary>
        public bool? CriteriaCollapsed
        {
            get { return (CriteriaPanel != null) ? (bool?)!CriteriaPanel.IsExpanded : null; }
            set { if (CriteriaPanel != null) CriteriaPanel.IsExpanded = value == null || !value.Value; }
        }
    }
}
