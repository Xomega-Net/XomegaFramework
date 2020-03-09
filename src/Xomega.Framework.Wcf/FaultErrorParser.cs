// Copyright (c) 2020 Xomega.Net. All rights reserved.

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
        /// Retrieves the error list from the specified exception if possible,
        /// otherwise constructs a new error list with the exception as the error message.
        /// </summary>
        /// <param name="ex">Exception to retrieve the error list from.</param>
        /// <returns>An error list retrieved from the exception.</returns>
        public override ErrorList FromException(Exception ex)
        {
            FaultException<ErrorList> fex = ex as FaultException<ErrorList>;
            if (fex != null) return fex.Detail;

            // use the server side exception if applicable
            FaultException<ExceptionDetail> fexd = ex as FaultException<ExceptionDetail>;
            if (fexd != null && fexd.Detail != null)
            {
                ErrorList err = new ErrorList();
                err.Add(new ErrorMessage("EXCEPTION", fexd.Detail.ToString()));
                return err;
            }

            return base.FromException(ex);
        }
    }
}
