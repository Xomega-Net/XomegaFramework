// Copyright (c) 2020 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xomega.Framework.Views;

namespace Xomega.Framework.Blazor.Views
{
    /// <summary>
    /// Base class for a blazor details view.
    /// </summary>
    public class BlazorDetailsView : BlazorView
    {
        /// <summary>
        /// View model as a details view model.
        /// </summary>
        protected DetailsViewModel DetailsModel => Model as DetailsViewModel;

        /// <summary>
        /// Exposed details object from the details view model.
        /// </summary>
        protected DataObject DetailsObject => DetailsModel?.DetailsObject;

        /// <summary>
        /// Determines if the view is modified.
        /// </summary>
        /// <returns>True if the view is modified, false otherwise.</returns>
        public bool IsModified() => DetailsObject?.IsModified() ?? false;

        /// <summary>
        /// Text for the Save button that can be overridden in subclasses.
        /// </summary>
        protected virtual string SaveText => "Save";

        /// <summary>
        /// Determines whether or not the Save button is enabled, which can be overridden in subclasses.
        /// </summary>
        protected virtual bool SaveEnabled => (Model as DetailsViewModel)?.SaveEnabled() ?? false;

        /// <summary>
        /// Text for the Delete button that can be overridden in subclasses.
        /// </summary>
        protected virtual string DeleteText => "Delete";

        /// <summary>
        /// Determines whether or not the Delete button is enabled, which can be overridden in subclasses.
        /// </summary>
        protected virtual bool DeleteEnabled => (Model as DetailsViewModel)?.DeleteEnabled() ?? false;

        /// <summary>
        /// Default handler for saving the view delegating the action to the view model.
        /// </summary>
        protected virtual async Task OnSaveAsync(MouseEventArgs e)
            => await DetailsModel?.SaveAsync();

        /// <summary>
        /// Default handler for deleting the view delegating the action to the view model.
        /// </summary>
        protected virtual async Task OnDeleteAsync(MouseEventArgs e)
            => await DetailsModel?.DeleteAsync();

        /// <inheritdoc/>
        public override void BindTo(ViewModel viewModel)
        {
            if (DetailsObject != null) DetailsObject.PropertyChanged -= OnDetailsObjectChanged;
            base.BindTo(viewModel);
            OnDetailsObjectChanged(this, new PropertyChangedEventArgs(DataObject.ModifiedProperty));
            if (DetailsObject != null) DetailsObject.PropertyChanged += OnDetailsObjectChanged;
        }

        /// <summary>
        /// Handles changes in the Modified property of the view model's data object, and updates the view title accordingly.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Property chang event arguments.</param>
        protected virtual async void OnDetailsObjectChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == DataObject.ModifiedProperty
                && (DetailsObject?.TrackModifications ?? false)
                && TitleComponent != null)
            {
                await TitleComponent.SetModified(IsModified());
            }
        }

        /// <summary>
        /// Asynchrounsly determines if the view can be closed. If the view is modified,
        /// then prompts the user to confirm discarding unsaved changes.
        /// The method can be overridden in subclasses to customize the confirmation prompt.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True, if the view can be closed, false otherwise.</returns>
        public override async Task<bool> CanCloseAsync(CancellationToken token = default)
        {
            if (IsModified())
            {
                return await JSRuntime.InvokeAsync<bool>("confirm", token,
                    "You have unsaved changes. Do you want to discard them and close the view?");
            }
            return await base.CanCloseAsync(token);
        }
    }
}