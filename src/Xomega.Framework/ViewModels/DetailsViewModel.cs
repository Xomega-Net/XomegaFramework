// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System;
using System.Collections.Specialized;
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

            DetailsObject.SetValues(Params);

            if (ViewParams.Action.Create == Params[ViewParams.Action.Param])
                DetailsObject.SetModified(false, true);
            else LoadData(false);
            return true;
        }

        /// <inheritdoc/>
        public override async Task<bool> ActivateAsync(NameValueCollection parameters, CancellationToken token = default)
        {
            if (!await base.ActivateAsync(parameters, token) || DetailsObject == null) return false;

            DetailsObject.SetValues(Params);

            if (ViewParams.Action.Create == Params[ViewParams.Action.Param])
                DetailsObject.SetModified(false, true);
            else await LoadDataAsync(false, token);
            return true;
        }

        #endregion

        #region Data object

        /// <summary>
        /// The primary data object for the details view.
        /// </summary>
        public DataObject DetailsObject;

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
                Errors = DetailsObject.Read(new DataObject.CrudOpions { PreserveSelection = preserveSelection });
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
                Errors = await DetailsObject.ReadAsync(new DataObject.CrudOpions { PreserveSelection = preserveSelection }, token);
            }
            catch (Exception ex)
            {
                Errors = ErrorParser.FromException(ex);
            }
        }

        /// <summary>
        /// Default handler for saving or deleting of a child details view.
        /// </summary>
        /// <param name="childViewModel">Child view model that fired the original event</param>
        /// <param name="e">Event object</param>
        protected override void OnChildEvent(object childViewModel, ViewEvent e)
        {
            // ignore events from grandchildren
            if (e.IsSaved(false) || e.IsDeleted(false))
                LoadData(true); // reload child lists if a child was updated

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
            // ignore events from grandchildren
            if (e.IsSaved(false) || e.IsDeleted(false))
                await LoadDataAsync(true, token); // reload child lists if a child was updated

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