// Copyright (c) 2019 Xomega.Net. All rights reserved.

using System;

namespace Xomega.Framework
{
    /// <summary>
    /// Exception thrown when operation is aborted due to severity of the current errors.
    /// </summary>
    public class ErrorAbortException : Exception
    {
        /// <summary>
        /// The list of current errors that caused the exception
        /// </summary>
        public ErrorList Errors { get; set; }

        /// <summary>
        /// Constructs a new exception from the current list of errors
        /// </summary>
        /// <param name="message">Excetpion message</param>
        /// <param name="errors">The current list of errors</param>
        public ErrorAbortException(String message, ErrorList errors) : base(message)
        {
            Errors = errors;
        }
    }
}
