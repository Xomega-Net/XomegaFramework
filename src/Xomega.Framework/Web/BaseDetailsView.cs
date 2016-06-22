// Copyright (c) 2016 Xomega.Net. All rights reserved.

using System;
using System.Collections.Specialized;
using System.Web.UI;

namespace Xomega.Framework.Web
{
    /// <summary>
    /// Base class for object details view with CRUD operations
    /// </summary>
    public abstract class BaseDetailsView : BaseView
    {
        #region Data Object

        /// <summary>
        /// The primary data object for the details view.
        /// </summary>
        protected DataObject dataObj;

        /// <summary>
        /// Retrieves the data object possibly creating a new one along the way.
        /// </summary>
        /// <typeparam name="T">Data object type</typeparam>
        /// <param name="createNew">Whether or not to create a new data object</param>
        /// <returns>The data object for the view</returns>
        protected virtual T GetObject<T>(bool createNew) where T : class, new()
        {
            return WebUtil.GetCachedObject<T>(UniqueID + Request.CurrentExecutionFilePath, createNew);
        }

        #endregion

        #region Binding

        /// <summary>
        /// Main panel for the object
        /// </summary>
        protected Control pnl_Object;

        /// <summary>
        /// Binds object panel to the current object
        /// </summary>
        protected override void BindObjects()
        {
            if (dataObj != null && pnl_Object != null)
            {
                pnl_Object.DataBind();
                WebUtil.BindToObject(pnl_Object, dataObj);
            }
        }

        #endregion

        #region Initialization/Activation

        /// <summary>
        /// Activates details view and populates properties from parameters
        /// </summary>
        /// <param name="query">Parameters to activate the view with</param>
        public override void Activate(NameValueCollection query)
        {
            IsNew = ActionCreate.Equals(query[QueryAction]);

            InitObjects(true);
            foreach (string p in query.Keys)
                if (dataObj != null && dataObj.HasProperty(p))
                    dataObj[p].SetValue(query[p]);

            if (!IsNew) LoadData();
            BindObjects();
        }

        #endregion

        #region State

        /// <summary>
        /// An indicator if the object is new and not yet saved
        /// </summary>
        protected bool IsNew
        {
            get
            {
                object val = ViewState["IsNew"];
                return val != null && (bool)val;
            }
            set { ViewState["IsNew"] = value; }
        }

        /// <summary>
        /// Occurs when the details object is successfully saved
        /// </summary>
        public event EventHandler Saved;

        /// <summary>
        /// Occurs when the details object is successfully deleted
        /// </summary>
        public event EventHandler Deleted;

        /// <summary>
        /// Raises the Saved event
        /// </summary>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        protected virtual void OnSaved(EventArgs e)
        {
            if (Saved != null) Saved(this, e);
        }

        /// <summary>
        /// Raises the Deleted event
        /// </summary>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        protected virtual void OnDeleted(EventArgs e)
        {
            if (Deleted != null) Deleted(this, e);
        }

        #endregion

        #region Data loading

        /// <summary>
        /// Main function to load details data
        /// </summary>
        protected abstract void LoadData();

        /// <summary>
        /// Default handler for saving or deleting of a child details view.
        /// </summary>
        /// <param name="obj">View being saved or deleted</param>
        /// <param name="e">Event arguments</param>
        protected override void OnChildChanged(object obj, EventArgs e)
        {
            LoadData();
        }

        #endregion
    }
}
