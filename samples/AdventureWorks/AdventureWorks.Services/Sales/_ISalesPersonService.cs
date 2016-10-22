//---------------------------------------------------------------------------------------------
// This file was AUTO-GENERATED by "WCF Service Contracts" Xomega.Net generator.
//
// Manual CHANGES to this file WILL BE LOST when the code is regenerated.
//---------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using Xomega.Framework;

namespace AdventureWorks.Services
{
    #region ISalesPersonService

    ///<summary>
    /// Sales representative current information.
    ///</summary>
    [ServiceContract]
    public interface ISalesPersonService
    {

        ///<summary>
        /// Reads a list of Sales Person objects based on the specified criteria.
        ///</summary>
        [OperationContract]
        [FaultContract(typeof(ErrorList))]
        IEnumerable<SalesPerson_ReadListOutput> ReadList();

    }
    #endregion

    #region SalesPerson_ReadListOutput structure

    ///<summary>
    /// The output structure of operation ISalesPersonService.ReadList.
    ///</summary>
    [DataContract]
    public class SalesPerson_ReadListOutput
    {
        [DataMember]
        public int BusinessEntityId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool IsCurrent { get; set; }
        [DataMember]
        public int? TerritoryId { get; set; }
    }
    #endregion

}