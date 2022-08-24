// Copyright (c) 2022 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        private List<XGridColumn> Columns { get; set; } = new List<XGridColumn>();

        private IEnumerable<XGridColumn> VisibleColumns => Columns.Where(c => c.IsVisible);

        private IEnumerable<DataRow> VisibleRows => (AllowPaging ? 
            List?.GetData()?.Skip(PageSize * (CurrentPage - 1))?.Take(PageSize) : List?.GetData())
            ?? new List<DataRow>();

        internal void AddColumn(XGridColumn column)
        {
            Columns.Add(column);
        }

        private void OnHeaderClicked(XGridColumn col, MouseEventArgs e) => col.OnHeaderClicked(e);

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
        [Parameter] public bool AllowPaging { get; set; } = true;

        /// <summary>
        /// The index of the current page, where 1 indicates the first page.
        /// </summary>
        [Parameter] public int CurrentPage { get; set; }

        /// <summary>
        /// Event for when the index of the current page changes.
        /// </summary>
        [Parameter] public EventCallback<int> CurrentPageChanged { get; set; }

        private async Task OnPagerCurrentPageChanged(int page)
        {
            CurrentPage = page;
            await CurrentPageChanged.InvokeAsync(page);
        }

        /// <summary>
        /// The current size of the page. If not explicitly specified,
        /// will default to the second option in the list of page sizes,
        /// or first if there is only one option.
        /// </summary>
        [Parameter] public int PageSize { get; set; }

        /// <summary>
        /// Event for when the page size changes.
        /// </summary>
        [Parameter] public EventCallback<int> PageSizeChanged { get; set; }

        private async Task OnPagerPageSizeChanged(int pageSize)
        {
            PageSize = pageSize;
            await PageSizeChanged.InvokeAsync(pageSize);
        }

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