// Copyright (c) 2016 Xomega.Net. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base class for controllers of details views
    /// </summary>
    public abstract class DetailsViewController : ViewController
    {
        #region Initialization/Activation

        /// <summary>
        /// Constructs a new controller for the given search view
        /// </summary>
        /// <param name="svcProvider">Service provider for the controller</param>
        /// <param name="view">Details view associated with the controller</param>
        public DetailsViewController(IServiceProvider svcProvider, IDetailsView view) : base(svcProvider, view)
        {
        }

        /// <summary>
        /// Activates the view controller and the view
        /// </summary>
        /// <param name="parameters">Parameters to activate the view with</param>
        /// <returns>True if the view was successfully activated, False otherwise</returns>
        public override bool Activate(NameValueCollection parameters)
        {
            if (!base.Activate(parameters)) return false;

            IsNew = ViewParams.Action.Create == parameters[ViewParams.Action.Param];

            if (DetailsObject != null) DetailsObject.SetValues(parameters);

            if (!IsNew) LoadData();
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
        /// Wrapper around the corresponding IDetailsView.IsNew
        /// </summary>
        protected bool IsNew {
            get { return ((IDetailsView)View).IsNew; }
            set { ((IDetailsView)View).IsNew = value;  }
        }

        /// <summary>
        /// Main function to load details data
        /// </summary>
        public abstract void LoadData();

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

        #region Event handling

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
        protected void OnSaved(EventArgs e)
        {
            Saved?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the Deleted event
        /// </summary>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        protected void OnDeleted(EventArgs e)
        {
            Deleted?.Invoke(this, e);
        }

        /// <summary>
        /// Handler for saving the current view
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        public virtual void Save(object sender, EventArgs e)
        {
            // implemented in subclasses
        }

        /// <summary>
        /// A function that determines if the current object can be saved
        /// </summary>
        /// <returns></returns>
        public virtual bool CanSave()
        {
            return true;
        }

        /// <summary>
        /// Handler for deleting the objejct displayed in the current view
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        public virtual void Delete(object sender, EventArgs e)
        {
            // implemented in subclasses
        }

        /// <summary>
        /// A function that determines if the current object can be deleted
        /// </summary>
        /// <returns></returns>
        public virtual bool CanDelete()
        {
            IDetailsView dtlView = View as IDetailsView;
            return dtlView != null && !dtlView.IsNew;
        }

        #endregion
    }
}