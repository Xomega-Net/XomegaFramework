// Copyright (c) 2023 Xomega.Net. All rights reserved.

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
        /// <summary>
        /// View model as a details view model.
        /// </summary>
        protected DetailsViewModel DetailsModel => Model as DetailsViewModel;

        /// <summary>
        /// Exposed details object from the details view model.
        /// </summary>
        protected DataObject DetailsObject => DetailsModel?.DetailsObject;

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

        /// <inheritdoc/>
        public override void BindTo(ViewModel viewModel)
        {
            base.BindTo(viewModel);
            BindDataObject(this, DetailsObject);
        }

        /// <summary>
        /// Checks if the view can be closed
        /// </summary>
        /// <returns>True if the view can be closed, False otherwise</returns>
        public override bool CanClose()
        {
            bool isModified = DetailsObject?.IsModified() ?? false;
            if (isModified && MessageBox.Show(
                    Model?.GetString(Messages.View_UnsavedMessage),
                    Model?.GetString(Messages.View_UnsavedTitle),
                    MessageBoxButton.YesNo, MessageBoxImage.Warning,
                    MessageBoxResult.No) == MessageBoxResult.No)
                return false;

            return base.CanClose();
        }

        #region Event handlers

        /// <summary>
        /// Default handler for saving the view delegating the action to the view model.
        /// </summary>
        protected virtual async void Save(object sender, EventArgs e)
        {
            if (Model is DetailsViewModel dvm)
            {
                // store enabled state of the save button to restore at the end
                // in case when a subclass overrides save and manages the button's state
                bool enabled = SaveButton?.IsEnabled ?? false;
                try
                {
                    if (SaveButton != null) // prevent double-clicks
                        SaveButton.IsEnabled = false;
                    if (IsAsync) await dvm.SaveAsync();
                    else dvm.Save(sender, e);
                }
                finally
                {
                    if (SaveButton != null)
                        SaveButton.IsEnabled = enabled;
                }
            }
        }

        /// <summary>
        /// Default handler for checking if save is enabled by delegating it to the view model.
        /// </summary>
        protected virtual bool SaveEnabled()
        {
            return (Model is DetailsViewModel dvm) ? dvm.SaveEnabled() : false;
        }

        /// <summary>
        /// Default handler for deleting the view delegating the action to the view model.
        /// </summary>
        protected virtual async void Delete(object sender, EventArgs e)
        {
            if (Model is DetailsViewModel dvm)
            {
                bool enabled = DeleteButton?.IsEnabled ?? false;
                try
                {
                    if (DeleteButton != null)
                        DeleteButton.IsEnabled = false;
                    // this will call CanDelete
                    if (IsAsync) await dvm.DeleteAsync();
                    else dvm.Delete(sender, e);

                }
                finally
                {
                    if (DeleteButton != null)
                        DeleteButton.IsEnabled = enabled;
                }
            }
        }

        /// <summary>
        /// Default handler for checking if delete is enabled by delegating it to the view model.
        /// </summary>
        protected virtual bool DeleteEnabled()
        {
            return (Model is DetailsViewModel dvm) ? dvm.DeleteEnabled() : false;
        }

        /// <summary>
        /// Checks if the view can be deleted
        /// </summary>
        /// <returns>True if the view can be deleted, False otherwise</returns>
        public override bool CanDelete()
        {
            return MessageBox.Show(
                Model?.GetString(Messages.View_DeleteMessage),
                Model?.GetString(Messages.View_DeleteTitle),
                MessageBoxButton.YesNo, MessageBoxImage.Warning,
                MessageBoxResult.No) == MessageBoxResult.Yes;
        }

        #endregion
    }
}
