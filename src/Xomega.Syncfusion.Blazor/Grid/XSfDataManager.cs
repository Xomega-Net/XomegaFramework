// Copyright (c) 2023 Xomega.Net. All rights reserved.

using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;
using System.Threading.Tasks;

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
            AdaptorInstance = typeof(DataListAdaptor);
            await base.OnInitializedAsync();
            if (BaseAdaptor.Instance is DataListAdaptor dla)
                dla.XDataManager = this;
        }
    }
}