﻿// Copyright (c) 2025 Xomega.Net. All rights reserved.

using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Xomega.Framework.Blazor.Components;
using Xomega.Framework.Views;

namespace Xomega.Framework.Blazor.Views
{
	/// <summary>
	/// Base class for a blazor details view.
	/// </summary>
	public class BlazorPage : ComponentBase
    {
        /// <summary>
        /// Injected instance of the navigation service.
        /// </summary>
        [Inject] protected NavigationManager Navigation { get; set; }
        
		/// <summary>
        /// Main view for the page.
        /// </summary>
        protected BlazorView MainView { get; set; }

		/// <summary>
		/// The fragment that should be refreshed whenever the view is modified.
		/// Typically it contains the PageTitle and NavigationLock components on the page.
		/// </summary>
		protected Fragment ModifyFragment { get; set; }

		/// <inheritdoc/>
		protected override void OnAfterRender(bool firstRender)
		{
			base.OnAfterRender(firstRender);
			if (firstRender && MainView?.Model != null)
			{
				MainView.Model.PropertyChanged += OnMainViewPropertyChanged;
			}
		}

		private async void OnMainViewPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ViewModel.ViewTitleProperty)
			{
				if (ModifyFragment != null)
				{
					// to improve performance, refresh only the relevant fragment
					await ModifyFragment.Update();
				}
				else await InvokeAsync(() => StateHasChanged());
			}
		}

		/// <summary>
		/// Internal navigation handler for the NavigationLock component that prevents navigation
		/// if the main view cannot close.
		/// </summary>
		/// <param name="ctx">Location changing context</param>
		protected async Task ConfirmNavigation(LocationChangingContext ctx)
		{
			if (Navigation.Uri != ctx.TargetLocation && MainView != null && 
				!await MainView.CanCloseAsync())
			{
				ctx.PreventNavigation();
			}
		}
	}
}