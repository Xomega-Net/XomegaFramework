// Copyright (c) 2021 Xomega.Net. All rights reserved.

namespace Xomega.Framework
{
    /// <summary>
    /// Error type indicating the origin of the error
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// Error resulting from concurrency checks.
        /// </summary>
        Concurrency,

        /// <summary>
        /// Error resulting from a data issue.
        /// </summary>
        Data,

        /// <summary>
        /// Error originated from an external system.
        /// </summary>
        External,

        /// <summary>
        /// Error resulting from security validation.
        /// </summary>
        Security,

        /// <summary>
        /// Internal system error.
        /// </summary>
        System,

        /// <summary>
        /// Error resulting from request validation.
        /// </summary>
        Validation,

        /// <summary>
        /// Error resulting from functional validation.
        /// </summary>
        Functional,

        /// <summary>
        /// Error contains a message or a warning.
        /// </summary>
        Message
    }
}
