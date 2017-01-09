// Copyright (c) 2016 Xomega.Net. All rights reserved.

using System;
using System.ComponentModel;
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

        private string objKey { get { return UniqueID + Request.CurrentExecutionFilePath; } }

        private DataObject dataObj
        {
            get { return Session[objKey] as DataObject; }
            set
            {
                if (value == null) Session.Remove(objKey);
                Session[objKey] = value;
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

        /// <summary>
        /// Sets the state of the buttons after callbacks before rendering
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            DetailsViewModel dvm = Model as DetailsViewModel;
            if (dvm == null) return;
            if (btn_Save != null) btn_Save.Enabled = dvm.SaveEnabled();
            if (btn_Delete != null) btn_Delete.Enabled = dvm.DeleteEnabled();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the already constructed view model for each request
        /// </summary>
        /// <param name="e">Standard event arguments</param>
        protected override void OnLoad(EventArgs e)
        {
            DetailsViewModel dvm = Model as DetailsViewModel;
            if (dvm != null)
            {
                DataObject obj = dataObj;
                if (obj != null) dvm.DetailsObject = obj;
                if (IsNew != null) dvm.IsNew = IsNew.Value;
            }
            base.OnLoad(e);
        }

        #endregion

        #region Binding

        /// <summary>
        /// Binds the view to its model, or unbinds the current model if null is passed.
        /// </summary>
        /// <param name="viewModel">Model to bind the view to</param>
        public override void BindTo(ViewModel viewModel)
        {
            bool bind = viewModel != null;
            DetailsViewModel dvm = (bind ? viewModel : this.Model) as DetailsViewModel;
            if (dvm != null)
            {
                if (btn_Save != null)
                {
                    if (bind) btn_Save.Click += dvm.Save;
                    else btn_Save.Click -= dvm.Save;
                }
                if (btn_Delete != null)
                {
                    if (bind) btn_Delete.Click += dvm.Delete;
                    else btn_Delete.Click -= dvm.Delete;
                }

                OnModelPropertyChanged(dvm, new PropertyChangedEventArgs(DetailsViewModel.IsNewProperty));

                if (dvm.DetailsObject != null && pnl_Object != null)
                {
                    pnl_Object.DataBind();
                    WebPropertyBinding.BindToObject(pnl_Object, bind ? dvm.DetailsObject : null);
                }
                if (bind) dataObj = dvm.DetailsObject; // persist the object in session
            }
            base.BindTo(viewModel);
        }

        /// <summary>
        /// Handles IsNew property change to update the corresponding view state field
        /// </summary>
        /// <param name="sender">Model that sent the event</param>
        /// <param name="e">Event arguments</param>
        protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnModelPropertyChanged(sender, e);
            DetailsViewModel dvm = sender as DetailsViewModel;
            if (dvm != null && DetailsViewModel.IsNewProperty.Equals(e.PropertyName))
            {
                IsNew = dvm.IsNew;
            }
        }

        #endregion

        #region State

        /// <summary>
        /// An indicator if the object is new and not yet saved
        /// </summary>
        protected bool? IsNew
        {
            get { return (bool?)ViewState["IsNew"]; }
            set { ViewState["IsNew"] = value; }
        }

        #endregion
    }
}
