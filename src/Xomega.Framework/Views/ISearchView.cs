// Copyright (c) 2016 Xomega.Net. All rights reserved.

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Search view interface
    /// </summary>
    public interface ISearchView : IView
    {
        /// <summary>
        /// Shows whether criteria panel is collapsed. Null if there is no criteria panel
        /// </summary>
        bool? CriteriaCollapsed { get; set; }
    }
}
