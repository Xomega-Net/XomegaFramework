// Copyright (c) 2025 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(OpenInlineViews))
                    UpdateColumnVisibility();
            };
        }

        /// <inheritdoc/>
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
                Search(false, false);

            // try to auto-select as appropriate and don't show the view if succeeded
            if (Params[ViewParams.Action.Param] == ViewParams.Action.Select && AutoSelect())
                return false;

            return true;
        }

        /// <inheritdoc/>
        public override async Task<bool> ActivateAsync(NameValueCollection parameters, CancellationToken token = default)
        {
            if (!await base.ActivateAsync(parameters, token) || List == null) return false;

            // set list selection mode from the parameters if passed
            if (Params[ViewParams.SelectionMode.Param] != null)
            {
                List.RowSelectionMode = Params[ViewParams.SelectionMode.Param];
                List.SelectAction.Visible = true;
            }
            else List.SelectAction.Visible = false;

            // set criteria from the parameters
            if (List.CriteriaObject != null) await List.CriteriaObject.SetValuesAsync(Params, token);

            // auto-run search if specified so in parameters, or if there are no criteria to set
            if (Params[ViewParams.Action.Param] == ViewParams.Action.Search || List.CriteriaObject == null)
                await SearchAsync(false, token);

            // try to auto-select as appropriate and don't show the view if succeeded
            if (Params[ViewParams.Action.Param] == ViewParams.Action.Select && await AutoSelectAsync(token))
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
        /// Performs the search with the current criteria and populates the list
        /// </summary>
        /// <param name="reload">True to reload with currently applied criteria, false to use the CriteriaObject.</param>
        /// <param name="preserveSelection">A flag indicating whether or not to preserve selection.</param>
        /// <returns>True on success, false in case of errors.</returns>
        public virtual bool Search(bool reload, bool preserveSelection)
        {
            if (List == null) return false;
            try
            {
                List.Validate(true);
                ErrorList msgList = List.GetValidationErrors();
                msgList.AbortIfHasErrors();
                msgList.MergeWith(List.Read(new DataListObject.ReadOptions {
                    IsReload = reload,
                    PreserveSelection = preserveSelection 
                }));
                Errors = msgList;
                return !msgList.HasErrors();
            }
            catch (Exception ex)
            {
                Errors = ErrorParser.FromException(ex);
                return false;
            }
        }

        /// <summary>
        /// Performs asynchronous search with the current criteria and populates the list.
        /// </summary>
        /// <param name="reload">True to reload with currently applied criteria, false to use the CriteriaObject.</param>
        /// <param name="preserveSelection">A flag indicating whether or not to preserve selection.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True on success, false in case of errors.</returns>
        public virtual async Task<bool> SearchAsync(bool reload, bool preserveSelection, CancellationToken token = default)
        {
            if (List == null) return false;
            try
            {
                List.Validate(true);
                ErrorList msgList = List.GetValidationErrors();
                msgList.AbortIfHasErrors();
                var res = await List.ReadAsync(new DataListObject.ReadOptions {
                    IsReload = reload,
                    PreserveSelection = preserveSelection
                }, token);
                msgList.MergeWith(res);
                Errors = msgList;
                return !msgList.HasErrors();
            }
            catch (Exception ex)
            {
                Errors = ErrorParser.FromException(ex);
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
            if (Search(false, true) && AutoCollapseCriteria)
                CriteriaCollapsed = true;
        }

        /// <summary>
        /// Search function exposed as an event handler for the Search button.
        /// </summary>
        /// <param name="reload">True to reload with currently applied criteria, false to use the CriteriaObject.</param>
        /// <param name="token">Cancellation token.</param>
        public virtual async Task SearchAsync(bool reload, CancellationToken token = default)
        {
            if (await SearchAsync(reload, true, token) && AutoCollapseCriteria && !reload)
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
                List.CriteriaObject?.ResetData();
                Errors = null;
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
            if (List == null || List.CriteriaObject != null && !List.CriteriaObject.HasCriteria() || !Search(false, false))
                return false;
            bool res = DoAutoSelect();
            if (res) FireEvent(new ViewSelectionEvent(List.SelectedRows));
            return res;
        }

        /// <summary>
        /// Automates async row selection process.
        /// </summary>
        /// <returns>True if automatic selection succeeded, false otherwise.</returns>
        public virtual async Task<bool> AutoSelectAsync(CancellationToken token = default)
        {
            if (List == null || List.CriteriaObject != null && !List.CriteriaObject.HasCriteria() ||
                !(await SearchAsync(false, false, token))) return false;
            bool res = DoAutoSelect();
            if (res) await FireEventAsync(new ViewSelectionEvent(List.SelectedRows), token);
            return res;
        }

        /// <summary>
        /// Automates row selection process after search.
        /// </summary>
        /// <returns>True if automatic selection succeeded, false otherwise.</returns>
        protected virtual bool DoAutoSelect()
        {
            if (List.RowCount > 1 && AutoCollapseCriteria)
                CriteriaCollapsed = true;
            else if (List.RowCount == 0)
                CriteriaCollapsed = false;
            else if (List.RowCount == 1)
            {
                List.SelectRow(0);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Default handler for confirming selected records and closing the view.
        /// </summary>
        public virtual void Select(object sender, EventArgs e)
        {
            if (List != null)
            {
                FireEvent(new ViewSelectionEvent(List.SelectedRows));
                Close();
            }
        }

        /// <summary>
        /// Default async handler for confirming selected records and closing the view.
        /// </summary>
        public virtual async Task SelectAsync(CancellationToken token = default)
        {
            if (List != null)
            {
                await FireEventAsync(new ViewSelectionEvent(List.SelectedRows), token);
                await CloseAsync(token);
            }
        }

        #endregion

        #region Child updates

        /// <summary>
        /// Updates selected rows in the list when the child details view is opened or closed.
        /// </summary>
        /// <param name="dvm">View model of the child details view.</param>
        /// <param name="e">View event of the child details view.</param>
        protected virtual void UpdateDetailsSelection(DetailsViewModel dvm, ViewEvent e)
        {
            var keyChildProps = dvm?.DetailsObject?.Properties?.Where(p => p.IsKey)?.ToList();
            UpdateListSelection(List, keyChildProps, e);
        }


        /// <summary>
        /// Handles child closing or change to refresh the list.
        /// </summary>
        /// <param name="childViewModel">Child view model that fired the original event</param>
        /// <param name="e">Event object</param>
        protected override void OnChildEvent(object childViewModel, ViewEvent e)
        {
            UpdateDetailsSelection(childViewModel as DetailsViewModel, e);
            if (e.IsSaved(false) || e.IsDeleted(false))
                Search(true, true);

            base.OnChildEvent(childViewModel, e);
        }

        /// <summary>
        /// Handles child closing or change to refresh the list.
        /// </summary>
        /// <param name="childViewModel">Child view model that fired the original event</param>
        /// <param name="e">Event object</param>
        /// <param name="token">Cancellation token.</param>
        protected override async Task OnChildEventAsync(object childViewModel, ViewEvent e, CancellationToken token = default)
        {
            UpdateDetailsSelection(childViewModel as DetailsViewModel, e);
            if (e.IsSaved(false) || e.IsDeleted(false))
                await SearchAsync(true, token);

            await base.OnChildEventAsync(childViewModel, e, token);
        }

        /// <summary>
        /// Update column visibility based on the number of open inline views,
        /// in order to hide less relevant columns when child views are open
        /// to make list columns fit into smaller space.
        /// </summary>
        protected virtual void UpdateColumnVisibility()
        {
            int inlineViews = OpenInlineViews;
            if (List == null || inlineViews == 0) return;

            var props = RankedProperties;
            int propsToShow = props.Count() / inlineViews;
            int i = 0;
            foreach (var p in props)
            {
                p.Visible = i++ <= propsToShow;
            }
        }

        /// <summary>
        /// Returns a list of properties ordered by their importance.
        /// It uses the natural order by default, but subclasses can override it as needed.
        /// </summary>
        protected virtual IEnumerable<DataProperty> RankedProperties => List.Properties;

        #endregion
    }
}