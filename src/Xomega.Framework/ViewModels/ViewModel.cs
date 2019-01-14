// Copyright (c) 2019 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base class for models of different types of views.
    /// </summary>
    public abstract class ViewModel : INotifyPropertyChanged
    {
        #region Initialization/Activation

        /// <summary> The service provider for the model </summary>
        public IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Base view constructor
        /// </summary>
        /// <param name="svcProvider">Base service provider for the model</param>
        public ViewModel(IServiceProvider svcProvider)
        {
            if (svcProvider == null) throw new ArgumentNullException("svcProvider");

            // create a separate scope for each view to avoid memore leaks
            var scope = svcProvider.CreateScope();
            this.ServiceProvider = (scope != null) ? scope.ServiceProvider : svcProvider;

            Params = new NameValueCollection();

            errorParser = svcProvider.GetService<ErrorParser>();
            if (errorParser == null) errorParser = new ErrorParser();

            Initialize();
        }

        /// <summary>
        /// Initializes the view model
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Parameters the view was last activated with
        /// </summary>
        public NameValueCollection Params { get; private set; }

        /// <summary>
        /// Activates the view model and the view
        /// </summary>
        /// <param name="parameters">Parameters to activate the view with</param>
        /// <returns>True if the view was successfully activated, False otherwise</returns>
        public virtual bool Activate(NameValueCollection parameters)
        {
            Params = parameters == null ? new NameValueCollection() : new NameValueCollection(parameters);
            return true;
        }

        #endregion

        #region View

        /// <summary> The view for the model </summary>
        public IView View { get; set; }

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
        /// about changes in model's properties
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

        #region Error list

        /// <summary>
        /// Property name for the error list
        /// </summary>
        public const string ErrorsProperty = "Errors";

        private ErrorList errors;

        /// <summary>
        /// The current list of errors of the view model
        /// </summary>
        public ErrorList Errors
        {
            get { return errors; }
            set
            {
                errors = value;
                OnPropertyChanged(new PropertyChangedEventArgs(ErrorsProperty));
            }
        }

        /// <summary>
        /// The error parser for handling service exceptions
        /// </summary>
        protected ErrorParser errorParser;

        #endregion

        #region Navigation

        /// <summary>
        /// Generic routine to activate a view model with given parameters either as a child or as standalone
        /// and display it in a new view or an existing view, which will need to be successfully closed first.
        /// </summary>
        /// <param name="tgtViewModel">View model to activate</param>
        /// <param name="tgtView">View to bind to the model</param>
        /// <param name="activationParams">A collection of activation parameters</param>
        /// <param name="srcViewModel">Parent model, or null if activate as standalone</param>
        /// <param name="curView">Optional current view to be replaced by the target view. Same as tgtView if we need to reuse the target view</param>
        /// <returns>True if navigation succeeded, false otherwise</returns>
        public static bool NavigateTo(ViewModel tgtViewModel, IView tgtView, NameValueCollection activationParams, ViewModel srcViewModel, IView curView)
        {
            if (tgtViewModel == null || tgtView == null || curView != null && !curView.CanClose()) return false;

            if (srcViewModel != null)
                srcViewModel.SubscribeToChildEvents(tgtViewModel);
            if (!tgtViewModel.Activate(activationParams) && srcViewModel != null)
                return false; // child model activation reports showing view is not needed

            if (curView != null)
            { // close the current view, or dispose if reusing it
                if (curView != tgtView) curView.Close();
                else curView.Dispose();
            }
            tgtView.BindTo(tgtViewModel);
            if (tgtView != curView) tgtView.Show();

            return true;
        }

        /// <summary>
        /// Performs model level checks if the view can be closed
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
        public virtual void SubscribeToChildEvents(ViewModel child)
        {
            if (child != null)
                child.ViewEvents += OnChildEvent;
        }

        /// <summary>
        /// Default handler for child events, which just re-publishes them.
        /// </summary>
        /// <param name="childViewModel">Child view model that fired the original event</param>
        /// <param name="e">Event object</param>
        protected virtual void OnChildEvent(object childViewModel, ViewEvent e)
        {
            FireEvent(childViewModel, e + ViewEvent.Child);
        }

        #endregion
    }
}