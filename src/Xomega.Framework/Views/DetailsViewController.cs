// Copyright (c) 2016 Xomega.Net. All rights reserved.

using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base class for controllers of details views
    /// </summary>
    public abstract class DetailsViewController : ViewController
    {
        #region Initialization/Activation

        /// <summary>
        /// Constructs a new details view controller
        /// </summary>
        /// <param name="svcProvider">Service provider for the controller</param>
        public DetailsViewController(IServiceProvider svcProvider) : base(svcProvider)
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
        /// The name of the IsNew observable property
        /// </summary>
        public const string IsNewProperty = "IsNew";

        private bool isNew;

        /// <summary>
        /// An indicator if the object is new and not yet saved
        /// </summary>
        public bool IsNew {
            get { return isNew; }
            set
            {
                isNew = value;
                OnPropertyChanged(new PropertyChangedEventArgs(IsNewProperty));
            }
        }

        /// <summary>
        /// Main function to load details data
        /// </summary>
        public abstract void LoadData();

        /// <summary>
        /// Default handler for saving or deleting of a child details view.
        /// </summary>
        /// <param name="childController">Child view controller that fired the original event</param>
        /// <param name="e">Event object</param>
        protected override void OnChildEvent(object childController, ViewEvent e)
        {
            if (e.IsSaved() || e.IsDeleted())
                LoadData(); // reload child lists
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
            return !IsNew;
        }

        #endregion
    }
}