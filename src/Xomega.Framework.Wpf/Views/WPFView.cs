// Copyright (c) 2021 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base class for WPF views.
    /// A view can contain other child views that can be shown or hidden dynamically.
    /// </summary>
    public class WPFView : UserControl, IView, IAsyncView, INotifyPropertyChanged
    {
        #region View properties and constructor

        /// <summary>Default view width.</summary>
        public double? ViewWidth { get; set; }

        /// <summary>Default view height.</summary>
        public double? ViewHeight { get; set; }

        /// <summary>
        /// Constructs a new WPF view
        /// </summary>
        public WPFView()
        {
            ChildViews = new List<WPFView>();
        }

        /// <summary>
        /// Indicates whether or not the view is using async operations.
        /// </summary>
        protected bool IsAsync { get; set; }

        /// <summary>
        /// Checks if the view can be deleted
        /// </summary>
        /// <returns>True if the view can be deleted, False otherwise</returns>
        public virtual bool CanDelete() => false;

        #endregion

        #region View title

        /// <summary>
        /// Text control that serves as a view title for inline views.
        /// </summary>
        protected virtual TextBlock TitleControl { get; }

        /// <summary>
        /// Updates either the title of the ower window or the view control with the view title.
        /// </summary>
        public virtual void UpdateViewTitle()
        {
            if (Model == null) return;
            Window w = Window.GetWindow(this);
            bool isTopLevel = (Owner == null || Window.GetWindow(Owner) != w);
            string viewTitle = Model.ViewTitle;
            if (isTopLevel && w != null && viewTitle != null)
                w.Title = viewTitle;
            if (TitleControl != null)
            {
                TitleControl.Visibility = isTopLevel ? Visibility.Collapsed : Visibility.Visible;
                TitleControl.Text = viewTitle;
            }
        }

        #endregion

        #region Binding

        /// <summary>View model the view is bound to</summary>
        public ViewModel Model { get; private set; }

        /// <summary>
        /// Binds the view to its model, or unbinds the current model if null is passed.
        /// </summary>
        public virtual void BindTo(ViewModel viewModel)
        {
            bool bind = viewModel != null;
            ViewModel vc = bind ? viewModel : Model;
            if (vc != null)
            {
                if (CloseButton != null)
                {
                    if (bind && vc.Params != null)
                        CloseButton.Visibility = (vc.Params[ViewParams.Mode.Param] == null) ?  Visibility.Hidden : Visibility.Visible;

                    if (bind) CloseButton.Click += Close;
                    else CloseButton.Click -= Close;
                }
                if (bind)
                {
                    vc.PropertyChanged += OnModelPropertyChanged;
                    vc.ViewEvents += OnViewEvents;
                    vc.View = this;
                }
                else
                {
                    vc.PropertyChanged -= OnModelPropertyChanged;
                    vc.ViewEvents -= OnViewEvents;
                    vc.View = null;
                }
            }
            Model = viewModel;
            // show errors (if any) last, since the error presenter needs the model's service provider
            OnModelPropertyChanged(bind ? vc : null, new PropertyChangedEventArgs(ViewModel.ErrorsProperty));
            OnModelPropertyChanged(bind ? vc : null, new PropertyChangedEventArgs(ViewModel.ViewTitleProperty));
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

        #region View model event handling

        /// <summary>A panel that shows errors and messages.</summary>
        protected virtual IErrorPresenter ErrorsPanel { get; }

        /// <summary>
        /// Error presenter for the view
        /// </summary>
        protected virtual IErrorPresenter ErrorPresenter
        {
            get
            {
                IErrorPresenter ep = ErrorsPanel ?? Model?.ServiceProvider.GetService<IErrorPresenter>();
                if (ep is Window) ((Window)ep).Owner = Window.GetWindow(this);
                return ep;
            }
        }

        /// <summary>
        /// Handles property change for errors to update the Errors panel
        /// </summary>
        /// <param name="sender">Model that sent the event</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ViewModel vc = sender as ViewModel;
            if (e.PropertyName == ViewModel.ErrorsProperty)
                ErrorPresenter?.Show(vc?.Errors);
            else if (e.PropertyName == ViewModel.ViewTitleProperty)
                UpdateViewTitle();
        }

        /// <summary>
        /// Listens for view events allowing subclasses to handle them.
        /// </summary>
        /// <param name="sender">View model that sent the event</param>
        /// <param name="e">View event</param>
        protected virtual void OnViewEvents(object sender, ViewEvent e)
        {
            // subclasses can override
        }

        #endregion

        #region INotifyPropertyChanged support

        /// <summary> Event for INotifyPropertyChanged to facilitate clean binding in XAML </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises property changed event.
        /// </summary>
        /// <param name="e">Event arguments with property name</param>
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
            => PropertyChanged?.Invoke(this, e);

        #endregion

        #region Showing the view

        /// <summary>
        /// Shows the view using the mode it was activated with
        /// </summary>
        public virtual bool Show()
        {
            ContentControl ownerPanel = Owner is WPFView ownerView ? ownerView.ChildPanel : null;
            if (Model.Params[ViewParams.Mode.Param] == ViewParams.Mode.Inline)
            {
                if (ViewWidth != null) MinWidth = ViewWidth.Value;
                if (ViewHeight != null) MinHeight = ViewHeight.Value;
                if (ownerPanel != null) ownerPanel.Content = this;
                CollapseParentSplitter(false);
            }
            else
            {
                Window w = CreateWindow();
                if (Owner != null) w.Owner = Window.GetWindow(Owner);
                if (ViewWidth != null) w.Width = ViewWidth.Value;
                if (ViewHeight != null) w.Height = ViewHeight.Value;
                w.Closing += OnWindowClosing;
                w.Closed += OnWindowClosed;
                w.Show();
            }
            UpdateViewTitle();
            return true;
        }

        /// <summary>
        /// Creates a window for the view. Can be overridden in subclasses.
        /// </summary>
        /// <returns>A window for the view</returns>
        protected virtual Window CreateWindow()
        {
            var windowCreator = Model?.ServiceProvider.GetService<IWindowCreator>();
            return windowCreator != null ? windowCreator.CreateWindow(this) : new Window { Content = this };
        }

        #endregion

        #region View composition support

        private DependencyObject owner;

        /// <summary>Owner for the view</summary>
        public DependencyObject Owner
        {
            get { return owner; }
            set {
                if (value == this)
                    throw new InvalidOperationException("Cannot set the owner to itself");
                if (owner is WPFView ownerView && !ownerView.ChildViews.Contains(this))
                    ownerView.ChildViews.Remove(this);
                owner = value;
                ownerView = owner as WPFView;
                if (ownerView != null && !ownerView.ChildViews.Contains(this))
                    ownerView.ChildViews.Add(this);
            }
        }

        /// <summary> A list of child views owned by this view </summary>
        private List<WPFView> ChildViews { get; set; }

        /// <summary> Panel for displaying inline child views </summary>
        protected virtual ContentControl ChildPanel { get; }

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
                if (!(child is GridSplitter sp)) continue;
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
            // ask own model first
            if (Model != null && !Model.CanClose()) return false;

            // ask any child views next
            if (ChildViews.Any(v => !v.CanClose())) return false;

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
            else Dispose(); // dispose before window is closed and possibly garbage collected
        }

        /// flag to prevent multiple CanClose checks
        protected bool canCloseChecked = false;

        /// <summary>
        /// Closes the view without checking first, which should be done separately by the caller.
        /// </summary>
        public virtual void Close()
        {
            if (Parent is ContentControl ownerPanel && Model?.Params[ViewParams.Mode.Param] == ViewParams.Mode.Inline)
            {
                CollapseParentSplitter(true);
                Model.FireEvent(ViewEvent.Closed);
                Dispose(); // unsets the model, so do after firing
                ownerPanel.Content = null;
            }
            else
            {
                Window w = Window.GetWindow(this);
                if (w != null) w.Close();
                // disposal and model event firing happens 
                // in the subsequent OnWindowClosing and OnWindowClosed respectively
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
            if (!CanClose() || Model == null) return;
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
            if (Model != null)
                Model.FireEvent(ViewEvent.Closed);
            if (sender is Window w && w.Owner != null) w.Owner.Activate();
        }

        /// <summary>
        /// Disposes the view and any child views
        /// </summary>
        public virtual void Dispose()
        {
            BindTo(null); // unbind model to get it garbage collected in case of weak refs on this view
            foreach (var v in ChildViews)
            {
                // close any child views without asking, since we should've asked before disposing
                v.canCloseChecked = true;
                v.Close();
            }
        }

        #endregion

        #region IAsyncView implementation

        /// <inheritdoc/>
        public Task<bool> ShowAsync(CancellationToken token = default)
            => Task.FromResult(Show());

        /// <inheritdoc/>
        public Task<bool> CanCloseAsync(CancellationToken token = default)
            => Task.FromResult(CanClose());

        /// <inheritdoc/>
        public Task CloseAsync(CancellationToken token = default)
        {
            Close();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<bool> CanDeleteAsync(CancellationToken token = default)
            => Task.FromResult(CanDelete());

        #endregion
    }
}