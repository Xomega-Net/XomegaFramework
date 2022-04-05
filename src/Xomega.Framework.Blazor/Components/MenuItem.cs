// Copyright (c) 2021 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;

namespace Xomega.Framework.Blazor.Components
{
    /// <summary>
    /// A data model for a hierarchical menu item on the screen, which supports security.
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        /// The text for the menu item, overriding any localized resources.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Resource key for the localized menu item's text.
        /// </summary>
        public string ResourceKey { get; set; }

        /// <summary>
        /// Hyperlink reference for the menu item.
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// Icon class for the menu item.
        /// </summary>
        public string IconClass { get; set; }

        /// <summary>
        /// A list of child menu items contained within the current item.
        /// </summary>
        public List<MenuItem> Items { get; set; }

        /// <summary>
        /// Authorization policy associated with the current menu item.
        /// </summary>
        public string Policy { get; set; }

        /// <summary>
        /// A list of user roles associated with the current menu item.
        /// </summary>
        public string[] Roles { get; set; }

        /// <summary>
        /// Returns authorization data as a standard <see cref="IAuthorizeData"/> interface,
        /// or null if no authorization data is specified for the menu item.
        /// </summary>
        public IAuthorizeData AuthorizeData => 
            Policy == null && Roles == null ? null : new AuthData(this);

        /// <summary>
        /// A helper method to run an action recursively against this item and all its descendants.
        /// Useful for setting up authorization data on the menus.
        /// </summary>
        /// <param name="action">The action to run.</param>
        public void ForEachItem(Action<MenuItem> action)
        {
            action?.Invoke(this);
            if (Items != null)
                foreach (var mi in Items)
                    mi.ForEachItem(action);
        }

        /// <summary>
        /// Adapter class to allow returning authorization data as a standard <see cref="IAuthorizeData"/> interface.
        /// </summary>
        public class AuthData : IAuthorizeData
        {
            /// <summary>
            ///  Constructs authorization data from the specified menu item.
            /// </summary>
            /// <param name="mi">Menu item with authorization data.</param>
            public AuthData(MenuItem mi)
            {
                Policy = mi.Policy;
                Roles = mi.Roles != null ? string.Join(",", mi.Roles) : null;
            }

            /// <inheritdoc/>
            public string AuthenticationSchemes { get; set; }

            /// <inheritdoc/>
            public string Policy { get; set; }

            /// <inheritdoc/>
            public string Roles { get; set; }
        }
    }
}
