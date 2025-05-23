﻿// Copyright (c) 2025 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Specialized;
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
        /// Localized text for the title of the search criteria bar.
        /// </summary>
        protected virtual string CriteriaText => Model?.GetString(Messages.View_Criteria);

        /// <inheritdoc/>
        protected override bool FooterVisible => base.FooterVisible ||
            (ListObject?.SelectAction?.Visible ?? false);

        /// <inheritdoc/>
        public override void BindTo(ViewModel viewModel)
        {
            if (ListObject != null) ListObject.CollectionChanged -= OnListChanged;
            base.BindTo(viewModel);
            if (ListObject != null) ListObject.CollectionChanged += OnListChanged;
        }

        private void OnListChanged(object sender, NotifyCollectionChangedEventArgs e)
            => StateHasChanged();

        #region Event handlers

        /// <summary>
        /// Default handler for searching that delegates the action to the view model.
        /// </summary>
        protected virtual async Task OnSearchAsync(MouseEventArgs e) => await SearchModel?.SearchAsync(false);

        /// <summary>
        /// Default handler for refreshing that delegates the action to the view model.
        /// </summary>
        protected virtual async Task OnRefreshAsync(MouseEventArgs e) => await SearchModel?.SearchAsync(true);

        /// <summary>
        /// Default handler for resetting that delegates the action to the view model.
        /// </summary>
        protected virtual async Task OnResetAsync(MouseEventArgs e) => await SearchModel?.ResetAsync(this, e);

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
        /// Default handler for selecting that delegates the action to the view model.
        /// </summary>
        protected virtual async Task OnSelectAsync(MouseEventArgs e)
            => await SearchModel?.SelectAsync();

        #endregion
    }
}
