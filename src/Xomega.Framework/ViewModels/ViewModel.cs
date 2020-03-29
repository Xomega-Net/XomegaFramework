// Copyright (c) 2020 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;

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

            // create a separate scope for each view to avoid memory leaks
            var scope = svcProvider.CreateScope();
            ServiceProvider = (scope != null) ? scope.ServiceProvider : svcProvider;

            Params = new NameValueCollection();

            ErrorParser = svcProvider.GetService<ErrorParser>() ?? new ErrorParser(svcProvider.GetService<ResourceManager>());

            Initialize();
        }

        /// <summary>
        /// Initializes the view model
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Parameters the view was last activated with.
        /// </summary>
        public NameValueCollection Params { get; private set; }
        
        /// <summary>
        /// Indicates if the view model has been activated.
        /// </summary>
        public bool Activated { get; private set; }

        /// <summary>
        /// Activates the view model and the view.
        /// </summary>
        /// <param name="parameters">Parameters to activate the view with</param>
        /// <returns>True if the view was successfully activated, False otherwise</returns>
        public virtual bool Activate(NameValueCollection parameters)
        {
            Params = parameters == null ? new NameValueCollection() : new NameValueCollection(parameters);
            Activated = true;
            return true;
        }

        /// <summary>
        /// Asynchrounsly activates the view model and the view.
        /// </summary>
        /// <param name="parameters">Parameters to activate the view with</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if the view was successfully activated, False otherwise</returns>
        public virtual async Task<bool> ActivateAsync(NameValueCollection parameters, CancellationToken token = default)
        {
            if (Activated) return false;
            Params = parameters == null ? new NameValueCollection() : new NameValueCollection(parameters);
            Activated = true;
            return await Task.FromResult(true);
        }

        #endregion

        #region View

        /// <summary> The view for the model. </summary>
        public object View { get; set; }

        /// <summary>
        /// Base title of the view to be overridden in subclasses.
        /// The override can include additional data from the object.
        /// </summary>
        public virtual string BaseTitle => GetString("View");

        /// <summary> Property name for the view title. </summary>
        public const string ViewTitleProperty = "ViewTitle";

        /// <summary>
        /// The current title of the view, which is based on the <see cref="BaseTitle"/>,
        /// and can include additional indicators of the view state, such as modification.
        /// </summary>
        public virtual string ViewTitle => BaseTitle;

        /// <summary>
        /// Gets localized string using the specified key and parameters.
        /// If no resource is defined for the key, the key text will be used.
        /// </summary>
        /// <param name="key">The resource key or the actual text.</param>
        /// <param name="values">Values to substitute into any placeholders in the text.</param>
        /// <returns></returns>
        public string GetString(string key, params object[] values)
        {
            var resources = ServiceProvider.GetService<ResourceManager>() ?? Messages.ResourceManager;
            var text = resources.GetString(key) ?? key;
            return string.Format(text, values);
        }

        #endregion

        #region Events

        /// <summary>
        /// Events that happened to the current view or any of its child views.
        /// </summary>
        public event EventHandler<ViewEvent> ViewEvents;

        /// <summary>
        /// Fires the specified event.
        /// </summary>
        public void FireEvent(ViewEvent evt) => FireEvent(this, evt);

        /// <summary>
        /// Fires the specified event using specific sender.
        /// </summary>
        public void FireEvent(object sender, ViewEvent evt)
        {
            ViewEvents?.Invoke(sender, evt);
        }

        /// <summary>
        /// Async events that happened to the current view or any of its child views.
        /// </summary>
        public event AsyncViewEventHandler AsyncViewEvents;

        /// <summary>
        /// Asyncronously fires the specified event.
        /// </summary>
        public async Task FireEventAsync(ViewEvent evt, CancellationToken token = default)
            => await FireEventAsync(this, evt, token);

        /// <summary>
        /// Asyncronously fires the specified event using specific sender.
        /// </summary>
        public async Task FireEventAsync(object sender, ViewEvent evt, CancellationToken token = default)
        {
            var tasks = AsyncViewEvents?.GetInvocationList()?.Select(d => (Task)d.DynamicInvoke(sender, evt, token));
            if (tasks != null)
                await Task.WhenAll(tasks);
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
        public ErrorParser ErrorParser { get; set; }

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

            tgtViewModel.FireEvent(ViewEvent.Opened);

            return true;
        }

        /// <summary>
        /// Asynchrouns routine to activate a view model with given parameters either as a child or as standalone
        /// and display it in a new view or an existing view, which will need to be successfully closed first.
        /// </summary>
        /// <param name="tgtViewModel">View model to activate</param>
        /// <param name="tgtView">View to bind to the model</param>
        /// <param name="activationParams">A collection of activation parameters</param>
        /// <param name="srcViewModel">Parent model, or null if activate as standalone</param>
        /// <param name="curView">Optional current view to be replaced by the target view. Same as tgtView if we need to reuse the target view</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if navigation succeeded, false otherwise</returns>
        public static async Task<bool> NavigateToAsync(ViewModel tgtViewModel, IAsyncView tgtView,
            NameValueCollection activationParams, ViewModel srcViewModel, IAsyncView curView, CancellationToken token = default)
        {
            if (tgtViewModel == null || tgtView == null || curView != null && !(await curView.CanCloseAsync(token)))
                return false;

            if (srcViewModel != null)
                srcViewModel.SubscribeToChildEvents(tgtViewModel);
            if (!(await tgtViewModel.ActivateAsync(activationParams, token)) && srcViewModel != null)
                return false; // child model activation reports showing view is not needed

            if (curView != null)
            { // close the current view, or dispose if reusing it
                if (curView != tgtView)
                    await curView.CloseAsync(token);
                else curView.Dispose();
            }
            tgtView.BindTo(tgtViewModel);
            if (tgtView != curView)
                await tgtView.ShowAsync(token);

            await tgtViewModel.FireEventAsync(ViewEvent.Opened);

            return true;
        }

        /// <summary>
        /// Performs model level checks if the view can be closed
        /// </summary>
        /// <returns>True if the view can be closed, false otherwise</returns>
        public virtual bool CanClose() => true;

        /// <summary>
        /// Closes the view.
        /// </summary>
        protected void Close() => (View as IView)?.Close();

        /// <summary>
        /// Performs model level checks if the view can be closed
        /// </summary>
        /// <returns>True if the view can be closed, false otherwise</returns>
        public virtual async Task<bool> CanCloseAsync(CancellationToken token = default)
            => await Task.FromResult(true);

        /// <summary>
        /// Closes the view asynchronously.
        /// </summary>
        protected async Task CloseAsync(CancellationToken token = default)
            => await (View as IAsyncView)?.CloseAsync(token);

        #endregion

        #region Handling child view updates

        /// <summary>
        /// Subscribes to child view's events
        /// </summary>
        /// <param name="child">Child view</param>
        public virtual void SubscribeToChildEvents(ViewModel child)
        {
            if (child != null)
            {
                child.ViewEvents += OnChildEvent;
                child.AsyncViewEvents += OnChildEventAsync;
            }
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

        /// <summary>
        /// Default handler for child events, which just re-publishes them.
        /// </summary>
        /// <param name="childViewModel">Child view model that fired the original event</param>
        /// <param name="e">Event object</param>
        /// <param name="token">Cancellation token.</param>
        protected virtual async Task OnChildEventAsync(object childViewModel, ViewEvent e, CancellationToken token = default)
        {
            await FireEventAsync(childViewModel, e + ViewEvent.Child, token);
        }

        /// <summary>
        /// Updates selection in the specified list object for a details open/close event
        /// using the provided key property on the details view object.
        /// </summary>
        /// <param name="list">Data list object to update.</param>
        /// <param name="keyChildProp">The key property on the child details view.</param>
        /// <param name="e">Open/close event of the chilid details view.</param>
        /// <returns></returns>
        protected virtual bool UpdateListSelection(DataListObject list, DataProperty keyChildProp, ViewEvent e)
        {
            // Find key property in the list with the same name, as the key property in the child details object.
            var keyListProp = list?.Properties?.Where(p => p.IsKey && p.Name == keyChildProp?.Name)?.FirstOrDefault();
            if (keyListProp != null)
            {
                if (e.IsOpened())
                    list.SelectedRows = list.GetData().Where(r => Equals(keyListProp.GetValue(ValueFormat.Internal, r),
                        keyChildProp.InternalValue)).ToList();
                else if (e.IsClosed()) list.ClearSelectedRows();
                return true;
            }
            return false;
        }

        #endregion
    }
}