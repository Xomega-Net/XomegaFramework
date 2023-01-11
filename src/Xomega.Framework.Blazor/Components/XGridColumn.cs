// Copyright (c) 2023 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Xomega.Framework.Properties;

namespace Xomega.Framework.Blazor.Components
{
    /// <summary>
    /// Alignment enumeration.
    /// </summary>
    public enum Alignment
    {
        /// <summary>
        /// Alignment at the start (normally left).
        /// </summary>
        Start,

        /// <summary>
        /// Alignment at the center.
        /// </summary>
        Center,
        
        /// <summary>
        /// Alignment at the end (normally right).
        /// </summary>
        End
    }

    /// <summary>
    /// Specification of a grid column that can be bound to a data property.
    /// </summary>
    public class XGridColumn : ComponentBase
    {
        /// <summary>
        /// Cascading parameter for the parent grid.
        /// </summary>
        [CascadingParameter] protected XGrid ParentGrid { get; set; }

        /// <summary>
        /// Data property for the column.
        /// </summary>
        [Parameter] public DataProperty Property { get; set; }

        /// <summary>
        /// Custom header text to use for the column instead of the property label.
        /// </summary>
        [Parameter] public string HeaderText { get; set; }

        /// <summary>
        /// Gets the text for the column header.
        /// </summary>
        /// <returns>The text for the column header.</returns>
        public string GetHeaderText() => HeaderText ?? Property?.Label;

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            if (ParentGrid != null)
                ParentGrid.AddColumn(this);

            if (TextAlign == null && (DecimalProperty.IsPropertyNumeric(Property) || Property is DateTimeProperty))
                TextAlign = Alignment.End;

            base.OnInitialized();
        }

        /// <summary>
        /// Custom display cell template for the column.
        /// </summary>
        [Parameter] public RenderFragment<DataRow> Template { get; set; }

        /// <summary>
        /// The custom or default display cell template to use for the column.
        /// </summary>
        public RenderFragment<DataRow> DisplayTemplate => Template ?? RenderRowValue;

        /// <summary>
        /// The default cell template for the column that formats property value as using <see cref="ValueFormat.DisplayString"/> format.
        /// </summary>
        /// <param name="row">The row to render</param>
        /// <returns>The resulting render fragment.</returns>
        protected RenderFragment RenderRowValue(DataRow row) => (b) =>
        {
            var prop = Property;
            if (prop != null)
                b.AddContent(1, prop.GetStringValue(ValueFormat.DisplayString, row));
        };

        /// <summary>
        /// Returns whether or not the grid column is currently visible based on the visibility of the underlying property.
        /// </summary>
        public bool IsVisible => Property?.Visible ?? true;

        /// <summary>
        /// The width of the column with CSS units (px, rem, %, etc).
        /// </summary>
        [Parameter] public string Width { get; set; }

        /// <summary>
        /// Text alignment to use for the column's header and row cells.
        /// </summary>
        [Parameter] public Alignment? TextAlign { get; set; }

        /// <summary>
        /// Returns the Bootstrap class for the specified text alignment.
        /// </summary>
        public string AlignmentClass => TextAlign == Alignment.End ? "text-end" :
            TextAlign == Alignment.Center ? "text-center" : ""; // default is text-start

        /// <summary>
        /// Whether or not to allow wrapping content in the column cells.
        /// </summary>
        [Parameter] public bool AllowWrap { get; set; }

        /// <summary>
        /// Returns the Bootstrap class for wrapping cell text as needed.
        /// </summary>
        public string WrapClass => AllowWrap ? "" : "text-nowrap text-truncate";

        /// <summary>
        /// Gets the CSS style string for the column header, primarily to specify the width of the column.
        /// </summary>
        /// <returns>The CSS style string for the column header.</returns>
        public string GetHeaderStyle()
        {
            var style = new List<string>();
            if (!string.IsNullOrEmpty(Width))
                style.Add($"width: {Width}");
            return string.Join(';', style);
        }

        #region Sorting

        /// <summary>
        /// Specifies whether or not the current column allows sorting.
        /// </summary>
        [Parameter] public bool Sortable { get; set; } = true;

        /// <summary>
        /// The class used to display an icon for the ascending sort order.
        /// </summary>
        [Parameter] public string AscendingClass { get; set; } = "bi-arrow-up";

        /// <summary>
        /// The class used to display an icon for the descending sort order.
        /// </summary>
        [Parameter] public string DescendingClass { get; set; } = "bi-arrow-down";

        /// <summary>
        /// Convenient accessor of the underlying data list object.
        /// </summary>
        protected DataListObject List => Property?.GetParent() as DataListObject;

        internal bool IsSortable => Sortable && (ParentGrid?.AllowSorting ?? false) && Property != null;
        
        internal ListSortField SortField => Property == null ? null :
            SortCriteria?.Find(sf => sf.PropertyName == Property.Name);

        internal int SortIndex => Property == null ? -1 :
            SortCriteria?.FindIndex(sf => sf.PropertyName == Property.Name) + 1 ?? -1;

        internal ListSortCriteria SortCriteria => List?.SortCriteria;

        internal string GetSortClass()
        {
            var sf = SortField;
            if (sf == null) return "";
            if (sf.SortDirection == ListSortDirection.Ascending)
                return AscendingClass;
            else if (sf.SortDirection == ListSortDirection.Descending)
                return DescendingClass;
            return "";
        }

        internal void OnHeaderClicked(MouseEventArgs e)
        {
            if (!IsSortable) return;

            var list = List;
            var sc = SortCriteria;
            var curSf = SortField;
            var newSf = new ListSortField()
            {
                PropertyName = Property.Name,
                SortDirection = ListSortDirection.Ascending
            };

            if (e.CtrlKey && curSf != null) sc.Remove(curSf);
            else if (e.CtrlKey && sc != null) sc.Add(newSf);
            else if (curSf != null)
                curSf.SortDirection = ListSortDirection.Toggle(curSf.SortDirection);
            else if (list != null)
                list.SortCriteria = new ListSortCriteria() { newSf };
            if (list != null) list.Sort();
        }

        #endregion
    }
}