// Copyright (c) 2021 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xomega.Framework;
using Xomega.Framework.Operators;
using ListSortDirection = Xomega.Framework.ListSortDirection;

namespace Xomega._Syncfusion.Blazor
{
    /// <summary>
    /// Custom Syncfusion data grid adaptor for working with Xomega data list objects.
    /// </summary>
    public class DataListAdaptor : DataAdaptor
    {
        /// <summary>
        /// A reference to the data manager that provides access to the underlying list object,
        /// selected rows and the grid as needed.
        /// </summary>
        public XSfDataManager DataManager { get; set; }

        private OperatorRegistry operatorRegistry;

        /// <summary>
        /// Default constructor for the data list object adaptor
        /// </summary>
        public DataListAdaptor()
        {
        }

        private OperatorRegistry GetOperatorRegistry()
        {
            if (operatorRegistry == null)
                operatorRegistry = DataManager?.List?.ServiceProvider?.GetService<OperatorRegistry>() ?? new OperatorRegistry();
            return operatorRegistry;
        }

        private Framework.Operators.Operator GetOperator(string oper)
            => GetOperatorRegistry().GetOperator(oper, null) ??
            throw new ArgumentException($"Operator {oper} is not registered with the OperatorRegistry.");

        /// <inheritdoc/>
        public async override Task<object> ReadAsync(DataManagerRequest dm, string key = null)
        {
            DataListObject list = DataManager?.List;
            if (list == null)
                return dm.RequiresCounts ? new DataResult() { Result = null, Count = 0 } : null;

            IEnumerable<DataRow> data = list.GetData();
            if (dm.Where != null && dm.Where.Count > 0)
                data = data.Where(r => Matches(r, "and", dm.Where));
            if (dm.Search != null && dm.Search.Count > 0)
                data = data.Where(r => Matches(r, "and", SearchToWhere(dm.Search)));

            data = data.ToList(); // run the search before sorting and counting
            if (dm.Sorted != null && dm.Sorted.Count > 0)
            {
                var sortCrit = ToSortCriteria(dm.Sorted.Reverse<Sort>()); // multi-column sort comes in reverse order
                ((List<DataRow>)data).Sort(sortCrit);
            }

            int count = data.Count();
            if (dm.Skip != 0)
                data = data.Skip(dm.Skip);
            if (dm.Take != 0)
                data = data.Take(dm.Take);

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
            return await Task.FromResult(res);
        }

        private bool Matches(DataRow row, string condition, IEnumerable<WhereFilter> criteria)
        {
            bool isOr = condition == "or";
            foreach (var c in criteria)
            {
                bool m;
                if (c.IsComplex)
                    m = Matches(row, c.Condition, c.predicates);
                else
                {
                    var prop = row.List[c.Field];
                    if (prop == null) continue;
                    var op = GetOperator(c.Operator);
                    m = row.List.PropertyValueMatches(prop, row, op, c.value, !c.IgnoreCase);
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
            => new ListSortCriteria(sort.Select(s => new ListSortField()
            {
                PropertyName = s.Name,
                SortDirection = ListSortDirection.FromString(s.Direction)
            }));

        /// <inheritdoc/>
        public async override Task<object> InsertAsync(DataManager dataManager, object data, string key)
        {
            var list = DataManager?.List;
            if (list != null && data is DataRow row)
                await list.InsertAsync(0, row);
            return data;
        }

        /// <inheritdoc/>
        public async override Task<object> UpdateAsync(DataManager dataManager, object data, string keyField, string key)
        {
            var list = DataManager?.List;
            if (list != null && data is DataRow row)
                await list.UpdateRow(row.OriginalRow, row);
            return data;
        }

        /// <inheritdoc/>
        public async override Task<object> RemoveAsync(DataManager dataManager, object data, string keyField, string key)
        {
            var list = DataManager?.List;
            var keyProp = keyField == null ? null : list?[keyField];
            var rowsToRemove = keyProp == null ? new List<DataRow>() :
                list.GetData().Where(r => keyProp.GetValue(ValueFormat.Internal, r) == data).ToList();
            if (!rowsToRemove.Any())
                rowsToRemove = DataManager?.SelectedRows;
            await list.RemoveRows(rowsToRemove);
            return rowsToRemove;
        }

        /// <inheritdoc/>
        public async override Task<object> BatchUpdateAsync(DataManager dataManager, object changedRecords,
            object addedRecords, object deletedRecords, string keyField, string key, int? dropIndex)
        {
            if (deletedRecords is IEnumerable<DataRow> rowsToRemove)
                await DataManager?.List?.RemoveRows(rowsToRemove);
            
            return await base.BatchUpdateAsync(dataManager, changedRecords, addedRecords, deletedRecords, keyField, key, dropIndex);
        }
    }
}