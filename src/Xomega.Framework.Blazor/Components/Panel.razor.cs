// Copyright (c) 2023 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Xomega.Framework.Blazor.Components
{
    /// <summary>
    /// Base class for collapsible panel components.
    /// </summary>
    public partial class Panel : ComponentBase
    {
        /// <summary>
        /// Additional CSS class to use for the panel.
        /// </summary>
        [Parameter] public string Class { get; set; }

        /// <summary>
        /// The title of the panel.
        /// </summary>
        [Parameter] public string Title { get; set; }

        /// <summary>
        /// True, if the panel is collapsed, false if expanded.
        /// </summary>
        [Parameter] public bool Collapsed { get; set; }

        /// <summary>
        /// Event callback to call when the collapsed state changes.
        /// </summary>
        [Parameter] public EventCallback<bool> CollapsedChanged { get; set; }

        /// <summary>
        /// The content of the panel to display in the collapsible area.
        /// </summary>
        [Parameter] public RenderFragment ChildContent { get; set; }

        /// <summary>
        /// Event handler for toggling the collapsed state
        /// that can be used on the corresponding HTML element.
        /// </summary>
        /// <param name="args">Click arguments.</param>
        /// <returns>A task for the function.</returns>
        public async Task ToggleClicked(MouseEventArgs args)
        {
            Collapsed = !Collapsed;
            await CollapsedChanged.InvokeAsync(Collapsed);
        }
    }
}