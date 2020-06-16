using Microsoft.AspNetCore.Components.Authorization;
using System.Threading.Tasks;

namespace Xomega.Framework.Blazor
{
    /// <summary>
    /// Blazor implementation of the <see cref="IPrincipalProvider"/> interface that sources
    /// the current principal from the <see cref="AuthenticationStateProvider"/> service.
    /// </summary>
    public class AuthStatePrincipalProvider : DefaultPrincipalProvider
    {
        /// <summary>
        /// Constructs a new AuthStatePrincipalProvider with injected instance of the
        /// AuthenticationStateProvider service.
        /// </summary>
        /// <param name="authStateProvider">The AuthenticationStateProvider service to source the current principal from.</param>
        public AuthStatePrincipalProvider(AuthenticationStateProvider authStateProvider)
        {
            authStateProvider.AuthenticationStateChanged += UpdateCurrentPrincipal;
            UpdateCurrentPrincipal(authStateProvider.GetAuthenticationStateAsync());
        }

        /// <summary>
        /// Updates the current principal using the specified task for getting AuthenticationState.
        /// This method is invoked initially, and also whenever the authentication state is changed
        /// with the AuthenticationStateProvider.
        /// </summary>
        /// <param name="task"></param>
        protected virtual async void UpdateCurrentPrincipal(Task<AuthenticationState> task)
        {
            CurrentPrincipal = (await task).User;
        }
    }
}
