// Copyright (c) 2023 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Syncfusion.Blazor.Grids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xomega.Framework;
using Action = Syncfusion.Blazor.Grids.Action;

namespace Xomega._Syncfusion.Blazor
{
    /// <summary>
    /// Enhancement of the standard Syncfusion grid events to support editing and validation of data rows in the list object,
    /// as well as to synchronize grid selection with the list object selection, as configured.
    /// </summary>
    public class XSfGridEvents : GridEvents<DataRow>
    {
        private DataListObject List => (Parent as XSfGrid)?.List;
        
        private bool initialized = false;

        /// <inheritdoc/>
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (initialized)
            {
                // reset event handlers to built-in callbacks
                OnActionBegin = EventCallback.Factory.Create<ActionEventArgs<DataRow>>(this, InvokeOnActionBegin);
                OnActionComplete = EventCallback.Factory.Create<ActionEventArgs<DataRow>>(this, InvokeOnActionComplete);
                RowSelected = EventCallback.Factory.Create<RowSelectEventArgs<DataRow>>(this, OnRowSelected);
                RowDeselected = EventCallback.Factory.Create<RowDeselectEventArgs<DataRow>>(this, OnRowDeselected);
            }
            else // Store custom callbacks to invoke after built-in callbacks.
            {

                if (OnActionBegin.HasDelegate)
                    customActionBegin = OnActionBegin;
                OnActionBegin = EventCallback.Factory.Create<ActionEventArgs<DataRow>>(this, InvokeOnActionBegin);

                if (OnActionComplete.HasDelegate)
                    customActionComplete = OnActionComplete;
                OnActionComplete = EventCallback.Factory.Create<ActionEventArgs<DataRow>>(this, InvokeOnActionComplete);

                if (RowSelected.HasDelegate)
                    customRowSelected = RowSelected;
                RowSelected = EventCallback.Factory.Create<RowSelectEventArgs<DataRow>>(this, OnRowSelected);

                if (RowDeselected.HasDelegate)
                    customRowDeselected = RowDeselected;
                RowDeselected = EventCallback.Factory.Create<RowDeselectEventArgs<DataRow>>(this, OnRowDeselected);

                var list = (Parent as XSfGrid)?.List;
                if (list != null)
                    list.SelectionChanged += OnListSelectionChanged;

                initialized = true; // ensure events are initialized only once
            }
        }

        #region Selection support

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            var list = List;
            if (list != null)
                list.SelectionChanged -= OnListSelectionChanged;
            base.Dispose(disposing);
        }

        private bool suppressGridSelectionUpdate = false;
        private bool suppressListSelectionUpdate = false;

        private bool UseListSelection => customRowSelected.HasDelegate // custom selection handler must update list selection
            || ((Parent.SelectionSettings as XSfGridSelectionSettings)?.UseListSelection ?? false);

        private async void OnListSelectionChanged(object sender, EventArgs e) => await UpdateGridSelection();

        private async Task UpdateGridSelection()
        {
            if (Parent is XSfGrid grid && !suppressGridSelectionUpdate)
            {
                suppressListSelectionUpdate = true;
                suppressGridSelectionUpdate = true; // to avoid recursion
                var selIndexes = grid.GetSelectedDataRowIndexes();
                if (selIndexes != null)
                    await grid.SelectRowsAsync(selIndexes.ToArray());
                suppressListSelectionUpdate = false;
                suppressGridSelectionUpdate = false;
            }
        }

        private void UpdateListSelection()
        {
            var grid = Parent as XSfGrid;
            if (grid?.List != null && !suppressListSelectionUpdate)
            {
                suppressGridSelectionUpdate = true;
                grid.List.SelectedRows = grid.SelectedRecords;
                suppressGridSelectionUpdate = false;
            }
        }

        private EventCallback<RowSelectEventArgs<DataRow>> customRowSelected;
        private async Task OnRowSelected(RowSelectEventArgs<DataRow> args)
        {
            if (!UseListSelection)
            {
                UpdateListSelection();
            }
            else if (!suppressGridSelectionUpdate) // avoid double execution in recursion
            {
                await customRowSelected.InvokeAsync(args);
                // update grid selection to match the list selection at the end of user row selection
                await UpdateGridSelection();
            }
        }

        private EventCallback<RowDeselectEventArgs<DataRow>> customRowDeselected;
        private async Task OnRowDeselected(RowDeselectEventArgs<DataRow> args)
        {
            if (!customRowSelected.HasDelegate && !UseListSelection)
                UpdateListSelection();
            else await customRowDeselected.InvokeAsync(args);
        }

        #endregion

        #region Validation support

        private readonly Dictionary<EditContext, ValidationMessageStore> validationStores = new Dictionary<EditContext, ValidationMessageStore>();

        private EventCallback<ActionEventArgs<DataRow>> customActionBegin;
        
        private EventCallback<ActionEventArgs<DataRow>> customActionComplete;

        private async Task InvokeOnActionBegin(ActionEventArgs<DataRow> args)
        {
            if (args.RequestType == Action.Add || args.RequestType == Action.BeginEdit)
            {
                if (args.RequestType == Action.BeginEdit)
                {
                    args.Data = new DataRow(args.RowData);
                    await args.Data.CopyFromAsync(args.RowData);
                }
                else args.Data = new DataRow(List);
                args.EditContext = new EditContext(args.Data);
                validationStores[args.EditContext] = new ValidationMessageStore(args.EditContext);
                args.EditContext.OnValidationRequested += OnValidationRequested;
                args.EditContext.OnFieldChanged += OnFieldChanged;
            }
            if (args.RequestType == Action.Save || args.RequestType == Action.Cancel)
            {
                var ectx = validationStores.Keys.Where(k => k.Model == args.Data).FirstOrDefault();
                if (ectx != null) validationStores.Remove(ectx);
            }
            if (customActionBegin.HasDelegate)
                await customActionBegin.InvokeAsync(args);
        }

        private async Task InvokeOnActionComplete(ActionEventArgs<DataRow> args)
        {
            if (args.RequestType == Action.Save || args.RequestType == Action.Delete || args.RequestType == Action.BatchSave)
            {
                // deferred setting of modified flag to make sure resulting UI updates happen during OnComplete,
                // since SfGrid cannot handle those while the data source is being updated.
                List?.SetModified(true, false);
            }
            if (customActionComplete.HasDelegate)
                await customActionComplete.InvokeAsync(args);
        }

        private void OnValidationRequested(object sender, ValidationRequestedEventArgs e)
        {
            var ectx = sender as EditContext;
            var list = List;
            if (!validationStores.TryGetValue(ectx, out ValidationMessageStore vstore) || list == null) return;
            vstore.Clear();
            foreach (DataProperty p in list.Properties)
                ValidateProperty(ectx, p);
        }

        private void OnFieldChanged(object sender, FieldChangedEventArgs e)
        {
            var ectx = sender as EditContext;
            var list = List;
            if (!validationStores.TryGetValue(ectx, out ValidationMessageStore vstore) || list == null) return;
            vstore.Clear(e.FieldIdentifier);
            ValidateProperty(ectx, list[e.FieldIdentifier.FieldName]);
        }

        private void ValidateProperty(EditContext ectx, DataProperty p)
        {
            var vstore = validationStores[ectx];
            var row = ectx.Model as DataRow;
            p.Validate(row);
            var errors = p.GetValidationErrors(row);
            if (errors?.Errors != null)
            {
                var fld = ectx.Field(p.Name);
                foreach (var error in errors.Errors)
                    vstore.Add(fld, error.Message);
            }
        }

        #endregion
    }
}