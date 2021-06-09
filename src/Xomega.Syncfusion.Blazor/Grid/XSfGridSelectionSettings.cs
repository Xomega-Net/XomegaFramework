// Copyright (c) 2021 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids;
using System.Threading.Tasks;
using Xomega.Framework;

namespace Xomega._Syncfusion.Blazor
{
    /// <summary>
    /// Specialized Xomega Syncfusion grid selection settings that provide additional configuration
    /// or use selection mode configuration from the list object by default.
    /// </summary>
    public class XSfGridSelectionSettings : GridSelectionSettings
    {
        /// <summary>
        /// Whether the grid selection should be based on the selected data rows
        /// in the underlying list object.
        /// </summary>
        [Parameter] public bool UseListSelection { get; set; }

        /// <summary>
        /// Sets row selection type to single or multiple based on the bound list object's configuration.
        /// </summary>
        /// <returns></returns>
        protected async override Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            var list = (MainParent as XSfGrid)?.List;

            if (list != null && Mode == SelectionMode.Row)
            {
                switch (list.RowSelectionMode)
                {
                    case DataListObject.SelectionModeSingle:
                        Type = SelectionType.Single; break;
                    case DataListObject.SelectionModeMultiple:
                        Type = SelectionType.Multiple; break;
                }
            }
        }
    }
}
