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
    public class WPFView : UserControl, IView
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

        /// <summary>Binds the view to its controller</summary>
        public virtual void BindTo(ViewController controller)
        {
            this.Controller = controller;
            if (Controller != null && CloseButton != null)
                CloseButton.Click += Controller.Close;
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
            w.Closed += delegate (object sender, EventArgs e)
            {
                if (!isClosed && Controller != null)
                    Controller.FireClosed();
                if (w.Owner != null) w.Owner.Activate();
            };
            w.Show();
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