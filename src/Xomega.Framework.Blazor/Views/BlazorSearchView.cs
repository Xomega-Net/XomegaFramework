// Copyright (c) 2021 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xomega.Framework.Views;

namespace Xomega.Framework.Blazor.Views
{
    /// <summary>
    /// Base class for a blazor search view.
    /// </summary>
    public class BlazorSearchView : BlazorView
    {
        /// <summary>
        /// View model as a search view model.
        /// </summary>
        protected SearchViewModel SearchModel => Model as SearchViewModel;

        /// <summary>
        /// Exposed list object from the search view model.
        /// </summary>
        protected DataListObject ListObject => SearchModel?.List;

        /// <summary>
        /// Search view model's indicator if the criteria panel is collapsed
        /// </summary>
        public bool CriteriaCollapsed
        {
            get { return SearchModel?.CriteriaCollapsed ?? false; }
            set
            {
                if (SearchModel != null)
                    SearchModel.CriteriaCollapsed = value;
            }
        }

        /// <summary>
        /// The 1-based index of the currently displayed page.
        /// </summary>
        protected int CurrentPage { get; set; } = 1;
        
        /// <summary>
        /// The size of the page to display.
        /// </summary>
        protected int CurrentPageSize { get; set; }

        /// <summary>
        /// Text for the Search button that can be overridden in subclasses.
        /// </summary>
        protected virtual string SearchText => "Search";

        /// <summary>
        /// Text for the Reset button that can be overridden in subclasses.
        /// </summary>
        protected virtual string ResetText => "Reset";

        /// <summary>
        /// Text for the Refresh button that can be overridden in subclasses.
        /// </summary>
        protected virtual string RefreshText => "Refresh";

        /// <summary>
        /// Text for the PermaLink button that can be overridden in subclasses.
        /// </summary>
        protected virtual string PermaLinkText => "PermaLink";

        /// <summary>
        /// Text for the Select button that can be overridden in subclasses.
        /// </summary>
        protected virtual string SelectText => "Select";

        /// <summary>
        /// Determines whether or not the Select button is visible, which can be overridden in subclasses.
        /// </summary>
        protected virtual bool SelectVisible => Model?.Params?[ViewParams.SelectionMode.Param] != null;

        /// <inheritdoc/>
        public override void BindTo(ViewModel viewModel)
        {
            if (ListObject != null) ListObject.CollectionChanged -= OnListChanged;
            base.BindTo(viewModel);
            if (ListObject != null) ListObject.CollectionChanged += OnListChanged; ;
        }

        private void OnListChanged(object sender, NotifyCollectionChangedEventArgs e)
            => StateHasChanged();

        #region Event handlers

        /// <summary>
        /// Default handler for searching that delegates the action to the view model.
        /// </summary>
        protected virtual async Task OnSearchAsync(MouseEventArgs e)
        {
            await SearchModel?.SearchAsync();
            CurrentPage = 1;
        }

        /// <summary>
        /// Default handler for refreshing that delegates the action to the view model.
        /// </summary>
        protected virtual async Task OnRefreshClicked(MouseEventArgs e) => await OnSearchAsync(e);

        /// <summary>
        /// Default handler for resetting that delegates the action to the view model.
        /// </summary>
        protected virtual async Task OnResetAsync(MouseEventArgs e)
        {
            SearchModel?.Reset(this, e);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Default handler for the PermaLink action that adds criteria to the URL.
        /// </summary>
        protected virtual async Task OnPermaLinkAsync(MouseEventArgs e)
        {
            if (ListObject?.CriteriaObject == null) return;
            var criteria = ListObject.CriteriaObject.ToNameValueCollection();
            var dict = new Dictionary<string, string>();
            foreach (string key in criteria.Keys) dict[key] = criteria[key];
            var uri = new Uri(Navigation.Uri);
            Navigation.NavigateTo(QueryHelpers.AddQueryString(uri.AbsolutePath, dict));
            await Task.CompletedTask;
        }

        /// <summary>
        /// Default handler for selectinging that delegates the action to the view model.
        /// </summary>
        protected virtual async Task OnSelectAsync(MouseEventArgs e)
            => await SearchModel?.SelectAsync();

        #endregion
    }
}
