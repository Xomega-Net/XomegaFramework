// Copyright (c) 2016 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base class for controllers of different types of views.
    /// </summary>
    public abstract class ViewController : INotifyPropertyChanged
    {
        #region Initialization/Activation

        private IView view;

        /// <summary> The view for the controller </summary>
        public IView View
        {
            get { return view; }
            internal set { view = value; }
        }

        /// <summary>
        /// Creates a new view for the current controller, if supported.
        /// </summary>
        /// <returns>A new view, or null if view creation for this controller is not supported</returns>
        public virtual IView CreateView() {
            return null; // to be implemented by concrete controllers as needed
        }


        /// <summary> The service provider for the controller </summary>
        protected IServiceProvider serviceProvider;

        /// <summary>
        /// Base view constructor
        /// </summary>
        /// <param name="svcProvider">Service provider for the controller</param>
        public ViewController(IServiceProvider svcProvider)
        {
            if (svcProvider == null) throw new ArgumentNullException("svcProvider");

            // create a separate scope for each view to avoid memore leaks
            var scope = svcProvider.CreateScope();
            this.serviceProvider = (scope != null) ? scope.ServiceProvider : svcProvider;

            Initialize();
        }

        /// <summary>
        /// Initializes the view controller
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Parameters the view was last activated with
        /// </summary>
        public NameValueCollection Params { get; private set; }

        /// <summary>
        /// Activates the view controller and the view
        /// </summary>
        /// <param name="parameters">Parameters to activate the view with</param>
        /// <returns>True if the view was successfully activated, False otherwise</returns>
        public virtual bool Activate(NameValueCollection parameters)
        {
            Params = parameters;
            return true;
        }

        #endregion

        #region Events

        /// <summary>
        /// Events that happened to the current view or any of its child views
        /// </summary>
        public event EventHandler<ViewEvent> ViewEvents;

        /// <summary>
        /// Fires the specified event
        /// </summary>
        public void FireEvent(ViewEvent evt)
        {
            FireEvent(this, evt);
        }

        /// <summary>
        /// Fires the specified event using specific sender
        /// </summary>
        public void FireEvent(object sender, ViewEvent evt)
        {
            ViewEvents?.Invoke(sender, evt);
        }

        /// <summary>
        /// Implements INotifyPropertyChanged to notify listeners
        /// about changes in controller's properties
        /// </summary>
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

        #region Navigation

        /// <summary>
        /// Generic routine to activate a standalone view controller with given parameters.
        /// </summary>
        /// <param name="tgtController">View controller to activate</param>
        /// <param name="activationParams">A collection of activation parameters</param>
        /// <param name="owner">Owner for the view, but not a parent listening to its events</param>
        /// <returns>True if navigation succeeded, false otherwise</returns>
        public static bool NavigateTo(ViewController tgtController, NameValueCollection activationParams, object owner)
        {
            return NavigateTo(tgtController, activationParams, null, null, owner);
        }

        /// <summary>
        /// Generic routine to activate a view controller with given parameters either as a child or as standalone
        /// and display it in a new view or an existing view, which will need to be successfully closed first.
        /// </summary>
        /// <param name="tgtController">View controller to activate</param>
        /// <param name="activationParams">A collection of activation parameters</param>
        /// <param name="srcController">Parent controller, or null if activate as standalone</param>
        /// <param name="tgtView">View to bind to the controller, or null to create a new view</param>
        /// <param name="owner">Custom owner for the new target view in case it's not the source controller's view</param>
        /// <returns>True if navigation succeeded, false otherwise</returns>
        public static bool NavigateTo(ViewController tgtController, NameValueCollection activationParams, ViewController srcController, IView tgtView, object owner)
        {
            if (tgtView != null && !tgtView.CanClose()) return false;

            if (srcController != null)
                srcController.SubscribeToChildEvents(tgtController);
            if (!tgtController.Activate(activationParams) && srcController != null)
                return false; // child controller activation reports showing view is not needed

            IView view = tgtController.CreateView();
            if (view == null) return false;

            if (tgtView != null && view.GetType().Equals(tgtView.GetType()))
            {   // if target view is compatible with target controller's view type 
                // then reuse it by binding it to the current controller to preserve its state
                tgtView.Dispose();
                tgtView.BindTo(tgtController);
            }
            else
            {
                if (tgtView != null) tgtView.Close();
                view.BindTo(tgtController);
                view.Show(owner ?? (srcController != null ? srcController.View : null));
            }
            return true;
        }

        /// <summary>
        /// Performs controller level checks if the view can be closed
        /// </summary>
        /// <returns>True if the view can be closed, false otherwise</returns>
        public virtual bool CanClose()
        {
            return true;
        }

        /// <summary>
        /// Closes the view
        /// </summary>
        protected void Close()
        {
            if (View != null) View.Close();
        }

        #endregion

        #region Handling child view updates

        /// <summary>
        /// Subscribes to child view's events
        /// </summary>
        /// <param name="child">Child view</param>
        protected virtual void SubscribeToChildEvents(ViewController child)
        {
            child.ViewEvents += OnChildEvent;
        }

        /// <summary>
        /// Default handler for child events, which just re-publishes them.
        /// </summary>
        /// <param name="childController">Child view controller that fired the original event</param>
        /// <param name="e">Event object</param>
        protected virtual void OnChildEvent(object childController, ViewEvent e)
        {
            FireEvent(childController, e + ViewEvent.Child);
        }

        #endregion
    }
}