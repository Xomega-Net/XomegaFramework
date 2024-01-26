// Copyright (c) 2023 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Security.Principal;

namespace Xomega.Framework.Blazor;

/// <summary>
/// AuthenticationStateProvider that provides the authentication state from supplied principal data.
/// </summary>
public class PrincipalAuthStateProvider : AuthenticationStateProvider, IPrincipalProvider
{
    private ClaimsPrincipal currentPrincipal;

    /// <inheritdoc/>
    public IPrincipal CurrentPrincipal
    {
        get => currentPrincipal;
        set
        {
            currentPrincipal = value as ClaimsPrincipal;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }

    /// <inheritdoc/>
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var principal = currentPrincipal ?? new ClaimsPrincipal(new ClaimsIdentity());
        return Task.FromResult(new AuthenticationState(principal));
    }
}
