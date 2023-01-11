// Copyright (c) 2023 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Xomega.Framework.Blazor
{
    /// <summary>
    /// Base class for components that are bound to action properties, such as action buttons.
    /// </summary>
    public class XActionComponent : XComponent
    {
        /// <summary>
        /// The action property that this action component is bound to.
        /// </summary>
        [Parameter] public ActionProperty Action { get => property as ActionProperty; set => property = value; }

        /// <summary>
        /// Indicates if this is a primary action.
        /// </summary>
        [Parameter] public bool IsPrimary { get; set; }

        /// <summary>
        /// Specifies if this action should be displayed using outline.
        /// </summary>
        [Parameter] public bool Outline { get; set; }

        /// <summary>
        /// Specifies if this action component should have no text, e.g. icon only.
        /// </summary>
        [Parameter] public bool NoText { get; set; }

        /// <summary>
        /// Event callback for when the action component is clicked on.
        /// </summary>
        [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

        /// <summary>
        /// Shorthand to determine if the action is enabled.
        /// </summary>
        public bool IsEnabed => Action != null && Action.Enabled;

        /// <summary>
        /// Constructs a new action component.
        /// </summary>
        public XActionComponent()
        {
            ObservedChanges = PropertyChange.Editable + PropertyChange.Visible;
        }
    }
}
