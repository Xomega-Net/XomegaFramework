// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base class for WPF search views that may contain a results grid and a criteria panel
    /// </summary>
    public class WPFDetailsView : WPFView
    {
        /// <summary>Button to delete the current object</summary>
        protected virtual Button DeleteButton { get; }

        /// <summary>Button to save the view</summary>
        protected virtual Button SaveButton { get; }

        /// <summary>
        /// Configures button commands after initialization
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            if (SaveButton != null)
            {
                SaveButton.Command = new RelayCommand<object>(
                    p => Save(this, EventArgs.Empty),
                    p => SaveEnabled());
            }
            if (DeleteButton != null)
            {
                DeleteButton.Command = new RelayCommand<object>(
                    p => Delete(this, EventArgs.Empty),
                    p => DeleteEnabled());
            }
        }

        /// <summary>
        /// Binds the view to its model, or unbinds the current model if null is passed.
        /// </summary>
        public override void BindTo(ViewModel viewModel)
        {
            bool bind = viewModel != null;
            DetailsViewModel dvm = (bind ? viewModel : Model) as DetailsViewModel;
            if (dvm != null)
            {
                BindDataObject(this, bind ? dvm.DetailsObject : null);
            }
            base.BindTo(viewModel);
        }

        /// <summary>
        /// Checks if the view can be closed
        /// </summary>
        /// <returns>True if the view can be closed, False otherwise</returns>
        public override bool CanClose()
        {
            DetailsViewModel dvm = Model as DetailsViewModel;
            DataObject dtlObj = (dvm != null) ? dvm.DetailsObject : null;
            bool? modified = dtlObj != null ? dtlObj.IsModified() : null;
            if (modified != null && modified.Value && MessageBox.Show(
                    "You have unsaved changes. Do you want to discard them and close the window?",
                    "Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                    MessageBoxResult.No) == MessageBoxResult.No)
                return false;

            return base.CanClose();
        }

        #region Event handlers

        /// <summary>
        /// Default handler for saving the view delegating the action to the view model.
        /// </summary>
        protected virtual void Save(object sender, EventArgs e)
        {
            DetailsViewModel dvm = Model as DetailsViewModel;
            if (dvm != null) dvm.Save(sender, e);
        }

        /// <summary>
        /// Default handler for checking if save is enabled by delegating it to the view model.
        /// </summary>
        protected virtual bool SaveEnabled()
        {
            DetailsViewModel dvm = Model as DetailsViewModel;
            return (dvm != null) ? dvm.SaveEnabled() : false;
        }

        /// <summary>
        /// Default handler for deleting the view delegating the action to the view model.
        /// </summary>
        protected virtual void Delete(object sender, EventArgs e)
        {
            DetailsViewModel dvm = Model as DetailsViewModel;
            if (dvm != null) dvm.Delete(sender, e); // this will call CanDelete
        }

        /// <summary>
        /// Default handler for checking if delete is enabled by delegating it to the view model.
        /// </summary>
        protected virtual bool DeleteEnabled()
        {
            DetailsViewModel dvm = Model as DetailsViewModel;
            return (dvm != null) ? dvm.DeleteEnabled() : false;
        }

        /// <summary>
        /// Checks if the view can be deleted
        /// </summary>
        /// <returns>True if the view can be deleted, False otherwise</returns>
        public override bool CanDelete()
        {
            return MessageBox.Show(
                "Are you sure you want to delete this object?\nThis action cannot be undone.",
                "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                MessageBoxResult.No) == MessageBoxResult.Yes;
        }

        #endregion
    }
}
