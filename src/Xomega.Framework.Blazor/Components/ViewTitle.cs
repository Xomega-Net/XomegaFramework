// Copyright (c) 2025 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Xomega.Framework.Blazor.Components
{
	/// <summary>
	/// Component that displays the view title.
	/// </summary>
	public class ViewTitle : Fragment
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
    }
}