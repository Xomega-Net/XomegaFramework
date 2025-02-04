// Copyright (c) 2025 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xomega._Syncfusion.Blazor.Controls;
using Xomega.Framework;
using Xomega.Framework.Blazor.Controls;
using Xomega.Framework.Properties;
using FilterType = Syncfusion.Blazor.Grids.FilterType;

namespace Xomega._Syncfusion.Blazor
{
    /// <summary>
    /// A column in a Syncfusion grid that is bound to a data property via its Field member.
    /// </summary>
    public class XSfGridColumn : GridColumn
    {
        /// <summary>
        /// Returns the property bound to this grid column.
        /// </summary>
        protected DataProperty Property => (MainParent as XSfGrid)?.List?[Field];

        /// <summary>
        ///  Flag tracking if the grid column is already initialized
        /// </summary>
        protected bool initialized = false;

        /// <inheritdoc/>
        protected override async Task OnParametersSetAsync()
        {
            base.OnParametersSet();
            var prop = Property;
            var grid = MainParent as XSfGrid;
            if (prop == null || initialized || grid == null) return;

            if (DecimalProperty.IsPropertyNumeric(prop) || prop is DateTimeProperty)
                TextAlign = TextAlign.Right;

            if (Template == null)
                Template = RenderRowValue;

            if (AllowEditing && !prop.Editable)
                AllowEditing = false;

            if (prop is EnumProperty)
            {
                await GetFilterItems();
                if (FilterTemplate == null)
                    FilterTemplate = RenderFilterDropDown;
                
                if (FilterSettings == null)
                    FilterSettings = new FilterSettings() { Operator = Operator.Equal };
                else if (FilterSettings.Operator == null)
                    FilterSettings.Operator = Operator.Equal;

                if (EditTemplate == null && AllowEditing)
                    EditTemplate = RenderEditDropDown;
            }

            if (DecimalProperty.IsPropertyNumeric(prop) || prop is TextProperty)
            {
                if (EditTemplate == null && AllowEditing)
                    EditTemplate = RenderEditor<XSfTextBox>;
            }

            if (prop is DateTimeProperty)
            {
                if (EditTemplate == null && AllowEditing)
                    EditTemplate = RenderEditor<XSfDatePicker>;
            }

            if (EditTemplate == null && !AllowEditing)
            {
                if (grid.EditSettings.Mode == EditMode.Dialog)
                    EditTemplate = RenderEditor<XSfDataLabel>;
                else EditTemplate = RenderEditor<XDataText>;
            }

            if (string.IsNullOrEmpty(Format))
            {
                if (prop is DecimalProperty dp)
                    Format = dp.DisplayFormat;
                if (prop is DateTimeProperty dtp)
                {
                    Format = dtp.Format;
                    Type = ColumnType.Date;
                }
            }
            if (prop.IsKey) IsPrimaryKey = true;
            if (string.IsNullOrEmpty(HeaderText) || HeaderText == Field)
                HeaderText = prop.Label;
            else prop.Label = HeaderText;

            prop.Change += OnPropertyChange;
            initialized = true;
        }

        private void OnPropertyChange(object sender, PropertyChangeEventArgs e)
        {
            InvokeAsync(async () =>
            {
                if (e.Change.IncludesVisible() && MainParent is XSfGrid grid)
                {
                    if (Property.Visible) await grid.ShowColumnAsync(HeaderText);
                    else await grid.HideColumnAsync(HeaderText);
                }
            });
        }

        /// <summary>
        /// Disposes the class and unsubscribes from property change events.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            var prop = Property;
            if (prop != null)
                prop.Change -= OnPropertyChange;
            base.Dispose(disposing);
        }

        /// <summary>
        /// Default renderer of a row's value for this column based on the property's DisplayString format.
        /// </summary>
        /// <param name="row">The DataRow being rendered</param>
        /// <returns>The render fragment for the row's value.</returns>
        protected RenderFragment RenderRowValue(object row) => (b) =>
        {
            var prop = Property;
            if (prop != null)
                b.AddContent(1, prop.GetStringValue(ValueFormat.DisplayString, row as DataRow));
        };

        #region Filtering

        /// <summary>
        /// The value to use for a filter dropdown to apply no filter on the column (show all row values).
        /// </summary>
        [Parameter] public string FilterAllText { get; set; } = "All";

        private List<Header> FilterItems;

        private bool IsFilterBarType => FilterSettings?.Type == FilterType.FilterBar ||
            FilterSettings?.Type == null && (MainParent as XSfGrid)?.FilterSettings?.Type == FilterType.FilterBar;

