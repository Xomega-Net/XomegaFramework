// Copyright (c) 2016 Xomega.Net. All rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;

namespace Xomega.Framework.Views
{
    /// <summary>
    /// Base class for WPF search views that may contain a results grid and a criteria panel
    /// </summary>
    public class WPFDetailsView : WPFView, IDetailsView
    {
        /// <summary>Button to delete the current object</summary>
        public virtual Button DeleteButton { get; }

        /// <summary>Button to save the view</summary>
        public virtual Button SaveButton { get; }

        /// <summary>Binds the view to its controller</summary>
        public override void BindTo(ViewController controller)
        {
            bool bind = controller != null;
            DetailsViewController dtlController = (bind ? controller : Controller) as DetailsViewController;
            if (dtlController != null)
            {
                if (SaveButton != null)
                {
                    SaveButton.Command = bind ? new RelayCommand<object>(
                        p => dtlController.Save(this, EventArgs.Empty),
                        p => dtlController.CanSave()) : null;
                }
                if (DeleteButton != null)
                {
                    DeleteButton.Command = bind ? new RelayCommand<object>(
                        p => dtlController.Delete(this, EventArgs.Empty),
                        p => dtlController.CanDelete()) : null;
                }
                BindDataObject(this, bind ? dtlController.DetailsObject : null);
            }
            base.BindTo(controller);
        }

        /// <summary>
        /// Checks if the view can be closed
        /// </summary>
        /// <returns>True if the view can be closed, False otherwise</returns>
        public override bool CanClose()
        {
            DetailsViewController dtlController = Controller as DetailsViewController;
            DataObject dtlObj = (dtlController != null) ? dtlController.DetailsObject : null;
            bool? modified = dtlObj != null ? dtlObj.IsModified() : null;
            if (modified != null && modified.Value && MessageBox.Show(
                    "You have unsaved changes. Do you want to discard them and close the window?",
                    "Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                    MessageBoxResult.No) == MessageBoxResult.No)
                return false;

            return base.CanClose();
        }

        /// <summary>
        /// An indicator if the object is new and not yet saved
        /// </summary>
        public bool IsNew { get; set; }
    }
}
