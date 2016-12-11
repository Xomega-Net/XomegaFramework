// Copyright (c) 2016 Xomega.Net. All rights reserved.

using System;
using System.Collections.Specialized;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base view interface
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// Binds the view to its controller
        /// </summary>
        void BindTo(ViewController controller);

        /// <summary>
        /// Activates the view
        /// </summary>
        /// <returns>True if the view was successfully activated, False otherwise</returns>
        bool Activate();

        /// <summary>
        /// Updates the view based on other views' changes
        /// </summary>
        void Update();

        /// <summary>
        /// Shows the view using the mode it was activated with
        /// </summary>
        /// <param name="owner">View owner</param>
        void Show(object owner);

        /// <summary>
        /// Checks if the view can be closed
        /// </summary>
        /// <returns>True if the view can be closed, False otherwise</returns>
        bool CanClose();

        /// <summary>
        /// Hides the view
        /// </summary>
        void Hide();
    }
}