        /// <summary>
        /// Default renderer of a single value for this column based on the property's DisplayString format.        /// 
        /// </summary>
        /// <param name="val">The value being rendered.</param>
        /// <returns>The render fragment for the value.</returns>
        protected RenderFragment RenderValue(Header val) => (b) =>
        {
            var prop = Property;
            if (prop != null)
                b.AddContent(1, prop.ValueToString(val, ValueFormat.DisplayString));
        };

        /// <summary>
        /// A renderer for column's filter dropdown.
        /// </summary>
        /// <param name="ctx">Current filter value as a PredicateModel</param>
        /// <returns>The render fragment for the filter dropdown.</returns>
        protected RenderFragment RenderFilterDropDown(object ctx) => (b) =>
        {
            RenderFragment childContent = (cb) =>
            {
                cb.OpenComponent<DropDownListEvents<string, Header>>(1);
                cb.AddAttribute(2, "ValueChange", EventCallback.Factory.Create<ChangeEventArgs<string, Header>>(this,
                    async args => await OnFilterChangedAsync(args, ctx)));
                cb.CloseComponent();
                cb.OpenComponent<DropDownListFieldSettings>(1);
                cb.AddAttribute(2, "Text", "Text");
                cb.AddAttribute(3, "Value", "Id");
                cb.CloseComponent();
            };

            var grid = MainParent as XSfGrid;
            b.OpenComponent<SfDropDownList<string, Header>>(1);
            b.AddAttribute(2, "ChildContent", childContent);
            b.AddAttribute(3, "PlaceHolder", FilterAllText);
            b.AddAttribute(4, "DataSource", FilterItems);
            var value = (ctx as PredicateModel)?.Value ?? (ctx as PredicateModel<Header>)?.Value?.Id;
            b.AddAttribute(5, "Value", value);
            RenderFragment<Header> itemTemplate = RenderValue;
            b.AddAttribute(6, "ItemTemplate", itemTemplate);
            b.CloseComponent();
        };

        private async Task GetFilterItems()
        {
            FilterItems = new List<Header>();
            var prop = Property;
            if (prop != null)
            {
                if (IsFilterBarType)
                    FilterItems.Add(new Header("", "", FilterAllText));
                FilterItems.AddRange((IEnumerable<Header>) await prop.AsyncItemsProvider(null, null, default));
            }
        }

        private async Task OnFilterChangedAsync(ChangeEventArgs<string, Header> args, object ctx)
        {
            if (ctx is PredicateModel pm) pm.Value = args.ItemData;
            else if (ctx is PredicateModel<Header> pmh) pmh.Value = args.ItemData;

            if (args.ItemData == null || !IsFilterBarType) return;

            // auto-apply for FilterBar type only
            var grid = MainParent as XSfGrid;
            if (string.IsNullOrEmpty(args.ItemData.Id))
                await grid.ClearFilteringAsync(Field);
            else await grid.FilterByColumnAsync(Field, "equal", args.ItemData.Id);
        }

        #endregion

        #region Editing

        /// <summary>
        /// A template for rendering a column's cell editor that wraps it in a cascading value of the current row.
        /// </summary>
        /// <param name="ctx">The current DataRow being rendered.</param>
        /// <param name="editor">The editor to render.</param>
        /// <returns>Render fragment for the editor wrapped into the row's cascading value.</returns>
        protected RenderFragment RenderEditor(object ctx, RenderFragment editor) => (b) =>
        {
            b.OpenComponent<CascadingValue<DataRow>>(1);
            b.AddAttribute(2, "Value", (ctx as DataRow));
            b.AddAttribute(3, "ChildContent", editor);
            b.CloseComponent();
        };

        /// <summary>
        /// A template for rendering an editor of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the editor component.</typeparam>
        /// <param name="ctx">The current DataRow.</param>
        /// <returns>Render fragment for the editor of the specified type.</returns>
        protected RenderFragment RenderEditor<T>(object ctx) where T : IComponent
            => RenderEditor(ctx, (b) =>
            {
                b.OpenComponent<T>(1);
                b.AddAttribute(2, "ID", Property?.Name);
                b.AddAttribute(3, "Property", Property);
                b.CloseComponent();
            });

        /// <summary>
        /// A template for rendering a dropdown editor that uses some editor settings,
        /// such as AllowFiltering flag.
        /// </summary>
        /// <param name="ctx">The current DataRow.</param>
        /// <returns>Render fragment for the dropdown editor for the column.</returns>
        protected RenderFragment RenderEditDropDown(object ctx) => RenderEditor(ctx, (b) =>
        {
            var editorParams = (EditorSettings as DropDownEditCellParams)?.Params;

            b.OpenComponent<XSfDropDownList>(1);
            b.AddAttribute(3, "ID", Property?.Name);
            b.AddAttribute(4, "Property", Property);
            b.AddAttribute(5, "AllowFiltering", editorParams?.AllowFiltering ?? false);
            b.CloseComponent();
        });

        #endregion
    }
}
