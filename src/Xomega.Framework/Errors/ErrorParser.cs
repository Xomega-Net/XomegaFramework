// Copyright (c) 2020 Xomega.Net. All rights reserved.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Resources;
using System.Runtime.Serialization;

namespace Xomega.Framework
{
    /// <summary>
    /// Customizable class for parsing a list of errors from an exception
    /// </summary>
    public class ErrorParser
    {
        /// <summary>
        /// Service provider for looking up services, such as a resource manager or a logger.
        /// </summary>
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// A flag indicating whether or not to return full exception details in the error list.
        /// </summary>
        protected readonly bool fullException;

        /// <summary>
        /// Constructs a new error parser.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="fullException">Whether or not to return full exception details in the error list.</param>
        public ErrorParser(IServiceProvider serviceProvider, bool fullException)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.fullException = fullException;
        }

        /// <summary>
        /// Retrieves the error list from the specified exception if possible,
        /// otherwise constructs a new error list with the exception as the error message.
        /// </summary>
        /// <param name="ex">Exception to retrieve the error list from.</param>
        /// <param name="logger">The logger to use for logging unhandled exceptions.</param>
        /// <returns>An error list retrieved from the exception.</returns>
        public virtual ErrorList FromException(Exception ex, ILogger logger = null)
        {
            // check if exception is ErrorAbortException first
            if (ex is ErrorAbortException ea) return ea.Errors;

            // check for a web exception with the error list in the response
            WebException webEx = ex as WebException;
            webEx = webEx ?? ex.InnerException as WebException;
            if (webEx != null && webEx.Response != null && webEx.Response.GetResponseStream() != null)
            {
                try
                {
                    return (ErrorList)new DataContractSerializer(typeof(ErrorList)).ReadObject(
                        webEx.Response.GetResponseStream());
                }
                catch (Exception) { }
            }
            
            LogException(ex, logger ?? serviceProvider.GetService<ILogger<ErrorParser>>());

            // construct a new error list from exception string
            return GetExceptionErrorList(ex.ToString());
        }

        /// <summary>
        /// Logs the exception using the specified logger. Subclasses can override the message.
        /// </summary>
        /// <param name="ex">Exception to log.</param>
        /// <param name="logger">Logger to use.</param>
        protected virtual void LogException(Exception ex, ILogger logger)
        {
            if (logger != null) logger.LogError(ex, "Unhandled Exception");
        }

        /// <summary>
        /// Helper method to construct an error list for the given exception messsage,
        /// taking into account parser's configuration for returning full exception details.
        /// </summary>
        /// <param name="exceptionMessage">Full exception message for the error list.</param>
        /// <returns>An error list with a single error for the exception.</returns>
        protected ErrorList GetExceptionErrorList(string exceptionMessage)
        {
            var resources = serviceProvider.GetService<ResourceManager>();
            ErrorList errList = new ErrorList(resources);
            errList.Add(new ErrorMessage(
                ErrorType.System,
                Messages.Exception_Unhandled,
                fullException? exceptionMessage : errList.GetMessage(Messages.Exception_Unhandled),
                ErrorSeverity.Critical));
            return errList;
        }
    }
}
