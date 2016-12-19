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

        /// <summary>
        /// Checks if the view can be closed
        /// </summary>
        /// <returns>True if the view can be closed, False otherwise</returns>
        public virtual bool CanClose() { return true; }

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
        public void Show(object owner)
        {
            DependencyObject ownerView = owner as DependencyObject;
            Window w = new Window();
            if (ownerView != null) w.Owner = Window.GetWindow(ownerView);
            w.Title = ViewTitle;
            w.Content = this;
            if (ViewWidth != null) w.Width = ViewWidth.Value;
            if (ViewHeight != null) w.Height = ViewHeight.Value;
            w.Closing += delegate(object sender, CancelEventArgs e)
            {
                if (Controller != null && !Controller.CanClose())
                    e.Cancel = true;
            };
            w.Closed += OnClosed;
            w.Show();
        }

        protected virtual void OnClosed(object sender, EventArgs e)
        {
            if (!isClosed && Controller != null)
                Controller.FireClosed();
            BindTo(null); // unbind controller to get it garbage collected in case of weak refs on this view
            Window w = sender as Window;
            if (w != null && w.Owner != null) w.Owner.Activate();
        }

        // prevents firing Controller.Closed event twice when closing through the Hide method
        private bool isClosed = false;

        /// <summary>
        /// Hides the view
        /// </summary>
        public void Hide()
        {
            isClosed = true;
            Window w = Window.GetWindow(this);
            if (w != null) w.Close();
        }
    }
}