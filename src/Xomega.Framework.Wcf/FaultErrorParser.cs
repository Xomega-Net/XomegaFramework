// Copyright (c) 2022 Xomega.Net. All rights reserved.

using Microsoft.Extensions.Logging;
using System;
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
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="fullException">Whether or not to return full exception details in the error list.</param>
        public FaultErrorParser(IServiceProvider serviceProvider, bool fullException = true) : base(serviceProvider, fullException)
        {
        }

        /// <inheritdoc/>
        public override ErrorList FromException(Exception ex, ILogger logger = null)
        {
            if (ex is FaultException<ErrorList> fex) return fex.Detail;

            // use the server side exception if applicable
            if (ex is FaultException<ExceptionDetail> fexd && fexd.Detail != null)
                return GetExceptionErrorList(fexd.Detail.Message);

            return base.FromException(ex);
        }
    }
}
