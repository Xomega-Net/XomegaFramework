// Copyright (c) 2025 Xomega.Net. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Xomega.Framework.Client
{
    /// <summary>
    /// Authorization token structure that consists of an access token and a refresh token.
    /// </summary>
    public class AuthToken
    {
        /// <summary>
        /// A token used to access secured REST API endpoints.
        /// </summary>
        [Required]
        public string AccessToken { get; set; }

        /// <summary>
        /// A token used to refresh an expired access token.
        /// </summary>
        [Required]
        public string RefreshToken { get; set; }
    }
}