// Copyright (c) 2020 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xomega.Framework.Blazor.Components
{
    /// <summary>
    /// Base class for an index-based pager component.
    /// </summary>
    public class BasePager : ComponentBase
    {
        /// <summary>
        /// The content to display for the current page.
        /// </summary>
        [Parameter]
        public RenderFragment<IEnumerable<int>> ChildContent { get; set; }

        /// <summary>
        /// An array of possible page sizes to select from.
        /// </summary>
        [Parameter]
        public int[] PageSizes { get; set; } = new[] { 10, 20, 50, 100 };

        /// <summary>
        /// The current size of the page. If not explicitly specified,
        /// will default to the second option in the list of page sizes,
        /// or first if there is only one option.
        /// </summary>
        [Parameter]
        public int PageSize { get; set; }

        /// <summary>
        /// Event for when the size of the page changes.
        /// </summary>
        [Parameter]
        public EventCallback<int> PageSizeChanged { get; set; }

        /// <summary>
        /// The maximum number of pages to show on the pager.
        /// </summary>
        [Parameter]
        public int PagesToShow { get; set; } = 5; // maximum number of pages to display

        /// <summary>
        /// The index of the current page, where 1 indicates the first page.
        /// </summary>
        [Parameter]
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// Event for when the index of the current page changes.
        /// </summary>
        [Parameter]
        public EventCallback<int> CurrentPageChanged { get; set; }

        /// <summary>
        /// The total number of items in the underlying list.
        /// </summary>
        [Parameter]
        public int ItemsCount { get; set; }

        /// <summary>
        /// Index of the last page based on the total number of items and the current page size.
        /// </summary>
        protected virtual int MaxPage => (int)Math.Ceiling((double)ItemsCount / PageSize);

        /// <summary>
        /// Indexes of the items on the current page.
        /// </summary>
        protected virtual IEnumerable<int> Indexes => Enumerable.Range(PageSize * (CurrentPage - 1),
                           Math.Max(0, Math.Min(PageSize, ItemsCount - PageSize * (CurrentPage - 1))));

        /// <inheritdoc/>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            // if page size is not provided, default it to the second option (20)
            if (PageSize == 0 && PageSizes?.Length > 0)
                await SetPageSize(PageSizes[Math.Min(1, PageSizes.Length - 1)]);
        }

        /// <summary>
        /// Gets an array of page indexes to display based on the current page 
        /// and the maximum number of pages to show.
        /// </summary>
        protected virtual int[] GetPagesRange()
        {
            int pagerOffset = (int)Math.Floor((double)PagesToShow / 2);
            int oddPagerSize = pagerOffset * 2 + 1; // ensure odd pager size
            int pageFrom = Math.Max(1, Math.Min(MaxPage - oddPagerSize + 1, CurrentPage - pagerOffset));
            int pageTo = Math.Min(MaxPage, Math.Max(oddPagerSize, CurrentPage + pagerOffset));
            var res = new int[pageTo - pageFrom + 1];
            for (int i = 0; i < res.Length; i++) res[i] = pageFrom + i;
            return res;
        }

        /// <summary>
        /// Sets the current page to the specified index asynchrounsly.
        /// </summary>
        /// <param name="page">The index of the page to set.</param>
        /// <returns>A task for this function.</returns>
        protected virtual async Task SetCurrentPage(int page)
        {
            if (page < 1 || page > MaxPage) return;
            if (page != CurrentPage)
            {
                CurrentPage = page;
                await CurrentPageChanged.InvokeAsync(CurrentPage);
            }
        }

        /// <summary>
        /// Sets the current page size to the specified value asynchrounsly.
        /// </summary>
        /// <param name="pageSize">The size of the page to set.</param>
        /// <returns>A task for this function.</returns>
        protected virtual async Task SetPageSize(int pageSize)
        {
            if (pageSize < 1) return;
            if (pageSize != PageSize)
            {
                PageSize = pageSize;
                await PageSizeChanged.InvokeAsync(PageSize);
            }
        }

        /// <summary>
        /// Handles change of the page size asynchrounsly.
        /// </summary>
        /// <param name="e">The chang event.</param>
        /// <returns>A task for this function.</returns>
        protected virtual async Task OnPageSizeChanged(ChangeEventArgs e)
        {
            int firstRecord = PageSize * (CurrentPage - 1);
            await SetPageSize(int.Parse(e.Value.ToString()));
            // update the current page to make the first record visible
            await SetCurrentPage(firstRecord / PageSize + 1);
        }
    }
}