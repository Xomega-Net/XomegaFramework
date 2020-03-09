// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System.Net;
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
            : this(ErrorType.System, code, message, ErrorSeverity.Error)
        {
        }

        /// <summary>
        /// Constructs an error message with a given code, message and severity.
        /// </summary>
        /// <param name="type">The type of error.</param>
        /// <param name="code">The error message code.</param>
        /// <param name="message">The text message.</param>
        /// <param name="sev">The error message severity.</param>
        public ErrorMessage(ErrorType type, string code, string message, ErrorSeverity sev)
        {
            Code = code;
            Message = message;
            Severity = sev;
            Type = type;
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

        /// <summary>
        /// Error type indicating the origin of the error.
        /// </summary>
        [DataMember]
        public ErrorType Type { get; set; }

        // explicitly set HTTP status code
        private HttpStatusCode? httpStatus;

        /// <summary>
        /// HTTP status code associated with the current error for REST services
        /// </summary>
        public HttpStatusCode HttpStatus
        {
            get
            {
                if (httpStatus != null) return httpStatus.Value;
                if (Severity < ErrorSeverity.Error)
                    return HttpStatusCode.OK;
                // for errors use the error type to default the status code
                switch (Type)
                {
                    case ErrorType.Functional:
                    case ErrorType.Validation:
                        return HttpStatusCode.BadRequest;
                    case ErrorType.Security:
                        return HttpStatusCode.Forbidden;
                    case ErrorType.Concurrency:
                        return HttpStatusCode.Conflict;
                    case ErrorType.External:
                        return HttpStatusCode.BadGateway;
                    case ErrorType.Data:
                        return HttpStatusCode.NotFound;
                    case ErrorType.System:
                    default:
                        return HttpStatusCode.InternalServerError;
                }
            }
            set { httpStatus = value; }
        }
    }
}
