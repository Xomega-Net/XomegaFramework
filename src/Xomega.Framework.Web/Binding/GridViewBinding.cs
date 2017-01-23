// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System.Collections.Specialized;
using System.Web.UI.WebControls;
using System;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// An interface that allows binding controls to data list objects 
    /// </summary>
    public interface IListBindable
    {
        /// <summary>
        /// Binds a control to the specified data list object.
        /// </summary>
        /// <param name="list">The data object lists to bind to.</param>
        void BindTo(DataListObject list);
    }

    /// <summary>
    /// A class that helps binding a <see cref="GridView"/> control to data list objects.
    /// It extends the <see cref="WebPropertyBinding"/> to leverage the binding registration
    /// mechanism for Web controls.
    /// </summary>
    public class GridViewBinding : WebPropertyBinding, IListBindable
    {
        /// <summary>
        ///  A static method to register the grid view binding for the given GridView web control.
        /// </summary>
        public static void Register()
        {
            Register(typeof(GridView), delegate(object obj)
            {
                GridView grid = obj as GridView;
                if (grid != null)
                {
                    grid.DataBind();
                    return new GridViewBinding(grid);
                }
                else return null;
            });
        }

        /// <summary>
        /// Constructs a new grid view binding for the given GridView web control.
        /// Sets up grid listeners to handle the data binding.
        /// </summary>
        /// <param name="grid">The GridView control to be bound to the data list object.</param>
        protected GridViewBinding(GridView grid)
            : base(grid)
        {
            grid.RowDataBound += delegate(object sender, GridViewRowEventArgs e)
            {
                DataRow dataRow = e.Row.DataItem as DataRow;
                if (dataRow == null) return;
                dataRow.List.CurrentRow = e.Row.DataItemIndex;
                if (dataRow.Selected) e.Row.ApplyStyle(grid.SelectedRowStyle);
                BindToObject(e.Row, dataRow.List, true);
            };
            grid.PageIndexChanging += delegate(object sender, GridViewPageEventArgs e)
            {
                if (e.Cancel) return;
                grid.PageIndex = e.NewPageIndex;
                grid.DataBind();
            };
            grid.Sorting += delegate(object s, GridViewSortEventArgs e)
            {
                if (e.Cancel) return;
                if (list.SortCriteria != null && list.SortCriteria.Count == 1 &&
                    list.SortCriteria[0].PropertyName == e.SortExpression)
                {
                    list.SortCriteria[0].SortDirection = ListSortDirection.Toggle(list.SortCriteria[0].SortDirection);
                }
                else
                {
                    list.SortCriteria = new ListSortCriteria();
                    ListSortField sortFld = new ListSortField();
                    sortFld.PropertyName = e.SortExpression;
                    sortFld.SortDirection = e.SortDirection == SortDirection.Ascending ? 
                        ListSortDirection.Ascending : ListSortDirection.Descending;
                    list.SortCriteria.Add(sortFld);
                }
                list.Sort();
            };
            grid.RowEditing += delegate(object sender, GridViewEditEventArgs e)
            {
                if (e.Cancel || list == null) return;
                grid.EditIndex = e.NewEditIndex;
                int idx = grid.PageSize * grid.PageIndex + e.NewEditIndex;
                list.EditRow = new DataRow(list);
                list.EditRow.CopyFrom(list.GetData()[idx]);
                grid.DataBind();
            };
            grid.RowUpdating += delegate(object sender, GridViewUpdateEventArgs e)
            {
                if (e.Cancel || list == null) return;
                int idx = grid.PageSize * grid.PageIndex + e.RowIndex;
                grid.EditIndex = -1;
                list.EditRow = null;
                grid.DataBind();
            };
            grid.RowCancelingEdit += delegate(object sender, GridViewCancelEditEventArgs e)
            {
                if (e.Cancel || list == null) return;
                grid.EditIndex = -1;
                int idx = grid.PageSize * grid.PageIndex + e.RowIndex;
                if (list.EditRow != null)
                {
                    list.GetData()[idx].CopyFrom(list.EditRow);
                    list.EditRow = null;
                    grid.DataBind(); // stop editing existing object
                }
                else list.RemoveAt(idx); // delete new object on cancel
            };
            grid.RowDeleting += delegate(object sender, GridViewDeleteEventArgs e)
            {
                if (e.Cancel) return;
                int idx = grid.PageSize * grid.PageIndex + e.RowIndex;
                if (list != null) list.RemoveAt(idx);
            };
            grid.RowCommand += delegate(object sender, GridViewCommandEventArgs e)
            {
                if (list == null || e.CommandName != "New") return;
                DataRow newRow = new DataRow(list);
                int index = 0;
                if (Int32.TryParse("" + e.CommandArgument, out index)) index++;
                grid.EditIndex = index;
                list.Insert(grid.PageSize * grid.PageIndex + index, newRow);
            };
            grid.SelectedIndexChanging += delegate(object sender, GridViewSelectEventArgs e)
            {
                if (e.Cancel || list == null) return;
                int idx = grid.PageSize * grid.PageIndex + e.NewSelectedIndex;
                if (list.SelectRow(idx)) list.FireCollectionChanged();
                // cancel, since we don't really want the grid to set SelectedIndex, which doesn't work for with paging
                e.Cancel = true;
            };
            // Defer data binding to the Load method after the view state is loaded.
            // Otherwise the view state will get corrupted.
            grid.Load += delegate
            {
                grid.DataBind();
            };
            grid.Unload += delegate
            {
                BindTo(null);
            };
        }

        /// <summary>
        /// The data list object that the grid view is bound to.
        /// </summary>
        protected DataListObject list;

        /// <summary>
        /// Binds the current GridView control to the specified data list object.
        /// Subscribes to listen for changes in the data list and update the grid accordingly.
        /// </summary>
        /// <param name="list">The data list object to bind to.</param>
        public virtual void BindTo(DataListObject list)
        {
            INotifyCollectionChanged observableList = this.list as INotifyCollectionChanged;
            if (observableList != null) observableList.CollectionChanged -= OnListChanged;
            if (this.list != null) this.list.SelectionChanged -= OnListSelectionChanged;
            this.list = list;
            observableList = list as INotifyCollectionChanged;
            if (observableList != null)
            {
                OnListChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                observableList.CollectionChanged += OnListChanged;
            }
            if (list != null) list.SelectionChanged += OnListSelectionChanged;
        }

        /// <summary>
        /// Handles the selection change event of the data list object to update the grid control.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        protected void OnListSelectionChanged(object sender, EventArgs e)
        {
            GridView grid = (GridView)control;
            grid.DataSource = list != null ? list.GetData() : null;
            grid.DataBind();
        }

        /// <summary>
        /// Handles the collection change event of the data list object to update the grid control.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        protected void OnListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnListSelectionChanged(sender, e);
        }
    }
}
