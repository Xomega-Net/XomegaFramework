// Copyright (c) 2025 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Xomega.Framework.Client;

namespace Xomega.Framework.Blazor;

/// <summary>
/// AuthenticationStateProvider that sources the authentication state from persisted principal data.
/// </summary>
/// <typeparam name="T">The type of DTO for persisted principal data.</typeparam>
public class PersistedAuthStateProvider<T> : PrincipalAuthStateProvider
{
    /// <summary>
    /// Constructs a new PersistedAuthStateProvider using DI.
    /// </summary>
    /// <param name="state">Injected persisted component state.</param>
    /// <param name="principalConverter">Injected principal converter.</param>
    public PersistedAuthStateProvider(PersistentComponentState state, IPrincipalConverter<T> principalConverter)
    {
        if (!state.TryTakeFromJson<T>(nameof(T), out var principalData) || principalData is null)
        {
            return;
        }

        CurrentPrincipal = principalConverter?.ToPrincipal(principalData);
    }
}
