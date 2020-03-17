// Copyright (c) 2020 Xomega.Net. All rights reserved.

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
        /// A resource manager, which can be used to translate the error messages to the current language.
        /// </summary>
        private readonly ResourceManager resources;

        /// <summary>
        /// A flag indicating whether or not to return full exception details in the error list.
        /// </summary>
        protected readonly bool fullException;

        /// <summary>
        /// Constructs a new error parser.
        /// </summary>
        /// <param name="resources">Resource manager to use for localization.</param>
        /// <param name="fullException">Whether or not to return full exception details in the error list.</param>
        public ErrorParser(ResourceManager resources, bool fullException = true)
        {
            this.resources = resources;
            this.fullException = fullException;
        }

        /// <summary>
        /// Retrieves the error list from the specified exception if possible,
        /// otherwise constructs a new error list with the exception as the error message.
        /// </summary>
        /// <param name="ex">Exception to retrieve the error list from.</param>
        /// <returns>An error list retrieved from the exception.</returns>
        public virtual ErrorList FromException(Exception ex)
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

            // construct a new error list from exception string
            return GetExceptionErrorList(ex.ToString());
        }

        /// <summary>
        /// Helper method to construct an error list for the given exception messsage,
        /// taking into account parser's configuration for returning full exception details.
        /// </summary>
        /// <param name="exceptionMessage">Full exception message for the error list.</param>
        /// <returns>An error list with a single error for the exception.</returns>
        protected ErrorList GetExceptionErrorList(string exceptionMessage)
        {
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
