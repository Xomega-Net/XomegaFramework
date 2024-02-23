// Copyright (c) 2024 Xomega.Net. All rights reserved.

using System.Security.Claims;

namespace Xomega.Framework.Client
{
    /// <summary>
    /// An interface for converting the principal to and from the specified type
    /// in order to persist it in the component state for WebAssembly.
    /// </summary>
    /// <typeparam name="T">The type used to persist the principal.</typeparam>
    public interface IPrincipalConverter<T>
    {
        /// <summary>
        /// Converts the specified principal to the persistent DTO.
        /// </summary>
        /// <param name="principal">The principal to convert.</param>
        /// <returns>A DTO with the principal's data to be persisted.</returns>
        T FromPrincipal(ClaimsPrincipal principal);

        /// <summary>
        /// Converts persisted principal data from the specified DTO to a claims principal.
        /// </summary>
        /// <param name="userInfo">A DTO with persisted principal data.</param>
        /// <returns>A claims principal from the persisted data.</returns>
        ClaimsPrincipal ToPrincipal(T userInfo);
    }
}