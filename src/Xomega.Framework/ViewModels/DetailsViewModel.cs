// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base class for models of details views
    /// </summary>
    public abstract class DetailsViewModel : ViewModel
    {
        #region Initialization/Activation

        /// <summary>
        /// Constructs a new details view model
        /// </summary>
        /// <param name="svcProvider">Service provider for the model</param>
        public DetailsViewModel(IServiceProvider svcProvider) : base(svcProvider)
        {
        }

        /// <summary>
        /// Activates the view model and the view
        /// </summary>
        /// <param name="parameters">Parameters to activate the view with</param>
        /// <returns>True if the view was successfully activated, False otherwise</returns>
        public override bool Activate(NameValueCollection parameters)
        {
            if (!base.Activate(parameters)) return false;

            IsNew = ViewParams.Action.Create == Params[ViewParams.Action.Param];

            if (DetailsObject != null) DetailsObject.SetValues(Params);

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

        private bool isNew = true;

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
        public virtual void LoadData()
        {
            // subclasses can implement it to load the data as needed
        }

        /// <summary>
        /// Default handler for saving or deleting of a child details view.
        /// </summary>
        /// <param name="childViewModel">Child view model that fired the original event</param>
        /// <param name="e">Event object</param>
        protected override void OnChildEvent(object childViewModel, ViewEvent e)
        {
            // ignore events from grandchildren
            if (!e.IsChild() && (e.IsSaved() || e.IsDeleted()))
                LoadData(); // reload child lists if a child was updated

            base.OnChildEvent(childViewModel, e);
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
        public virtual bool SaveEnabled()
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
        public virtual bool DeleteEnabled()
        {
            return !IsNew;
        }

        #endregion
    }
}