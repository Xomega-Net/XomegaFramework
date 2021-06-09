// Copyright (c) 2021 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
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

        /// <inheritdoc/>
        protected override string UpperClass => Mode == null ? "container d-flex align-items-center justify-content-center" : base.UpperClass;

        /// <inheritdoc/>
        protected override string LowerClass => Mode == null ? "modal-content" :base.LowerClass;

        /// <inheritdoc/>
        protected override bool FooterVisible => base.FooterVisible || 
            (DetailsObject?.SaveAction?.Visible ?? false) ||
            (DetailsObject?.DeleteAction?.Visible ?? false);

        /// <inheritdoc/>
        public override async Task<bool> CanDeleteAsync(CancellationToken token = default)
        {
            var msg = Model.GetString(Messages.View_DeleteMessage);
            return await JSRuntime.InvokeAsync<bool>("confirm", token, msg);
        }

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
                var msg = Model.GetString(Messages.View_UnsavedMessage);
                return await JSRuntime.InvokeAsync<bool>("confirm", token, msg);
            }
            return await base.CanCloseAsync(token);
        }
    }
}