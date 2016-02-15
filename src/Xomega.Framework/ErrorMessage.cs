// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System.Runtime.Serialization;

namespace Xomega.Framework
{
    /// <summary>
    /// An error message that consists of an error code, a text message and the severity.
    /// Error messages are typically added to an error list and can be serialized
    /// to allow sending them in a service call.
    /// </summary>
    [DataContract]
    public class ErrorMessage
    {
        /// <summary>
        /// Default constructor to support deserialization.
        /// </summary>
        protected ErrorMessage() { }
        
        /// <summary>
        /// Constructs an error with a given error code and message.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        public ErrorMessage(string code, string message)
            : this(code, message, ErrorSeverity.Error)
        {
        }

        /// <summary>
        /// Constructs an error message with a given code, message and severity.
        /// </summary>
        /// <param name="code">The error message code.</param>
        /// <param name="message">The text message.</param>
        /// <param name="sev">The error message severity.</param>
        public ErrorMessage(string code, string message, ErrorSeverity sev)
        {
            this.Code = code;
            this.Message = message;
            this.Severity = sev;
        }

        /// <summary>
        /// Error code, which is an error identifier.
        /// </summary>
        [DataMember]
        public string Code { get; set; }

        /// <summary>
        /// Full error message text in the current language.
        /// </summary>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// Error severity, which may affect the execution flow.
        /// </summary>
        [DataMember]
        public ErrorSeverity Severity { get; set; }
    }

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
