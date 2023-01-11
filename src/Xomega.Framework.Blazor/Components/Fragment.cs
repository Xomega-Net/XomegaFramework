// Copyright (c) 2022 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Xomega.Framework.Blazor.Components
{
	/// <summary>
	/// Fragment component that can be updated from the outside.
	/// </summary>
	public class Fragment : ComponentBase
	{
		/// <summary>
		/// The content of the fragment
		/// </summary>
		[Parameter] public RenderFragment ChildContent { get; set; }

		/// <inheritdoc/>
		protected override void BuildRenderTree(RenderTreeBuilder builder) => ChildContent(builder);

		/// <summary>
		/// Updates the fragment component.
		/// </summary>
		public async Task Update() => await InvokeAsync(() => StateHasChanged());
	}
}