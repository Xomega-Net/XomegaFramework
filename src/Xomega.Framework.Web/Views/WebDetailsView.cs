// Copyright (c) 2021 Xomega.Net. All rights reserved.

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Xomega.Framework.Views;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// Base class for object details view with CRUD operations
    /// </summary>
    public abstract class WebDetailsView : WebView
    {
        #region Data Object

        private string ObjKey { get { return GetSessionKey("dataObj"); } }

        private DataObject DataObj
        {
            get { return Session[ObjKey] as DataObject; }
            set
            {
                if (value == null) Session.Remove(ObjKey);
                Session[ObjKey] = value;
            }
        }

        #endregion

        #region Controls

        /// <summary>
        /// Main panel for the object
        /// </summary>
        protected Control pnl_Object;

        /// <summary>
        /// Save button
        /// </summary>
        protected Button btn_Save;

        /// <summary>
        /// Delete button
        /// </summary>
        protected Button btn_Delete;

        /// <inheritdoc/>
        protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnModelPropertyChanged(sender, e);

            // We really want to listen for changes in IsNew on the data object to update the Delete button's state.
            // But since the view model doesn't have such property, we piggyback off of the ViewTitle property,
            // which also depends on the IsNew value.
            if (e.PropertyName == ViewModel.ViewTitleProperty && sender is DetailsViewModel dvm)
            {
                if (btn_Save != null) btn_Save.Enabled = dvm.SaveEnabled();
                if (btn_Delete != null) btn_Delete.Enabled = dvm.DeleteEnabled();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the already constructed view model for each request
        /// </summary>
        /// <param name="e">Standard event arguments</param>
        protected override void OnLoad(EventArgs e)
        {
            // restore view model state
            if (Model is DetailsViewModel dvm)
            {
                DataObject obj = DataObj;
                if (obj != null) dvm.DetailsObject = obj;
            }

            // wire up event handlers for buttons
            if (btn_Save != null)
                btn_Save.Click += Save;
            if (btn_Delete != null)
            {
                btn_Delete.Click += Delete;
                var msg = Model.GetString(Messages.View_DeleteMessage).Replace(Environment.NewLine, "\\n");
                btn_Delete.OnClientClick = $"if (!confirm('{msg}')) return false;";
            }

            base.OnLoad(e);
        }

        #endregion

        #region Binding

        /// <inheritdoc/>
        public override void BindTo(ViewModel viewModel)
        {
            bool bind = viewModel != null;
            if ((viewModel ?? Model) is DetailsViewModel dvm)
            {
                if (dvm.DetailsObject != null && pnl_Object != null)
                {
                    pnl_Object.DataBind();
                    WebPropertyBinding.BindToObject(pnl_Object, bind ? dvm.DetailsObject : null);
                }
                if (bind) DataObj = dvm.DetailsObject; // persist the object in session
                else dvm.DetailsObject = null; // detach view model from persisted object
            }
            base.BindTo(viewModel);
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// Default handler for saving the view delegating the action to the view model.
        /// </summary>
        protected virtual void Save(object sender, EventArgs e)
        {
            if (Page.IsAsync)
                Page.RegisterAsyncTask(new PageAsyncTask(async (ct) => await SaveAsync(ct)));
            else if (Model is DetailsViewModel dvm)
                dvm.Save(sender, e);
        }

        /// <summary>
        /// Async handler for saving the view delegating the action to the view model.
        /// </summary>
        protected virtual async Task SaveAsync(CancellationToken token = default)
        {
            if (Model is DetailsViewModel dvm)
                await dvm.SaveAsync(token);
        }

        /// <summary>
        /// Default handler for deleting the view delegating the action to the view model.
        /// </summary>
        protected virtual void Delete(object sender, EventArgs e)
        {
            if (Page.IsAsync)
                Page.RegisterAsyncTask(new PageAsyncTask(async (ct) => await DeleteAsync(ct)));
            else if (Model is DetailsViewModel dvm)
                dvm.Delete(sender, e);
        }

        /// <summary>
        /// Async handler for deleting the view delegating the action to the view model.
        /// </summary>
        protected virtual async Task DeleteAsync(CancellationToken token = default)
        {
            if (Model is DetailsViewModel dvm)
                await dvm.DeleteAsync(token);
        }

        #endregion
    }
}