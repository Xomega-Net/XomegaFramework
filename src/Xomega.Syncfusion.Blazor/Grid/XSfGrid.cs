// Copyright (c) 2025 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Grids.Internal;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xomega.Framework;

namespace Xomega._Syncfusion.Blazor
{
    /// <summary>
    /// Syncfusion Grid that binds to a Xomega list object.
    /// </summary>
    public class XSfGrid : SfGrid<DataRow>
    {
        private DataListObject list;
        private bool refresh = false;

        /// <summary>
        /// List object bound to this grid control.
        /// </summary>
        [Parameter] public DataListObject List
        {
            get => list;
            set
            {
                if (list == value) return;
                if (list != null)
                {
                    list.AsyncCollectionChanged -= OnListDataChanged;
                    list.AsyncPropertyChanged -= OnListPropertyChanged;
                }
                list = value;
                if (list != null)
                {
                    list.AsyncCollectionChanged += OnListDataChanged;
                    list.AsyncPropertyChanged += OnListPropertyChanged;

                }
                refresh = true; // trigger refresh when changing lists
            }
        }

        private async Task OnListPropertyChanged(object sender, PropertyChangedEventArgs e, CancellationToken token)
        {
            if (e.PropertyName == nameof(DataListObject.CurrentPage) && PageSettings != null)
            {
                PageSettings.CurrentPage = list.CurrentPage;
                await Refresh();
            }
        }

        /// <inheritdoc/>
        protected async override Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            if (refresh) await InvokeAsync(Refresh);
            refresh = false;
        }

        private async Task OnListDataChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token)
        {
            await Refresh();
        }

        /// <summary>
        /// Get indexes in the grid of the underlying data rows that are marked as selected.
        /// </summary>
        /// <returns>Array of indexes for selected data rows, or null if cannot get those.</returns>
        public double[] GetSelectedDataRowIndexes()
        {
            List<double> selIndexes = new List<double>();
            var rowsProp = GetType().GetProperty("Rows", BindingFlags.NonPublic | BindingFlags.Instance);
            if (rowsProp == null) 
                return null; // reflecting nonpublic members doesn't work in WASM
            if (rowsProp.GetValue(this) is List<Row<object>> gridRows)
            {
                foreach (var row in gridRows)
                {
                    if (row.Data is DataRow dataRow && dataRow.Selected && row.Index != null)
                        selIndexes.Add(row.Index.Value);
                }
            }
            return selIndexes.ToArray();
        }


        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (List != null)
                List.AsyncCollectionChanged -= OnListDataChanged;
        }
    }
}
