// Copyright (c) 2022 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xomega.Framework;
using Xomega.Framework.Properties;

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

        /// <inheritdoc/>
        protected override async Task OnParametersSetAsync()
        {
            base.OnParametersSet();
            var prop = Property;
            if (prop == null) return;

            if (prop is IntegerProperty || prop is DecimalProperty || prop is DateTimeProperty)
                TextAlign = TextAlign.Right;

            if (Template == null)
                Template = RenderRowValue;

            if (prop is EnumProperty)
            {
                await GetFilterItems();
                if (FilterTemplate == null)
                    FilterTemplate = RenderFilterDropDown;
                if (FilterItemTemplate == null)
                    FilterItemTemplate = RenderValue;
                if (EditTemplate == null)
                    EditTemplate = RenderEditDropDown;
            }

            if (prop is TextProperty || prop is IntegerProperty || prop is DecimalProperty)
            {
                if (EditTemplate == null)
                    EditTemplate = RenderEditor<Controls.XSfTextBox>;
            }

            if (prop is DateTimeProperty)
            {
                if (EditTemplate == null)
                    EditTemplate = RenderEditor<Controls.XSfDatePicker>;
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
        }

        private void OnPropertyChange(object sender, PropertyChangeEventArgs e)
        {
            InvokeAsync(async () => {
                if (e.Change.IncludesVisible() && MainParent is XSfGrid grid)
                {
                    if (Property.Visible) await grid.Show(new string[] { HeaderText });
                    else await grid.Hide(new string[] { HeaderText });
                }
            });
        }

        /// <summary>
        /// Disposes the class and unsubscribes from property change events.
        /// </summary>
        public override void Dispose()
        {
            var prop = Property;
            if (prop != null)
                prop.Change -= OnPropertyChange;
            base.Dispose();
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

        private List<object> FilterItems;

        /// <summary>
        /// Default renderer of a single value for this column based on the property's DisplayString format.        /// 
        /// </summary>
        /// <param name="val">The value being rendered.</param>
        /// <returns>The render fragment for the value.</returns>
        protected RenderFragment RenderValue(object val) => (b) =>
        {
            if (val is FilterItemTemplateContext ctx)
                val = ctx.Value;
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
                cb.OpenComponent<DropDownListEvents<object, object>>(1);
                cb.AddAttribute(2, "ValueChange", EventCallback.Factory.Create<ChangeEventArgs<object, object>>(this, OnFilterChanged));
                cb.CloseComponent();
            };

            b.OpenComponent<SfDropDownList<object, object>>(1);
            b.AddAttribute(2, "ChildContent", childContent);
            b.AddAttribute(3, "PlaceHolder", FilterAllText);
            b.AddAttribute(4, "DataSource", FilterItems);
            b.AddAttribute(5, "Value", (ctx as PredicateModel)?.Value);
            b.AddAttribute(6, "ItemTemplate", FilterItemTemplate);
            b.CloseComponent();
        };

        private async Task GetFilterItems()
        {
            FilterItems = new List<object>();
            var prop = Property;
            if (prop != null)
            {
                FilterItems.Add(FilterAllText);
                FilterItems.AddRange((IEnumerable<object>) await prop.AsyncItemsProvider(null, null, default));
            }
        }

        private void OnFilterChanged(ChangeEventArgs<object, object> args)
        {
            var grid = MainParent as XSfGrid;
            if (FilterAllText != null && FilterAllText.Equals(args.ItemData))
                grid.ClearFiltering(Field);
            else if (args.ItemData != null)
                grid.FilterByColumn(Field, "equal", args.ItemData);
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

            b.OpenComponent<Controls.XSfDropDownList>(1);
            b.AddAttribute(3, "ID", Property?.Name);
            b.AddAttribute(4, "Property", Property);
            b.AddAttribute(5, "AllowFiltering", editorParams?.AllowFiltering ?? false);
            b.CloseComponent();
        });

        #endregion
    }
}
