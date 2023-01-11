// Copyright (c) 2023 Xomega.Net. All rights reserved.

using System;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base view interface
    /// </summary>
    public interface IView : IDisposable
    {
        /// <summary>
        /// Binds the view to its model, or unbinds the current model if null is passed.
        /// </summary>
        void BindTo(ViewModel viewModel);

        /// <summary>
        /// Shows the view using the mode it was activated with
        /// </summary>
        /// <returns>Whether or not the view was shown successfully</returns>
        bool Show();

        /// <summary>
        /// Checks if the view can be closed
        /// </summary>
        /// <returns>True if the view can be closed, False otherwise</returns>
        bool CanClose();

        /// <summary>
        /// Closes the view
        /// </summary>
        void Close();

        /// <summary>
        /// Checks if the view can be deleted
        /// </summary>
        /// <returns>True if the view can be deleted, False otherwise</returns>
        bool CanDelete();
    }
}
