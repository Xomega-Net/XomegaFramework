// Copyright (c) 2024 Xomega.Net. All rights reserved.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xomega.Framework.Client;

namespace Xomega.Framework.Services;

/// <summary>
/// A server-side AuthenticationStateProvider that uses PersistentComponentState to flow 
/// the authentication state to the client, which is then fixed for the lifetime of the WebAssembly application.
/// It also allows revalidating the connected user every 30 minutes an interactive circuit is connected.
/// </summary>
/// <typeparam name="T">The type for the persistent principal DTO.</typeparam>
public class PersistingAuthStateProvider<T> : RevalidatingServerAuthenticationStateProvider
{
    private readonly PersistentComponentState state;
    private readonly IPrincipalConverter<T> principalConverter;
    private readonly PersistingComponentStateSubscription subscription;

    private Task<AuthenticationState> authenticationStateTask;

    /// <summary>
    /// Constructs a new PersistingAuthStateProvider implementation using DI.
    /// </summary>
    /// <param name="loggerFactory">Injected logger factory.</param>
    /// <param name="persistentComponentState">Injected persistent component state.</param>
    /// <param name="principalConverter">Injected principal converter.</param>
    public PersistingAuthStateProvider(ILoggerFactory loggerFactory,
        PersistentComponentState persistentComponentState,
        IPrincipalConverter<T> principalConverter) : base(loggerFactory)
    {
        state = persistentComponentState;
        this.principalConverter = principalConverter;
        AuthenticationStateChanged += OnAuthenticationStateChanged;
        subscription = state.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveWebAssembly);
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        authenticationStateTask = task;
    }

    private async Task OnPersistingAsync()
    {
        if (authenticationStateTask is null)
        {
            throw new UnreachableException($"Authentication state not set in {nameof(OnPersistingAsync)}().");
        }

        var authenticationState = await authenticationStateTask;
        var principal = authenticationState.User;

        if (principal.Identity?.IsAuthenticated == true)
        {
            var userInfo = principalConverter.FromPrincipal(principal);
            if (userInfo != null)
                state.PersistAsJson(nameof(T), userInfo);
        }
    }

    /// <inheritdoc />
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    /// <inheritdoc />
    protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        return await Task.FromResult(true); // no validation by default, but subclasses can override this method to implement it
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        subscription.Dispose();
        AuthenticationStateChanged -= OnAuthenticationStateChanged;
        base.Dispose(disposing);
    }
}
