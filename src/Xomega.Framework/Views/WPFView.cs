// Copyright (c) 2016 Xomega.Net. All rights reserved.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base class for WPF views.
    /// A view can contain other child views that can be shown or hidden dynamically.
    /// </summary>
    public class WPFView : UserControl, IView, INotifyPropertyChanged
    {
        #region View properties

        /// <summary> Title of the view </summary>
        public string ViewTitle { get; set; }

        /// <summary>Default view width</summary>
        public double? ViewWidth { get; set; }

        /// <summary>Default view height</summary>
        public double? ViewHeight { get; set; }

        #endregion

        #region Binding

        /// <summary>Controller the view is bound to</summary>
        public ViewController Controller { get; private set; }

        /// <summary>
        /// Binds the view to its controller, or unbinds the current controller if null is passed.
        /// </summary>
        public virtual void BindTo(ViewController controller)
        {
            bool bind = controller != null;
            ViewController vc = bind ? controller : this.Controller;
            if (vc != null)
            {
                if (CloseButton != null)
                {
                    if (bind && vc.Params != null)
                        CloseButton.Visibility = (vc.Params[ViewParams.Mode.Param] == null) ?  Visibility.Hidden : Visibility.Visible;

                    if (bind) CloseButton.Click += Close;
                    else CloseButton.Click -= Close;
                }
                if (bind) vc.View = this;
                else vc.View = null;
            }
            this.Controller = controller;
        }

        /// <summary>
        /// Binds a framework element to the given data object
        /// </summary>
        /// <param name="el">Element to bind</param>
        /// <param name="obj">Data object to bind to</param>
        protected void BindDataObject(FrameworkElement el, DataObject obj)
        {
            if (el != null) el.DataContext = obj;
        }

        #endregion

        #region INotifyPropertyChanged support

        /// <summary> Event for INotifyPropertyChanged to facilitate clean binding in XAML </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises property changed event
        /// </summary>
        /// <param name="e">Event arguments with property name</param>
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        #endregion

        #region Showing the view

        /// <summary>
        /// Shows the view using the mode it was activated with
        /// </summary>
        /// <param name="owner">View owner</param>
        public virtual bool Show(object owner)
        {
            WPFView ownerView = owner as WPFView;
            ContentControl ownerPanel = ownerView != null ? ownerView.ChildPanel : null;
            if (ownerPanel != null && Controller.Params[ViewParams.Mode.Param] == ViewParams.Mode.Inline)
            {
                if (ViewWidth != null) MinWidth = ViewWidth.Value;
                if (ViewHeight != null) MinHeight = ViewHeight.Value;
                ownerPanel.Content = this;
                CollapseParentSplitter(false);
            }
            else
            {
                Window w = CreateWindow();
                if (owner is DependencyObject)
                    w.Owner = Window.GetWindow((DependencyObject)owner);
                w.Title = ViewTitle;
                w.Content = this;
                if (ViewWidth != null) w.Width = ViewWidth.Value;
                if (ViewHeight != null) w.Height = ViewHeight.Value;
                w.Closing += OnWindowClosing;
                w.Closed += OnWindowClosed;
                w.Show();
            }
            return true;
        }

        /// <summary>
        /// Creates a window for the view. Can be overridden in subclasses.
        /// </summary>
        /// <returns>A window for the view</returns>
        protected virtual Window CreateWindow()
        {
            return new Window();
        }

        #endregion

        #region View composition support

        /// <summary> Panel for displaying inline child views </summary>
        protected virtual ContentControl ChildPanel { get; }

        /// <summary>
        /// Gets the child view for the current view
        /// </summary>
        /// <returns>Current child view if any</returns>
        public IView GetChildView()
        {
            return ChildPanel == null ? null : ChildPanel.Content as IView;
        }

        /// <summary>
        /// Collapses or expands the parent view's splitter for current view
        /// </summary>
        /// <param name="collapsed">True to collapse, false to expand</param>
        protected virtual void CollapseParentSplitter(bool collapsed)
        {
            FrameworkElement panel = Parent as FrameworkElement;
            Grid grid = panel == null ? null : panel.Parent as Grid;
            if (panel == null) return; // no grid or parent
            int? col = panel.GetValue(Grid.ColumnProperty) as int?;
            if (col == null || grid.ColumnDefinitions.Count < col) return; // invalid column
            if (collapsed) grid.ColumnDefinitions[col.Value].Width = new GridLength(0);
            else if (grid.ColumnDefinitions[col.Value].Width.Value == 0)
                grid.ColumnDefinitions[col.Value].Width = GridLength.Auto;

            // find the splitter for the parent panel
            GridSplitter splitter = null;
            foreach (DependencyObject child in grid.Children)
            {
                GridSplitter sp = child as GridSplitter;
                if (sp == null) continue;
                else if (splitter == null)
                {
                    splitter = sp;
                    continue;
                }
                // more than one splitter in the parent grid -> get the closest one
                int? spCol = sp.GetValue(Grid.ColumnProperty) as int?;
                int? splitterCol = splitter.GetValue(Grid.ColumnProperty) as int?;
                if (spCol == null) continue;
                if (splitterCol == null || Math.Abs(col.Value - spCol.Value) < Math.Abs(col.Value - splitterCol.Value))
                    splitter = sp;
            }
            if (splitter != null)
                splitter.Visibility = collapsed ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion

        #region Closing the view

        /// <summary>
        /// Checks if the view can be closed
        /// </summary>
        /// <returns>True if the view can be closed, False otherwise</returns>
        public virtual bool CanClose()
        {
            // ask own controller first
            if (Controller != null && !Controller.CanClose()) return false;

            // ask any child views next
            IView childView = GetChildView();
            if (childView != null && !childView.CanClose()) return false;

            return true; // all good
        }

        /// <summary>
        /// Event handler for the view's parent window closing, which allows cancelling the event.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (!canCloseChecked && !CanClose())
                e.Cancel = true;
        }

        /// flag to prevent multiple CanClose checks
        protected bool canCloseChecked = false;

        /// <summary>
        /// Closes the view without checking first, which should be done separately by the caller.
        /// </summary>
        public virtual void Close()
        {
            ContentControl ownerPanel = Parent as ContentControl;
            if (ownerPanel != null && Controller.Params[ViewParams.Mode.Param] == ViewParams.Mode.Inline)
            {
                CollapseParentSplitter(true);
                Controller.FireEvent(ViewEvent.Closed);
                Dispose(); // unsets the controller, so do after firing
                ownerPanel.Content = null;
            }
            else
            {
                Window w = Window.GetWindow(this);
                if (w != null) w.Close();
                // disposal and controller event firing happens in the subsequent OnWindowClosed
            }
        }

        /// <summary> Button to close the view </summary>
        protected virtual Button CloseButton { get; }

        /// <summary>
        /// Checks if the view can be closed and closes the view. Default handler for the Close button.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        public void Close(object sender, EventArgs e)
        {
            if (!CanClose() || Controller == null) return;
            canCloseChecked = true;
            Close();
        }

        /// <summary>
        /// Event handler after the view's parent window has closed.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnWindowClosed(object sender, EventArgs e)
        {
            if (Controller != null)
                Controller.FireEvent(ViewEvent.Closed);
            Dispose();
            Window w = sender as Window;
            if (w != null && w.Owner != null) w.Owner.Activate();
        }

        /// <summary>
        /// Disposes the view and any child views
        /// </summary>
        public virtual void Dispose()
        {
            BindTo(null); // unbind controller to get it garbage collected in case of weak refs on this view
            IDisposable childView = ChildPanel == null ? null : ChildPanel.Content as IDisposable;
            if (childView != null) childView.Dispose();
        }

        #endregion
    }
}