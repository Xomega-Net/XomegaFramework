// Copyright (c) 2022 Xomega.Net. All rights reserved.

using System.Collections.Specialized;
using System.ComponentModel;
using Xomega.Framework.Blazor.Views;
using Xomega.Framework.Views;

namespace Xomega._Syncfusion.Blazor
{
    /// <summary>
    /// Base class for search views that use Syncfusion controls that provides some fixes and workarounds.
    /// </summary>
    public class XSfSearchView : BlazorSearchView
    {
        /// <summary>
        /// Refreshes the grid for the list object when inline child views are open/closed,
        /// since the list hides/shows columns, which Syncfusion grid doesn't handle well.
        /// </summary>
        protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnModelPropertyChanged(sender, e);
            if (e.PropertyName == nameof(ViewModel.OpenInlineViews))
            {
                // refresh the grid to work around issues with SfGrid
                // not updating properly after columns are hidden/revealed
                ListObject?.FireCollectionChange(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset));
            }
        }
    }
}