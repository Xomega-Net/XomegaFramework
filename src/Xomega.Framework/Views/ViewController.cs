// Copyright (c) 2016 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base class for controllers of different types of views.
    /// </summary>
    public abstract class ViewController
    {
        #region Initialization/Activation

        private IView view;

        /// <summary> The view for the controller </summary>
        public IView View
        {
            get { return view; }
            internal set
            {
                if (value == null) throw new ArgumentNullException("View");
                view = value;
            }
        }

        /// <summary> The service provider for the controller </summary>
        protected IServiceProvider serviceProvider;

        /// <summary>
        /// Base view constructor
        /// </summary>
        /// <param name="svcProvider">Service provider for the controller</param>
        /// <param name="view">View associated with the controller</param>
        public ViewController(IServiceProvider svcProvider, IView view)
        {
            if (svcProvider != null)
            {
                // create a separate scope for each view to avoid memore leaks
                var scope = svcProvider.CreateScope();
                if (scope != null)
                    this.serviceProvider = scope.ServiceProvider;
            }
            this.View = view;
            Initialize();
        }

        /// <summary> Initializes view data objects </summary>
        protected abstract void InitObjects();

        /// <summary>
        /// Initializes the view controller
        /// </summary>
        public virtual void Initialize()
        {
            InitObjects();
            View.BindTo(this);
        }

        /// <summary>
        /// Parameters the view was last activated with
        /// </summary>
        public NameValueCollection Params { get; set; }

        /// <summary>
        /// Activates the view controller and the view
        /// </summary>
        /// <param name="parameters">Parameters to activate the view with</param>
        /// <returns>True if the view was successfully activated, False otherwise</returns>
        public virtual bool Activate(NameValueCollection parameters)
        {
            Params = parameters;
            return View.Activate();
        }

        #endregion

        #region Show/Hide view

        /// <summary>
        /// Show the current view with specified activation parameters.
        /// </summary>
        /// <param name="ownerView">View owner</param>
        /// <param name="parameters">Activation parameters to use</param>
        /// <returns>Whether or not the view was shown successfully</returns>
        public bool Show(object ownerView, NameValueCollection parameters)
        {
            if (Activate(parameters))
                return View.Show(ownerView);
            else return false;
        }

        /// <summary> Occurs when the view is closed </summary>
        public event EventHandler Closed;

        /// <summary>Fires the Closed event</summary>
        public void FireClosed()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Checks if the view can be closed
        /// </summary>
        /// <returns>True if the view can be closed, False otherwise</returns>
        public virtual bool CanClose()
        {
            return View.CanClose();
        }

        /// <summary>Hides the view.</summary>
        public virtual void Hide()
        {
            if (CanClose())
            {
                View.Hide();
                FireClosed();
            }
        }

        /// <summary>
        /// Default handler for closing the view.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        public virtual void Close(object sender, EventArgs e)
        {
            Hide();
        }
        #endregion

        #region Handling child view updates

        /// <summary>
        /// Subscribes to child view's events
        /// </summary>
        /// <param name="child">Child view</param>
        protected virtual void SubscribeToChildEvents(ViewController child)
        {
            child.Closed += OnChildClosed;
            DetailsViewController details = child as DetailsViewController;
            if (details != null)
            {
                details.Saved += OnChildSaved;
                details.Deleted += OnChildDeleted;
            }
            SearchViewController search = child as SearchViewController;
            if (search != null)
            {
                search.Selected += OnChildSelection;
            }
        }

        /// <summary>
        /// Default handler for processing selection of a child search view.
        /// </summary>
        /// <param name="searchController">Search view controller where selection took place</param>
        /// <param name="selectedRows">Selected rows</param>
        protected virtual void OnChildSelection(object searchController, List<DataRow> selectedRows) { }

        /// <summary>
        /// Default handler for saving or deleting of a child details view.
        /// </summary>
        /// <param name="detailsController">Controller for view being saved or deleted</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnChildChanged(object detailsController, EventArgs e) { }

        /// <summary>
        /// Default handler for saving of a child details view.
        /// </summary>
        /// <param name="detailsController">Controller for view being saved</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnChildSaved(object detailsController, EventArgs e)
        {
            OnChildChanged(detailsController, e);
            View.Update();
        }

        /// <summary>
        /// Default handler for deleting of a child details view.
        /// </summary>
        /// <param name="detailsController">Controller for view being deleted</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnChildDeleted(object detailsController, EventArgs e)
        {
            OnChildChanged(detailsController, e);
            View.Update();
        }

        /// <summary>
        /// Default handler for closing of a child view.
        /// </summary>
        /// <param name="childViewController">Controller for view being closed</param>
        /// <param name="e">Event arguments</param>
        protected virtual void OnChildClosed(object childViewController, EventArgs e)
        {
            View.Update();
        }

        #endregion
    }
}