// Copyright (c) 2020 Xomega.Net. All rights reserved.

using System;
using System.Resources;
using System.ServiceModel;

namespace Xomega.Framework.Wcf
{
    /// <summary>
    /// WCF specific class for parsing a list of errors from a fault exception
    /// </summary>
    public class FaultErrorParser : ErrorParser
    {
        /// <summary>
        /// Constructs a new error parser that is able to parse WCF faults.
        /// </summary>
        /// <param name="resources">Resource manager to use for localization.</param>
        /// <param name="fullException">Whether or not to return full exception details in the error list.</param>
        public FaultErrorParser(ResourceManager resources, bool fullException = true) : base(resources, fullException)
        {
        }

        /// <summary>
        /// Retrieves the error list from the specified exception if possible,
        /// otherwise constructs a new error list with the exception as the error message.
        /// </summary>
        /// <param name="ex">Exception to retrieve the error list from.</param>
        /// <returns>An error list retrieved from the exception.</returns>
        public override ErrorList FromException(Exception ex)
        {
            if (ex is FaultException<ErrorList> fex) return fex.Detail;

            // use the server side exception if applicable
            if (ex is FaultException<ExceptionDetail> fexd && fexd.Detail != null)
                return GetExceptionErrorList(fexd.Detail.ToString());

            return base.FromException(ex);
        }
    }
}
