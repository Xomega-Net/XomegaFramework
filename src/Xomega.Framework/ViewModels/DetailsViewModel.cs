// Copyright (c) 2023 Xomega.Net. All rights reserved.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base class for models of details views
    /// </summary>
    public class DetailsViewModel : ViewModel
    {
        #region Initialization/Activation

        /// <summary>
        /// Constructs a new details view model
        /// </summary>
        /// <param name="svcProvider">Service provider for the model</param>
        public DetailsViewModel(IServiceProvider svcProvider) : base(svcProvider)
        {
        }

        /// <inheritdoc/>
        public override bool Activate(NameValueCollection parameters)
        {
            if (!base.Activate(parameters) || DetailsObject == null) return false;

            InitializeObject();

            if (ViewParams.Action.Create != Params[ViewParams.Action.Param])
                LoadData(false);

            return true;
        }

        /// <inheritdoc/>
        public override async Task<bool> ActivateAsync(NameValueCollection parameters, CancellationToken token = default)
        {
            if (!await base.ActivateAsync(parameters, token) || DetailsObject == null) return false;

            await InitializeObjectAsync(token);

            if (ViewParams.Action.Create != Params[ViewParams.Action.Param])
                await LoadDataAsync(false, token);

            return true;
        }

        /// <summary>
        /// Initializes view model's details object.
        /// </summary>
        protected virtual void InitializeObject()
        {
            DetailsObject.UpdateComputed();
            DetailsObject.SetValues(Params);

            if (ViewParams.Action.Create == Params[ViewParams.Action.Param])
                DetailsObject.SetModified(false, true);
        }

        /// <summary>
        /// Initializes view model's details object asynchronously.
        /// </summary>
        protected virtual async Task InitializeObjectAsync(CancellationToken token)
        {
            DetailsObject.UpdateComputed();
            await DetailsObject.SetValuesAsync(Params, token);

            if (ViewParams.Action.Create == Params[ViewParams.Action.Param])
                DetailsObject.SetModified(false, true);
        }

        #endregion

        #region Data object

        private DataObject detailsObject;

        /// <summary>
        /// The primary data object for the details view.
        /// </summary>
        public DataObject DetailsObject {
            get => detailsObject;
            set
            {
                if (detailsObject != null) detailsObject.PropertyChanged -= OnDetailsObjectChanged;
                detailsObject = value;
                if (detailsObject != null) detailsObject.PropertyChanged += OnDetailsObjectChanged;
            }
        }

        /// <summary>
        /// Handles changes in the Modified and IsNew properties of the data object to notify views of any view title updates.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Property change event arguments.</param>
        protected virtual void OnDetailsObjectChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataObject.Modified) ||
                e.PropertyName == nameof(DataObject.IsNew))
                OnPropertyChanged(new PropertyChangedEventArgs(ViewTitleProperty));
        }

        /// <inheritdoc/>
        public override string ViewTitle
        {
            get
            {
                bool isNew = DetailsObject?.IsNew ?? true;
                bool isModified = DetailsObject?.IsModified() ?? false;
                string title = BaseTitle;
                if (isNew) title = GetString(Messages.View_TitleNew, title);
                if (isModified) title += "*";
                return title;
            }
        }

        #endregion

        #region Data loading

        /// <summary>
        /// Main function to load details data.
        /// </summary>
        /// <param name="preserveSelection">A flag indicating whether or not to preserve selection in child lists.</param>
        public virtual void LoadData(bool preserveSelection)
        {
            if (DetailsObject == null) return;
            try
            {
                Errors = DetailsObject.Read(new DataObject.CrudOptions { PreserveSelection = preserveSelection });
            }
            catch (Exception ex)
            {
                Errors = ErrorParser.FromException(ex);
            }
        }

        /// <summary>
        /// Main function to load details data.
        /// </summary>
        /// <param name="preserveSelection">A flag indicating whether or not to preserve selection in child lists.</param>
        /// <param name="token">Cancellation token.</param>
        public virtual async Task LoadDataAsync(bool preserveSelection, CancellationToken token = default)
        {
            if (DetailsObject == null) return;
            try
            {
                Errors = await DetailsObject.ReadAsync(new DataObject.CrudOptions { PreserveSelection = preserveSelection }, token);
            }
            catch (Exception ex)
            {
                Errors = ErrorParser.FromException(ex);
            }
        }

        #endregion

        #region Child updates

        /// <summary>
        /// Finds a child list for the child details view and updates its selected rows
        /// when the child details view is opened or closed.
        /// </summary>
        /// <param name="dvm">View model of the child details view.</param>
        /// <param name="e">View event of the child details view.</param>
        protected virtual void UpdateDetailsSelection(DetailsViewModel dvm, ViewEvent e)
        {
            var keyProp = DetailsObject?.Properties?.Where(p => p.IsKey)?.FirstOrDefault();
            var keyChildProp = dvm?.DetailsObject?.Properties?.Where(p => p.IsKey && p.Name != keyProp?.Name)?.FirstOrDefault();
            if (keyChildProp == null) return;

            foreach (var list in DetailsObject.Children.Where(c => c is DataListObject).Cast<DataListObject>())
            {
                if (UpdateListSelection(list, keyChildProp, e)) break;
            }
        }

        /// <summary>
        /// Default handler for saving or deleting of a child details view.
        /// </summary>
        /// <param name="childViewModel">Child view model that fired the original event</param>
        /// <param name="e">Event object</param>
        protected override void OnChildEvent(object childViewModel, ViewEvent e)
        {
            UpdateDetailsSelection(childViewModel as DetailsViewModel, e);
            // ignore events from grandchildren
            if (e.IsSaved(false) || e.IsDeleted(false))
            {
                LoadData(true); // reload child lists if a child was updated
                UpdateDetailsSelection(childViewModel as DetailsViewModel, ViewEvent.Opened);
            }

            base.OnChildEvent(childViewModel, e);
        }

        /// <summary>
        /// Default async handler for saving or deleting of a child details view.
        /// </summary>
        /// <param name="childViewModel">Child view model that fired the original event</param>
        /// <param name="e">Event object</param>
        /// <param name="token">Cancellation token.</param>
        protected override async Task OnChildEventAsync(object childViewModel, ViewEvent e, CancellationToken token = default)
        {
            UpdateDetailsSelection(childViewModel as DetailsViewModel, e);
            // ignore events from grandchildren
            if (e.IsSaved(false) || e.IsDeleted(false))
            {
                await LoadDataAsync(true, token); // reload child lists if a child was updated
                UpdateDetailsSelection(childViewModel as DetailsViewModel, ViewEvent.Opened);
            }

            await base.OnChildEventAsync(childViewModel, e, token);
        }

        #endregion

        #region Event handling

        /// <summary>
        /// Handler for saving the current view
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        public virtual void Save(object sender, EventArgs e)
        {
            if (DetailsObject == null) return;
            try
            {
                Errors = DetailsObject.Save(null);
                Errors?.AbortIfHasErrors();
                FireEvent(ViewEvent.Saved);
            }
            catch (Exception ex)
            {
                Errors = ErrorParser.FromException(ex);
            }
        }

        /// <summary>
        /// Handler for asynchronously saving the current view.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        public virtual async Task SaveAsync(CancellationToken token = default)
        {
            if (DetailsObject == null) return;
            try
            {
                Errors = await DetailsObject.SaveAsync(null, token);
                Errors?.AbortIfHasErrors();
                await FireEventAsync(ViewEvent.Saved, token);
            }
            catch (Exception ex)
            {
                Errors = ErrorParser.FromException(ex);
            }
        }

        /// <summary>
        /// A function that determines if the current object can be saved.
        /// </summary>
        /// <returns></returns>
        public virtual bool SaveEnabled() => true;

        /// <summary>
        /// Handler for deleting the object displayed in the current view.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        public virtual void Delete(object sender, EventArgs e)
        {
            if (DetailsObject == null || (View is IView v) && !v.CanDelete()) return;
            try
            {
                Errors = DetailsObject.Delete(null);
                Errors?.AbortIfHasErrors();
                FireEvent(ViewEvent.Deleted);
                DetailsObject.SetModified(false, true); // so that we could close without asking
                Close();
            }
            catch (Exception ex)
            {
                Errors = ErrorParser.FromException(ex);
            }
        }

        /// <summary>
        /// Handler for deleting the object displayed in the current view.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        public virtual async Task DeleteAsync(CancellationToken token = default)
        {
            if (DetailsObject == null || (View is IAsyncView av) && !(await av.CanDeleteAsync(token))) return;
            try
            {
                Errors = await DetailsObject.DeleteAsync(null, token);
                Errors?.AbortIfHasErrors();
                await FireEventAsync(ViewEvent.Deleted, token);
                DetailsObject.SetModified(false, true); // so that we could close without asking
                await CloseAsync(token);
            }
            catch (Exception ex)
            {
                Errors = ErrorParser.FromException(ex);
            }
        }

        /// <summary>
        /// A function that determines if the current object can be deleted.
        /// </summary>
        public virtual bool DeleteEnabled()
        {
            return DetailsObject != null && !DetailsObject.IsNew;
        }

        #endregion
    }
}