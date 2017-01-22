// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System;
using System.Net;
using System.Runtime.Serialization;

namespace Xomega.Framework
{
    /// <summary>
    /// Customizable class for parsing a list of errors from an exception
    /// </summary>
    public class ErrorParser
    {
        /// <summary>
        /// Retrieves the error list from the specified exception if possible,
        /// otherwise constructs a new error list with the exception as the error message.
        /// </summary>
        /// <param name="ex">Exception to retrieve the error list from.</param>
        /// <returns>An error list retrieved from the exception.</returns>
        public virtual ErrorList FromException(Exception ex)
        {
            // check if exception is ErrorAbortException first
            ErrorAbortException ea = ex as ErrorAbortException;
            if (ea != null) return ea.Errors;

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
            ErrorList err = new ErrorList();
            err.Add(new ErrorMessage("EXCEPTION", ex.ToString()));
            return err;
        }
    }
}
