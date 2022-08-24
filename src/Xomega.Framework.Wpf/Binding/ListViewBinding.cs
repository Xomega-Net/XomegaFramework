// Copyright (c) 2022 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Controls;

namespace Xomega.Framework.Binding
{
    /// <summary>
    /// A class that provides binding for a ListView to the corresponding data list object.
    /// </summary>
    public class ListViewBinding : DataObjectBinding
    {
        /// <summary>
        ///  A static method to register list view context binding for ListView WPF element.
        /// </summary>
        public static void Register()
        {
            Register(typeof(ListView), delegate(object obj)
            {
                ListView listView = obj as ListView;
                return IsBindable(listView) ? new ListViewBinding(listView) : null;
            });
        }

        /// <summary>
        /// Constructs a new selector property binding for the given Selector element.
        /// </summary>
        /// <param name="listView">The selector element to be bound to the data property.</param>
        protected ListViewBinding(ListView listView) : base(listView)
        {
            listView.SelectionChanged += OnListViewSelectionChanged;
        }

        private void CleanupBindings(object sender, NotifyCollectionChangedEventArgs e)
        {
            ListView lv = element as ListView;
            for (int i=0; i < lv.Items.Count; i++)
                DataPropertyBinding.RemoveBindings(lv.ItemContainerGenerator.ContainerFromIndex(i));
        }

        /// <summary>
        /// Binds the framework element to the given data object.
        /// </summary>
        /// <param name="obj">The data object to bind the framework element to.</param>
        public override void BindTo(DataObject obj)
        {
            ListView lv = (ListView)element;
            if (obj is DataListObject lst)
            {
                lst.SelectionChanged -= OnListObjectSelectionChanged;
            }
            base.BindTo(obj); // assigns context
            OnListObjectSelectionChanged(context, EventArgs.Empty); // update current selection
            lst = obj as DataListObject;
            if (lst != null)
            {
                lst.SelectionChanged += OnListObjectSelectionChanged;
            }
            else CleanupBindings(element, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            if (lv.ItemsSource is INotifyCollectionChanged oldSrc)
                oldSrc.CollectionChanged -= CleanupBindings;
            lv.ItemsSource = lst?.GetData();
            if (lv.ItemsSource is INotifyCollectionChanged newSrc)
                newSrc.CollectionChanged += CleanupBindings;
        }

        /// <summary>
        /// Remove any listeners when disposing
        /// </summary>
        public override void Dispose()
        {
            ListView listView = (ListView)element;
            listView.SelectionChanged -= OnListViewSelectionChanged;
            base.Dispose();
        }

        /// <summary>
        /// Handles selection update from the data list object by updating the list view selection
        /// </summary>
        private void OnListObjectSelectionChanged(object sender, EventArgs e)
        {
            ListView listView = element as ListView;
            if (PreventElementUpdate || !(sender is DataListObject listObj)) return;

            PreventModelUpdate = true;

            // update selection model for list view as needed
            if (DataListObject.SelectionModeSingle.Equals(listObj.RowSelectionMode))
                listView.SelectionMode = SelectionMode.Single;
            else if (DataListObject.SelectionModeMultiple.Equals(listObj.RowSelectionMode) &&
                     listView.SelectionMode == SelectionMode.Single)
                listView.SelectionMode = SelectionMode.Extended;

            // clear selection
            if (listView.SelectionMode == SelectionMode.Single)
                listView.SelectedItem = null;
            else listView.SelectedItems.Clear();

            // set selected item(s) from the list object
            foreach (DataRow r in listView.Items)
            {
                if (r.Selected)
                {
                    if (listView.SelectionMode == SelectionMode.Single)
                    {
                        listView.SelectedItem = r;
                        break;
                    }
                    else listView.SelectedItems.Add(r);
                }
            }
            PreventModelUpdate = false;
        }

        /// <summary>
        /// Handles selection update from the list view by updating selection in the data list object
        /// </summary>
        private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = element as ListView;
            if (PreventModelUpdate || !(context is DataListObject listObj)) return;

            PreventElementUpdate = true;
            var selectedRows = new List<DataRow>();
            foreach (DataRow r in listView.SelectedItems)
                selectedRows.Add(r);
            listObj.SelectedRows = selectedRows;
            PreventElementUpdate = false;
        }
    }
}