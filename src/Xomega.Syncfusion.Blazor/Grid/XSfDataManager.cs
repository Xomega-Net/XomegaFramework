// Copyright (c) 2023 Xomega.Net. All rights reserved.

using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xomega.Framework;

namespace Xomega._Syncfusion.Blazor
{
    /// <summary>
    /// Data manager that helps binding a grid to a list object.
    /// </summary>
    public class XSfDataManager : SfDataManager
    {
        /// <inheritdoc/>
        protected async override Task OnInitializedAsync()
        {
            Adaptor = Adaptors.CustomAdaptor;
            //AdaptorInstance = typeof(DataListAdaptor);
            await base.OnInitializedAsync();
            //if (BaseAdaptor.Instance is DataListAdaptor dla)
            //    dla.XDataManager = this;
        }

        /// <summary>
        /// Provides access to the list object that the grid is bound to.
        /// </summary>
        public DataListObject List => (Parent as XSfGrid)?.List;

        /// <summary>
        /// Helps access selected rows of the grid.
        /// </summary>
        public List<DataRow> SelectedRows => (Parent as XSfGrid)?.SelectedRecords;

        /// <summary>
        /// Get the index of the row being inserted in the grid.
        /// </summary>
        public int GetNewRowIndex()
        {
            // TODO: figure out how to get the index of the row being inserted
            return 0;
        }
    }
}