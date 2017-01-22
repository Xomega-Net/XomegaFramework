// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

namespace Xomega.Framework
{
    /// <summary>
    /// Error severity possible values.
    /// </summary>
    public enum ErrorSeverity
    {
        /// <summary>
        /// Information message that can be displayed to the user.
        /// </summary>
        Info,

        /// <summary>
        /// A warning that may be displayed to the user for the confirmation before proceeding,
        /// if supported by the current execution context.
        /// </summary>
        Warning,

        /// <summary>
        /// An error, that will be displayed to the user with the other errors. It doesn't stop
        /// the execution flow, but prevents the operation from successfully completing.
        /// </summary>
        Error,

        /// <summary>
        /// A critical error, which stops the execution immediately and returns a fault to the user.
        /// </summary>
        Critical
    }
}
