// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System.Windows;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// A service for creating a window for a view.
    /// </summary>
    public interface IWindowCreator
    {
        /// <summary>
        /// Creates a window for the specified view.
        /// </summary>
        Window CreateWindow(WPFView view);
    }
}
