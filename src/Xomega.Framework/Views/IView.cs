// Copyright (c) 2016 Xomega.Net. All rights reserved.

using System;
using System.Collections.Specialized;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base view interface
    /// </summary>
    public interface IView : IDisposable
    {
        /// <summary>
        /// Binds the view to its controller
        /// </summary>
        void BindTo(ViewController controller);

        /// <summary>
        /// Shows the view using the mode it was activated with
        /// </summary>
        /// <param name="owner">View owner</param>
        /// <returns>Whether or not the view was shown successfully</returns>
        bool Show(object owner);

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
        /// Gets the child view for the current view
        /// </summary>
        /// <returns>Current child view if any</returns>
        IView GetChildView();
    }
}
