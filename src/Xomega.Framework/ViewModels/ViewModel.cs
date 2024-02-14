// Copyright (c) 2023 Xomega.Net. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
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

            ErrorParser = svcProvider.GetRequiredService<ErrorParser>();
            CloseAction = new ActionProperty(svcProvider, Messages.Action_Close);

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
            CloseAction.Visible = Params[ViewParams.Mode.Param] != null;
            return true;
        }

        /// <summary>
        /// Asynchronously activates the view model and the view.
        /// </summary>
        /// <param name="parameters">Parameters to activate the view with</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if the view was successfully activated, False otherwise</returns>
        public virtual async Task<bool> ActivateAsync(NameValueCollection parameters, CancellationToken token = default)
        {
            if (Activated && SameParams(parameters)) return false;
            Params = parameters == null ? new NameValueCollection() : new NameValueCollection(parameters);
            Activated = true;
            // make Close visible only for child views
            CloseAction.Visible = Params[ViewParams.Mode.Param] != null;
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Checks if the supplied parameters are the same as the ones the model has been activated with.
        /// </summary>
        /// <param name="parameters">Parameters to check</param>
        /// <returns>True if parameters are the same, false otherwise.</returns>
        protected virtual bool SameParams(NameValueCollection parameters)
        {
            if (Params == null && parameters == null) return true;
            if (Params == null && parameters != null || Params != null && parameters == null) return false;

            return Params.AllKeys.OrderBy(key => key).SequenceEqual(parameters.AllKeys.OrderBy(key => key))
                && Params.AllKeys.All(key => Params[key] == parameters[key]);
        }

        #endregion

        #region View

        /// <summary> The view for the model. </summary>
        public object View { get; set; }

        /// <summary>
        /// Base title of the view to be overridden in subclasses.
        /// The override can include additional data from the object.
        /// </summary>
        public virtual string BaseTitle => GetString("View_Title") ?? DataObject.StringToWords(GetResourceKey());

        /// <summary> Property name for the view title. </summary>
        public const string ViewTitleProperty = "ViewTitle";

        /// <summary>
        /// The current title of the view, which is based on the <see cref="BaseTitle"/>,
        /// and can include additional indicators of the view state, such as modification.
        /// </summary>
        public virtual string ViewTitle => BaseTitle;

        /// <summary>
        /// Gets a key for the current view that is used to look up localized resources.
        /// </summary>
        /// <returns>The resource key for the view.</returns>
        public virtual string GetResourceKey()
        {
            var t = GetType();
            if (t.Name.StartsWith(t.BaseType.Name))
                t = t.BaseType; // use base class for customized view models
            return Regex.Replace(t.Name, "ViewModel$", ""); // trim Model off the end
        }

        /// <summary>
        /// Gets localized string using the specified key and parameters.
        /// If no view-specific resource is defined for the key, a generic resource for the key will be used, if available.
        /// </summary>
        /// <param name="key">The resource key or the actual text.</param>
        /// <param name="values">Values to substitute into any placeholders in the text.</param>
        /// <returns>The localized view-specific or generic string for the key, if available, or null.</returns>
        public string GetString(string key, params object[] values)
        {
            var resources = ServiceProvider.GetService<ResourceManager>() ?? Messages.ResourceManager;
            var text = resources.GetString(key, GetResourceKey());
            return text == null ? null : string.Format(text, values);
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
        /// Asynchronously fires the specified event.
        /// </summary>
        public async Task FireEventAsync(ViewEvent evt, CancellationToken token = default)
            => await FireEventAsync(this, evt, token);

        /// <summary>
        /// Asynchronously fires the specified event using specific sender.
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
        public void OnPropertyChanged(PropertyChangedEventArgs e)
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
            tgtView.Show(); // always show even for the same view, in case mode has changed

            tgtViewModel.FireEvent(ViewEvent.Opened);

            return true;
        }

        /// <summary>
        /// Asynchronous routine to activate a view model with given parameters either as a child or as standalone
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
                else await curView.DisposeAsync(token);
            }
            tgtView.BindTo(tgtViewModel);
            await tgtView.ShowAsync(token); // always show even for the same view, in case mode has changed

            await tgtViewModel.FireEventAsync(ViewEvent.Opened);

            return true;
        }

        /// <summary>
        /// The action to close the view that can be bound to close buttons,
        /// which allows controlling when such buttons are visible or enabled.
        /// </summary>
        public ActionProperty CloseAction { get; private set; }

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
        /// Subscribes to child view's events.
        /// </summary>
        /// <param name="child">Child view</param>
        public virtual void SubscribeToChildEvents(ViewModel child)
        {
            if (child != null)
            {
                child.ViewEvents += OnChildEvent;
                child.AsyncViewEvents += OnChildEventAsync;
                child.PropertyChanged += OnChildPropertyChanged;
            }
        }

        /// <summary>
        /// Event handler for property change events on child view models,
        /// which propagates that event to this model's property change listeners.
        /// </summary>
        /// <param name="sender">The original view model that raised the event.</param>
        /// <param name="e">Property change event arguments.</param>
        protected virtual void OnChildPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
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
        /// <param name="keyChildProps">The key properties on the child details view.</param>
        /// <param name="e">Open/close event of the child details view.</param>
        /// <returns></returns>
        protected virtual bool UpdateListSelection(DataListObject list, List<DataProperty> keyChildProps, ViewEvent e)
        {
            if (list == null || keyChildProps == null || !keyChildProps.Any()) return false;
            // Find key property in the list with the same name, as the key property in the child details object.
            var keys = from ck in keyChildProps
                       from lk in list.Properties
                       where lk.IsKey && lk.Name == ck.Name
                       select new {
                           ListKey = lk,
                           ChildKey = ck
                       };
            if (keys.Count() == keyChildProps.Count)
            {
                if (e.IsOpened())
                    list.SelectedRows = list.GetData().Where(r => keys.All(k => 
                        Equals(k.ListKey.GetValue(ValueFormat.Internal, r), k.ChildKey.InternalValue))
                    ).ToList();
                else if (e.IsClosed()) list.ClearSelectedRows();
                return true;
            }
            return false;
        }

        private int openInlineViews = 1; // default to current view only

        /// <summary>
        /// The number of open inline child views plus the current view, if open.
        /// </summary>
        public int OpenInlineViews
        {
            get => openInlineViews;
            set
            {
                if (openInlineViews == value) return;
                openInlineViews = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(OpenInlineViews)));
            }
        }

        #endregion
    }
}