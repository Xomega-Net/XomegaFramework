// Copyright (c) 2020 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Threading.Tasks;

namespace Xomega.Framework.Blazor.Components
{
    /// <summary>
    /// Component that displays view title.
    /// </summary>
    public class ViewTitle : ComponentBase
    {
        /// <summary>
        /// The title of the view.
        /// </summary>
        [Parameter]
        public string Title { get; set; }

        /// <summary>
        /// The title of the view.
        /// </summary>
        public void SetTitle(string value)
        {
            Title = value;
        }

        /// <inheritdoc/>
        protected override void BuildRenderTree(RenderTreeBuilder builder) => builder.AddContent(0, Title);

        /// <summary>
        /// Updates the title component.
        /// </summary>
        public async Task Update() => await InvokeAsync(() => StateHasChanged());            
    }
}