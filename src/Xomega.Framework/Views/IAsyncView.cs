// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base view interface
    /// </summary>
    public interface IAsyncView : IDisposable
    {
        /// <summary>
        /// Binds the view to its model, or unbinds the current model if null is passed.
        /// </summary>
        void BindTo(ViewModel viewModel);

        /// <summary>
        /// Shows the view using the mode it was activated with.
        /// </summary>
        /// <returns>Whether or not the view was shown successfully</returns>
        Task<bool> ShowAsync(CancellationToken token = default);

        /// <summary>
        /// Checks if the view can be closed.
        /// </summary>
        /// <returns>True if the view can be closed, False otherwise</returns>
        Task<bool> CanCloseAsync(CancellationToken token = default);

        /// <summary>
        /// Closes the view.
        /// </summary>
        Task CloseAsync(CancellationToken token = default);

        /// <summary>
        /// Checks if the view can be deleted.
        /// </summary>
        /// <returns>True if the view can be deleted, False otherwise</returns>
        Task<bool> CanDeleteAsync(CancellationToken token = default);
    }
}
