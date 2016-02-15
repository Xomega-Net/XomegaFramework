// Copyright (c) 2010-2013 Xomega.Net. All rights reserved.

using System.ServiceModel;

namespace Xomega.Framework
{
    /// <summary>
    /// A base class for all Xomega service interfaces that provides common functionality for all interfaces.
    /// </summary>
    [ServiceContract]
    public interface IServiceBase
    {
        /// <summary>
        /// Validates and saves all changes that have been made during prior service calls in the same session.
        /// If there are any validation errors during saving of the changes than a fault will be raised
        /// with an error list that contains all the errors. A fault will also be raised if there are only
        /// validation warnings and the <c>suppressWarnings</c> flag is passed in as false. In this case
        /// the client can review the warnings and re-issue the service call with this flag set to true
        /// to proceed regardless of the warnings.
        /// </summary>
        /// <param name="suppressWarnings">True to save changes even if there are warnings,
        /// False to raise a fault if there are any warnings.</param>
        /// <returns>The number of objects that have been added, modified, or deleted in the current session.</returns>
        /// <seealso cref="System.Data.Objects.ObjectContext.SaveChanges()"/>
        [OperationContract]
        [FaultContract(typeof(ErrorList))]
        int SaveChanges(bool suppressWarnings);

        /// <summary>
        /// An explicit call to end the service session to support custom session mechanism for http bindings
        /// in Silverlight 3. This will allow releasing the instance of the service object on the server.
        /// </summary>
        [OperationContract]
        void EndSession();
    }
}
