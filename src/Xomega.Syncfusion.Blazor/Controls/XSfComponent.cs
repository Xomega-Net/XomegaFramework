// Copyright (c) 2021 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Popups;
using Xomega.Framework;
using Xomega.Framework.Blazor;

namespace Xomega._Syncfusion.Blazor
{
    /// <summary>
    /// Base component for Xomega Syncfusion controls
    /// </summary>
    public class XSfComponent : XComponent
    {
        /// <summary>
        /// Gets the type of float label to use, if any.
        /// </summary>
        public FloatLabelType FloatLabelType => ShowLabel ? FloatLabelType.Always : FloatLabelType.Never;

        /// <summary>
        /// Gets Syncfusion-specific classes for various property validation state.
        /// </summary>
        public static PropertyStateDescription GetStateDescriptions() =>
            new PropertyStateDescription()
            {
                Valid = "e-success",
                Warning = "e-warning",
                Invalid = "e-error"
            };

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            StateDescriptions = GetStateDescriptions();
        }

        /// <summary>
        /// Allows setting tooltip position for the validation error tooltip.
        /// </summary>
        [Parameter] public Position TooltipPosition { get; set; } = Position.BottomCenter;

        /// <summary>
        /// Global CSS class used for components' validation errors.
        /// </summary>
        public static string ErrorClass = "e-griderror";

        /// <summary>
        /// A tooltip handler that allows conditionally showing component's tooltip with errors as needed.
        /// </summary>
        /// <param name="args">Tooltip arguments.</param>
        protected void OnTooltipOpen(TooltipEventArgs args) => ShowTooltipError(this, args);

        /// <summary>
        /// A global function that prevents showing component's validation error tooltip,
        /// if there are no errors or if the component is in the grid, which shows it's own error.
        /// </summary>
        /// <param name="comp">Component with the error.</param>
        /// <param name="args">Tooltip arguments.</param>
        public static void ShowTooltipError(XComponent comp, TooltipEventArgs args)
        {
            // don't show error tooltip if there are no errors or when editing a grid, which shows its own error
            if (string.IsNullOrEmpty(comp.ErrorsText) || comp.Row != null)
                args.Cancel = true;
        }
    }
}
