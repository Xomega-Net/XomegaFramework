// Copyright (c) 2017 Xomega.Net. All rights reserved.

using System.ServiceModel;

namespace Xomega.Framework.Wcf
{
    /// <summary>
    /// WCF specific class for a list of errors
    /// </summary>
    public class FaultErrorList : ErrorList
    {
        /// <summary>
        /// Overrides abort to throw a fault exception
        /// </summary>
        /// <param name="reason"></param>
        public override void Abort(string reason)
        {
            ErrorList el = new ErrorList();
            el.MergeWith(this);
            throw new FaultException<ErrorList>(el, new FaultReason(reason), new FaultCode("Sender"), null);
        }
    }
}
