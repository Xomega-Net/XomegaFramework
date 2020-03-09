// Copyright (c) 2020 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Threading.Tasks;

namespace Xomega.Framework.Blazor.Components
{
    /// <summary>
    /// Component that displays title and the modification state of the view as an asterisk next to it.
    /// </summary>
    public class ViewTitle : ComponentBase
    {
        /// <summary>
        /// The base title of the view.
        /// </summary>
        [Parameter]
        public string Title { get; set; }

        /// <summary>
        /// Whether or not the view is modified.
        /// </summary>
        [Parameter]
        public bool IsModified { get; set; }

        /// <inheritdoc/>
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.AddContent(0, Title);
            if (IsModified)
                builder.AddContent(1, "*");
        }

        /// <summary>
        /// Sets the title component's modification state.
        /// </summary>
        /// <param name="modified">True if modified, false otherwise.</param>
        /// <returns>A task for the function.</returns>
        public async Task SetModified(bool modified)
        {
            IsModified = modified;
            await InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }
    }
}
