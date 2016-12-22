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
    public class WPFView : UserControl, IView, INotifyPropertyChanged, IDisposable
    {
        /// <summary> Title of the view </summary>
        public string ViewTitle { get; set; }

        /// <summary>Default view width</summary>
        public double? ViewWidth { get; set; }

        /// <summary>Default view height</summary>
        public double? ViewHeight { get; set; }

        /// <summary>Controller the view is bound to</summary>
        public ViewController Controller { get; private set; }

        /// <summary> Button to close the view </summary>
        public virtual Button CloseButton { get; }

        /// <summary> Panel for displaying inline child views </summary>
        public virtual ContentControl ChildPanel { get; }

        /// <summary>
        /// Checks if the view can be closed
        /// </summary>
        /// <returns>True if the view can be closed, False otherwise</returns>
        public virtual bool CanClose()
        {
            WPFView childView = ChildPanel == null ? null : ChildPanel.Content as WPFView;
            return childView == null || childView.Controller == null || childView.Controller.CanClose();
        }

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
                    if (bind) CloseButton.Click += vc.Close;
                    else CloseButton.Click -= vc.Close;
                }
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

        /// <summary>
        /// Activates the view
        /// </summary>
        /// <returns>True if the view was successfully activated, False otherwise</returns>
        public virtual bool Activate()
        {
            if (Controller != null && Controller.Params != null && CloseButton != null)
                CloseButton.Visibility = (Controller.Params[ViewParams.Mode.Param] == null) ? Visibility.Hidden : Visibility.Visible;
            return true;
        }

        /// <summary>
        /// Updates the view based on other views' changes
        /// </summary>
        public void Update()
        {
            // nothing to do, WPF views will update automatically
        }

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
                WPFView currentView = ownerPanel.Content as WPFView;
                if (currentView != null)
                {
                    if (currentView.Controller != null && !currentView.Controller.CanClose())
                        return false;
                    currentView.Dispose();
                }
                if (currentView != null && currentView.GetType().Equals(GetType()))
                {   // if existing view is of the same type then reuse it to preserve state
                    // by binding the it to the current controller
                    currentView.BindTo(Controller);
                    Controller.View = currentView;
                    BindTo(null); // unbind new view
                }
                else
                {
                    ownerPanel.Content = this;
                    if (ViewWidth != null) MinWidth = ViewWidth.Value;
                    if (ViewHeight != null) MinHeight = ViewHeight.Value;
                    CollapseParentSplitter(false);
                }
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

        /// <summary>
        /// Event handler for the view's parent window closing, which allows cancelling the event.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (!wasClosed && Controller != null && !Controller.CanClose())
                e.Cancel = true;
        }

        /// <summary>
        /// Event handler after the view's parent window has closed.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnWindowClosed(object sender, EventArgs e)
        {
            if (!wasClosed && Controller != null)
                Controller.FireClosed();
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

        /// indicates if the view was closed through the Hide method
        /// to suppress any checks/actions that were performed there
        protected bool wasClosed = false;

        /// <summary>
        /// Hides the view
        /// </summary>
        public void Hide()
        {
            wasClosed = true;
            ContentControl ownerPanel = Parent as ContentControl;
            if (ownerPanel != null && Controller.Params[ViewParams.Mode.Param] == ViewParams.Mode.Inline)
            {
                CollapseParentSplitter(true);
                Dispose();
                ownerPanel.Content = null;
            }
            else
            {
                Window w = Window.GetWindow(this);
                if (w != null) w.Close();
            }
        }
    }
}