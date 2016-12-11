// Copyright (c) 2016 Xomega.Net. All rights reserved.

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Search view interface
    /// </summary>
    public interface IDetailsView : IView
    {
        /// <summary>
        /// An indicator if the object is new and not yet saved
        /// </summary>
        bool IsNew { get; set; }
    }
}