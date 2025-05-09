﻿// Copyright (c) 2025 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Xomega.Framework.Blazor.Components
{
    /// <summary>
    /// Bootstrap-based grid component that binds to Xomega list objects.
    /// </summary>
    public partial class XGrid : ComponentBase
    {
        /// <summary>
        /// List object the grid is bound to.
        /// </summary>
        [Parameter] public DataListObject List { get; set; }

        /// <summary>
        /// The fragment of the grid that allows specifying grid columns.
        /// </summary>
        [Parameter] public RenderFragment GridColumns { get; set; }

        /// <summary>
        /// Whether or not the grid allows sorting.
        /// </summary>
        [Parameter] public bool AllowSorting { get; set; } = true;

        private List<XGridColumn> Columns { get; set; } = [];

        private IEnumerable<XGridColumn> VisibleColumns => Columns.Where(c => c.IsVisible);

        private IEnumerable<DataRow> VisibleRows => List?.CurrentPageData ?? [];

        private int RowCount => (List?.PagingMode == DataListObject.Paging.Server ?
            List?.TotalRowCount : List?.RowCount) ?? 0;

        internal void AddColumn(XGridColumn column)
        {
            Columns.Add(column);
        }

        private async Task OnHeaderClicked(XGridColumn col, MouseEventArgs e) => await col.OnHeaderClicked(e);

        #region Selection support

        /// <summary>
        /// A flag specifying whether or not to allow selection.
        /// </summary>
        [Parameter] public bool? AllowSelection { get; set; }

        /// <summary>
        /// Helper function returning the class for selected rows.
        /// </summary>
        /// <param name="row">The data row.</param>
        /// <returns>Row's CSS class based on whether or not the row is selected.</returns>
        protected string SelectedClass(DataRow row) => row.Selected ? "table-active" : "";

        /// <summary>
        /// Row's CSS class based on whether or not the list allows selection.
        /// </summary>
        protected virtual string SelectableClass => AllowSelection ?? false ||
            AllowSelection == null && List?.RowSelectionMode != null ? "table-hover" : "";

        /// <summary>
        /// Handles row selection on click, if the list latter is selectable.
        /// </summary>
        /// <param name="row">The row that was clicked.</param>
        protected virtual async Task RowClickedAsync(DataRow row)
        {
            if (SelectableClass != "")
            {
                if (RowSelected.HasDelegate)
                    await RowSelected.InvokeAsync(row);
                else List?.ToggleSelection(row);
            }
        }

        /// <summary>
        /// An event that is triggered when a grid's row has been selected.
        /// </summary>
        [Parameter] public EventCallback<DataRow> RowSelected { get; set; }

        #endregion

        /// <summary>
        /// Customizable resource key prefix to use when looking up grid resources.
        /// </summary>
        [Parameter] public string ResourceKey { get; set; }

        #region Pagination support

        /// <summary>
        /// A flag specifying whether or not to allow paging.
        /// </summary>
        public bool AllowPaging => !(List?.PagingMode == DataListObject.Paging.None);

        private async Task OnPagerCurrentPageChanged(int page) =>
            await List?.SetCurrentPage(page);

        private async Task OnPagerPageSizeChanged(int pageSize) =>
            await List?.SetPageSize(pageSize);

        /// <summary>
        /// An array of possible page sizes to select from.
        /// </summary>
        [Parameter] public int[] PageSizes { get; set; }

        /// <summary>
        /// The maximum number of pages to show on the pager.
        /// </summary>
        [Parameter] public int PagesToShow { get; set; }

        #endregion
    }
}