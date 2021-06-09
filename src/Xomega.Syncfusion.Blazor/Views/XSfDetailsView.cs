// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System.Collections.Specialized;
using System.Linq;
using Xomega.Framework;
using Xomega.Framework.Blazor.Views;
using Xomega.Framework.Views;

namespace Xomega._Syncfusion.Blazor
{
    /// <summary>
    /// Base class for details views that use Syncfusion controls that provides some fixes and workarounds.
    /// </summary>
    public class XSfDetailsView : BlazorDetailsView
    {
        /// <summary>
        /// Overrides binding to a view model in order to manually refresh any of the view's grids
        /// bound to the main details object.
        /// </summary>
        /// <param name="viewModel"></param>
        public override void BindTo(ViewModel viewModel)
        {
            base.BindTo(viewModel);
            // workaround to manually refresh any grids bound to child lists,
            // since SfGrid doesn't get refreshed automatically
            RefreshGrids(DetailsObject);
        }

        /// <summary>
        /// Forcing refresh of any grids bound to the specified list object
        /// or any of the list object children of a regular data object.
        /// </summary>
        /// <param name="dataObject">List object or a data object with child lists.</param>
        protected void RefreshGrids(DataObject dataObject)
        {
            if (dataObject is DataListObject listObject)
                listObject.FireCollectionChange(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            else if (dataObject?.Children?.Count() > 0)
            {
                foreach (DataObject childObj in dataObject.Children)
                    RefreshGrids(childObj);
            }
        }
    }
}