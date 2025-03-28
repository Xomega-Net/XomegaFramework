﻿// Copyright (c) 2025 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xomega.Framework;
using Xomega.Framework.Operators;
using ListSortDirection = Xomega.Framework.ListSortDirection;

namespace Xomega._Syncfusion.Blazor
{
    /// <summary>
    /// Custom Syncfusion data grid adapter for working with Xomega data list objects.
    /// </summary>
    public partial class DataListAdaptor : DataAdaptor
    {
        /// <summary>
        /// Any child content for the data adapter.
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }

        /// <summary>
        /// Data list object used as the data source for the current grid.
        /// </summary>
        [Parameter] public DataListObject List { get; set; }

        internal XSfDataManager XDataManager { get; set; }

        /// <summary>
        /// Parent XSfGrid component.
        /// </summary>
        protected XSfGrid Grid
        {
            get
            {
                SfDataManager dm = XDataManager;
                if (dm == null)
                {
                    var dmProp = typeof(DataAdaptor).GetProperty("DataManager", BindingFlags.NonPublic | BindingFlags.Instance);
                    dm = dmProp?.GetValue(this) as SfDataManager;
                }
                return dm?.BaseAdaptor?.ParentComponent as XSfGrid;
            }
        }

        /// <summary>
        /// Get a data list object for the data source
        /// </summary>
        /// <returns></returns>
        protected DataListObject GetListObject() => List ?? Grid?.List;

        private OperatorRegistry operatorRegistry;

        /// <summary>
        /// Default constructor for the data list object adapter
        /// </summary>
        public DataListAdaptor()
        {
        }

        private OperatorRegistry GetOperatorRegistry()
        {
            if (operatorRegistry == null)
                operatorRegistry = GetListObject()?.ServiceProvider?.GetService<OperatorRegistry>() ?? new OperatorRegistry();
            return operatorRegistry;
        }

        private Framework.Operators.Operator GetOperator(string oper)
            => GetOperatorRegistry().GetOperator(oper, null) ??
            throw new ArgumentException($"Operator {oper} is not registered with the OperatorRegistry.");

        /// <inheritdoc/>
        public async override Task<object> ReadAsync(DataManagerRequest dm, string key = null)
        {
            DataListObject list = GetListObject();
            if (list == null)
                return dm.RequiresCounts ? new DataResult() { Result = null, Count = 0 } : null;

            ListSortCriteria sortCrit = null;
            if (dm.Sorted != null && dm.Sorted.Count > 0)
            {
                sortCrit = ToSortCriteria(dm.Sorted.Reverse<Sort>()); // multi-column sort comes in reverse order
            }

            if (dm.Take != 0 || dm.Skip != 0)
                await list.SkipTakeAsync(dm.Skip, dm.Take, sortCrit);

            IEnumerable<DataRow> data = list.GetData();
            if (dm.Where != null && dm.Where.Count > 0)
                data = data.Where(r => Matches(r, "and", dm.Where, false));
            if (dm.Search != null && dm.Search.Count > 0)
                data = data.Where(r => Matches(r, "and", SearchToWhere(dm.Search), true));

            data = data.ToList();

            int count = 0;
            if (list.PagingMode == DataListObject.Paging.Server)
            {
                count = list.TotalRowCount ?? data.Count();
            }
            else
            {
                count = data.Count();
                data = data.Skip((list.CurrentPage - 1) * list.PageSize)
                    .Take(list.PageSize);
            }

            if (dm.Group != null)
            {
                IEnumerable groupedData = data;
                foreach (var group in dm.Group)
                {
                    groupedData = DataUtil.Group<DataRow>(groupedData, group, dm.Aggregates, 0, dm.GroupByFormatter);
                }
                return dm.RequiresCounts ? new DataResult() { Result = groupedData, Count = count } : (object)data;
            }
            var res = dm.RequiresCounts ? new DataResult() { Result = data, Count = count } : (object)data;
            await Task.Yield();
            return await Task.FromResult(res);
        }

        private bool Matches(DataRow row, string condition, IEnumerable<WhereFilter> criteria, bool displayFormat)
        {
            bool isOr = condition == "or";
            foreach (var c in criteria)
            {
                bool m;
                if (c.IsComplex)
                    m = Matches(row, c.Condition, c.predicates, displayFormat);
                else
                {
                    var prop = row.List[c.Field];
                    if (prop == null) continue;
                    var op = GetOperator(c.Operator);
                    m = row.List.PropertyValueMatches(prop, row, op, c.value, !c.IgnoreCase, displayFormat);
                }
                if (isOr == m) return m; // no need to evaluate further
            }
            return !isOr;
        }

        private IEnumerable<WhereFilter> SearchToWhere(IEnumerable<SearchFilter> search)
            => search.Select(sf => new WhereFilter()
            {
                IsComplex = true,
                Condition = "or",
                predicates = sf.Fields.Select(f => new WhereFilter()
                {
                    Field = f,
                    value = sf.Key,
                    Operator = sf.Operator,
                    IgnoreCase = sf.IgnoreCase
                }).ToList()
            });

        private ListSortCriteria ToSortCriteria(IEnumerable<Sort> sort)
            => [.. sort.Select(s => new ListSortField()
            {
                PropertyName = s.Name,
                SortDirection = ListSortDirection.FromString(s.Direction)
            })];

        /// <inheritdoc/>
        public async override Task<object> InsertAsync(DataManager dataManager, object data, string key)
        {
            var list = GetListObject();
            // insert data row, but suppress any notifications, since SfGrid cannot handle any UI updates at this point
            if (list != null && data is DataRow row)
            {
                int idx = 0; // TODO: figure out how to get the index of the row being inserted
                await list.InsertAsync(idx, row, true);
            }
            return data;
        }

        /// <inheritdoc/>
        public async override Task<object> UpdateAsync(DataManager dataManager, object data, string keyField, string key)
        {
            var list = GetListObject();
            if (list != null && data is DataRow row)
                await list.UpdateRow(row.OriginalRow, row, true);
            return data;
        }

        /// <inheritdoc/>
        public async override Task<object> RemoveAsync(DataManager dataManager, object data, string keyField, string key)
        {
            var list = GetListObject();
            var keyProp = keyField == null ? null : list?[keyField];
            var rowsToRemove = keyProp == null ? new List<DataRow>() :
                list.GetData().Where(r => keyProp.GetValue(ValueFormat.Internal, r) == data).ToList();
            if (!rowsToRemove.Any())
                rowsToRemove = Grid?.SelectedRecords;
            await list.RemoveRows(rowsToRemove, true);
            return rowsToRemove;
        }

        /// <inheritdoc/>
        public async override Task<object> BatchUpdateAsync(DataManager dataManager, object changedRecords,
            object addedRecords, object deletedRecords, string keyField, string key, int? dropIndex)
        {
            if (deletedRecords is IEnumerable<DataRow> rowsToRemove)
                await GetListObject()?.RemoveRows(rowsToRemove, true);

            return await base.BatchUpdateAsync(dataManager, changedRecords, addedRecords, deletedRecords, keyField, key, dropIndex);
        }
    }
}