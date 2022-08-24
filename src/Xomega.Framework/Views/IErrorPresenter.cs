// Copyright (c) 2022 Xomega.Net. All rights reserved.

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base interface for an errors presenter.
    /// </summary>
    public interface IErrorPresenter
    {
        /// <summary>
        /// Present the specified error list
        /// </summary>
        void Show(ErrorList errors);
    }
}
