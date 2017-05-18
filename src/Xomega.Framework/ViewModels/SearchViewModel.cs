// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base class for models of search views with a results grid and a criteria panel
    /// </summary>
    public class SearchViewModel : ViewModel
    {
        #region Initialization/Activation

        /// <summary>
        /// Constructs a new search view model
        /// </summary>
        /// <param name="svcProvider">Service provider for the model</param>
        public SearchViewModel(IServiceProvider svcProvider) : base(svcProvider)
        {

        }

        /// <summary>
        /// Activates the view model and the view
        /// </summary>
        /// <param name="parameters">Parameters to activate the view with</param>
        /// <returns>True if the view was successfully activated, False otherwise</returns>
        public override bool Activate(NameValueCollection parameters)
        {
            if (!base.Activate(parameters) || List == null) return false;

            // set list selection mode from the parameters if passed
            if (Params[ViewParams.SelectionMode.Param] != null)
                List.RowSelectionMode = Params[ViewParams.SelectionMode.Param];

            // set criteria from the parameters
            if (List.CriteriaObject != null) List.CriteriaObject.SetValues(Params);

            // auto-run search if specified so in parameters, or if there are no criteria to set
            if (Params[ViewParams.Action.Param] == ViewParams.Action.Search || List.CriteriaObject == null)
                Search(false);

            // try to auto-select as appropriate and don't show the view if succeeded
            if (Params[ViewParams.Action.Param] == ViewParams.Action.Select && AutoSelect())
                return false;

            return true;
        }

        #endregion

        #region Data object

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
        /// Name for the CriteriaCollapsed observable property
        /// </summary>
        public const string CriteriaCollapsedProperty = "CriteriaCollapsed";

        private bool criteriaCollapsed;

        /// <summary>
        /// An indicator if the search criteria panel is collapsed
        /// </summary>
        public bool CriteriaCollapsed
        {
            get { return criteriaCollapsed; }
            set
            {
                criteriaCollapsed = value;
                OnPropertyChanged(new PropertyChangedEventArgs(CriteriaCollapsedProperty));
            }
        }

        /// <summary>
        /// Perfroms the search with the current criteria and populates the list
        /// </summary>
        /// <param name="preserveSelection">A flag indicating whether or not to preserve selection.</param>
        /// <returns>True on success, false in case of errors.</returns>
        public virtual bool Search(bool preserveSelection)
        {
            if (List == null) return false;
            try
            {
                Errors = null;
                List.Validate(true);
                List.GetValidationErrors().AbortIfHasErrors();
                List.Read(new DataListObject.PopulateListOptions { PreserveSelection = preserveSelection });
                return true;
            }
            catch (Exception ex)
            {
                Errors = errorParser.FromException(ex);
                return false;
            }
        }

        /// <summary>
        /// Search function exposed as an event handler for the Search button
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event argument</param>
        public virtual void Search(object sender, EventArgs e)
        {
            if (Search(true) && AutoCollapseCriteria)
                CriteriaCollapsed = true;
        }

        /// <summary>
        /// Resets current view to initial state
        /// </summary>
        /// <param name="sender">Command sender</param>
        /// <param name="e">Event arguments</param>
        public virtual void Reset(object sender, EventArgs e)
        {
            if (List != null)
            {
                List.ResetData();
                if (List.CriteriaObject != null)
                    List.CriteriaObject.ResetData();
            }
            if (AutoCollapseCriteria)
                CriteriaCollapsed = false;
        }

        #endregion

        #region Selection

        /// <summary>
        /// Automates row selection process
        /// </summary>
        /// <returns>True if automatic selection succeeded, false otherwise.</returns>
        public virtual bool AutoSelect()
        {
            if (List == null || List.CriteriaObject != null && !List.CriteriaObject.HasCriteria() || !Search(false)) return false;
            if (List.RowCount > 1 && AutoCollapseCriteria)
                CriteriaCollapsed = true;
            else if (List.RowCount == 0)
                CriteriaCollapsed = false;
            else if (List.RowCount == 1)
            {
                List.SelectRow(0);
                FireEvent(new ViewSelectionEvent(List.SelectedRows));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Default handler for convirming selected records and closing the view.
        /// </summary>
        public virtual void Select(object sender, EventArgs e)
        {
            if (List != null)
            {
                FireEvent(new ViewSelectionEvent(List.SelectedRows));
                Close();
            }
        }

        #endregion

        #region Child updates

        /// <summary>
        /// Handles child closing or change to refresh the list.
        /// </summary>
        /// <param name="childViewModel">Child view model that fired the original event</param>
        /// <param name="e">Event object</param>
        protected override void OnChildEvent(object childViewModel, ViewEvent e)
        {
            if (e.IsClosed() && List != null)
            {
                List.ClearSelectedRows();
                List.FireCollectionChanged();
            }
            if (e.IsSaved() || e.IsDeleted())
                Search(true);

            base.OnChildEvent(childViewModel, e);
        }

        #endregion
    }
}