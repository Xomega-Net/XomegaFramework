//---------------------------------------------------------------------------------------------
// This file was AUTO-GENERATED by "Service Contracts" Xomega.Net generator.
//
// Manual CHANGES to this file WILL BE LOST when the code is regenerated.
//---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xomega.Framework.Services;

namespace AdventureWorks.Services.Common
{
    #region ICustomerService interface

    ///<summary>
    /// Current customer information. Also see the Person and Store tables.
    ///</summary>
    public interface ICustomerService
    {

        ///<summary>
        /// Reads a list of Customer objects based on the specified criteria.
        ///</summary>
        Task<Output<ICollection<Customer_ReadListOutput>>> ReadListAsync(Customer_ReadListInput_Criteria _criteria, CancellationToken token = default);

    }
    #endregion

    #region Customer_ReadListInput_Criteria structure

    ///<summary>
    /// Structure of parameter Criteria of the input structure of operation ICustomerService.ReadListAsync.
    ///</summary>
    public class Customer_ReadListInput_Criteria
    {

        ///<summary>
        /// ID of the territory in which the customer is located. Foreign key to SalesTerritory.SalesTerritoryID.
        ///</summary>
        [XLookupValue("sales territory")]
        public int? TerritoryId { get; set; }

        [XMaxLength(25)]
        [XLookupValue("operators")]
        public string PersonNameOperator { get; set; }

        public string PersonName { get; set; }

        [XMaxLength(25)]
        [XLookupValue("operators")]
        public string StoreNameOperator { get; set; }

        public string StoreName { get; set; }

        ///<summary>
        /// Comparison operator for the corresponding Account Number criteria.
        ///</summary>
        [XMaxLength(25)]
        [XLookupValue("operators")]
        public string AccountNumberOperator { get; set; }

        ///<summary>
        /// Unique number identifying the customer assigned by the accounting system.
        ///</summary>
        [XMaxLength(10)]
        public string AccountNumber { get; set; }
    }
    #endregion

    #region Customer_ReadListOutput structure

    ///<summary>
    /// The output structure of operation ICustomerService.ReadListAsync.
    ///</summary>
    public class Customer_ReadListOutput
    {

        public int CustomerId { get; set; }

        ///<summary>
        /// Foreign key to Person.BusinessEntityID
        ///</summary>
        public int? PersonId { get; set; }

        public string PersonName { get; set; }

        ///<summary>
        /// Foreign key to Store.BusinessEntityID
        ///</summary>
        public int? StoreId { get; set; }

        public string StoreName { get; set; }

        ///<summary>
        /// ID of the territory in which the customer is located. Foreign key to SalesTerritory.SalesTerritoryID.
        ///</summary>
        public int? TerritoryId { get; set; }

        ///<summary>
        /// Unique number identifying the customer assigned by the accounting system.
        ///</summary>
        public string AccountNumber { get; set; }
    }
    #endregion

}