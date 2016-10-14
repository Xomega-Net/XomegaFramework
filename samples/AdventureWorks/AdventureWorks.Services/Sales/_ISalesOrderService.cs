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
    #region ISalesOrderService

    ///<summary>
    /// General sales order information.
    ///</summary>
    [ServiceContract]
    public interface ISalesOrderService
    {

        ///<summary>
        /// Reads the values of a Sales Order object by its key.
        ///</summary>
        [OperationContract]
        [FaultContract(typeof(ErrorList))]
        SalesOrder_ReadOutput Read(int _salesOrderId);

        ///<summary>
        /// Creates a new Sales Order object using the specified data.
        ///</summary>
        [OperationContract]
        [FaultContract(typeof(ErrorList))]
        SalesOrder_CreateOutput Create(SalesOrder_CreateInput _data);

        ///<summary>
        /// Updates existing Sales Order object using the specified data.
        ///</summary>
        [OperationContract]
        [FaultContract(typeof(ErrorList))]
        void Update(int _salesOrderId, SalesOrder_UpdateInput_Data _data);

        ///<summary>
        /// Deletes the specified Sales Order object.
        ///</summary>
        [OperationContract]
        [FaultContract(typeof(ErrorList))]
        void Delete(int _salesOrderId);

        ///<summary>
        /// Reads a list of Sales Order objects based on the specified criteria.
        ///</summary>
        [OperationContract]
        [FaultContract(typeof(ErrorList))]
        IEnumerable<SalesOrder_ReadListOutput> ReadList(SalesOrder_ReadListInput_Criteria _criteria);

        ///<summary>
        /// Reads the values of a Sales Order Detail object by its key.
        ///</summary>
        [OperationContract]
        [FaultContract(typeof(ErrorList))]
        SalesOrderDetail_ReadOutput Detail_Read(int _salesOrderDetailId);

        ///<summary>
        /// Creates a new Sales Order Detail object using the specified data.
        ///</summary>
        [OperationContract]
        [FaultContract(typeof(ErrorList))]
        SalesOrderDetail_CreateOutput Detail_Create(SalesOrderDetail_CreateInput _data);

        ///<summary>
        /// Updates existing Sales Order Detail object using the specified data.
        ///</summary>
        [OperationContract]
        [FaultContract(typeof(ErrorList))]
        void Detail_Update(int _salesOrderDetailId, SalesOrderDetail_UpdateInput_Data _data);

        ///<summary>
        /// Deletes the specified Sales Order Detail object.
        ///</summary>
        [OperationContract]
        [FaultContract(typeof(ErrorList))]
        void Detail_Delete(int _salesOrderDetailId);

        ///<summary>
        /// Reads a list of Sales Order Detail objects based on the specified criteria.
        ///</summary>
        [OperationContract]
        [FaultContract(typeof(ErrorList))]
        IEnumerable<SalesOrderDetail_ReadListOutput> Detail_ReadList(int _salesOrderId);

        ///<summary>
        /// Reads the values of a Sales Order Reason object by its key.
        ///</summary>
        [OperationContract]
        [FaultContract(typeof(ErrorList))]
        SalesOrderReason_ReadOutput Reason_Read(int _salesOrderId, int _salesReasonId);

        ///<summary>
        /// Creates a new Sales Order Reason object using the specified data.
        ///</summary>
        [OperationContract]
        [FaultContract(typeof(ErrorList))]
        void Reason_Create(SalesOrderReason_CreateInput _data);

        ///<summary>
        /// Updates existing or creates a new Sales Order Reason object using the specified data.
        ///</summary>
        [OperationContract]
        [FaultContract(typeof(ErrorList))]
        void Reason_Update(int _salesOrderId, int _salesReasonId, SalesOrderReason_UpdateInput_Data _data);

        ///<summary>
        /// Deletes the specified Sales Order Reason object.
        ///</summary>
        [OperationContract]
        [FaultContract(typeof(ErrorList))]
        void Reason_Delete(int _salesOrderId, int _salesReasonId);

        ///<summary>
        /// Reads a list of Sales Order Reason objects based on the specified criteria.
        ///</summary>
        [OperationContract]
        [FaultContract(typeof(ErrorList))]
        IEnumerable<SalesOrderReason_ReadListOutput> Reason_ReadList(int _salesOrderId);

    }
    #endregion

    #region SalesOrder_ReadOutput structure

    ///<summary>
    /// The output structure of operation ISalesOrderService.Read.
    ///</summary>
    [DataContract]
    public class SalesOrder_ReadOutput
    {
        [DataMember]
        public byte RevisionNumber { get; set; }
        [DataMember]
        public DateTime OrderDate { get; set; }
        [DataMember]
        public DateTime DueDate { get; set; }
        [DataMember]
        public DateTime? ShipDate { get; set; }
        [DataMember]
        public byte Status { get; set; }
        [DataMember]
        public bool OnlineOrderFlag { get; set; }
        [DataMember]
        public string SalesOrderNumber { get; set; }
        [DataMember]
        public string PurchaseOrderNumber { get; set; }
        [DataMember]
        public string AccountNumber { get; set; }
        [DataMember]
        public int CustomerId { get; set; }
        [DataMember]
        public int? SalesPersonId { get; set; }
        [DataMember]
        public int? TerritoryId { get; set; }
        [DataMember]
        public int BillToAddressId { get; set; }
        [DataMember]
        public int ShipToAddressId { get; set; }
        [DataMember]
        public int ShipMethodId { get; set; }
        [DataMember]
        public int? CreditCardId { get; set; }
        [DataMember]
        public string CreditCardApprovalCode { get; set; }
        [DataMember]
        public int? CurrencyRateId { get; set; }
        [DataMember]
        public decimal SubTotal { get; set; }
        [DataMember]
        public decimal TaxAmt { get; set; }
        [DataMember]
        public decimal Freight { get; set; }
        [DataMember]
        public decimal TotalDue { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public Guid Rowguid { get; set; }
        [DataMember]
        public DateTime ModifiedDate { get; set; }
    }
    #endregion

    #region SalesOrder_CreateInput structure

    ///<summary>
    /// The input structure of operation ISalesOrderService.Create.
    ///</summary>
    [DataContract]
    public class SalesOrder_CreateInput
    {
        [DataMember]
        public byte RevisionNumber { get; set; }
        [DataMember]
        public DateTime OrderDate { get; set; }
        [DataMember]
        public DateTime DueDate { get; set; }
        [DataMember]
        public DateTime? ShipDate { get; set; }
        [DataMember]
        public byte Status { get; set; }
        [DataMember]
        public bool OnlineOrderFlag { get; set; }
        [DataMember]
        public string SalesOrderNumber { get; set; }
        [DataMember]
        public string PurchaseOrderNumber { get; set; }
        [DataMember]
        public string AccountNumber { get; set; }
        [DataMember]
        public int CustomerId { get; set; }
        [DataMember]
        public int? SalesPersonId { get; set; }
        [DataMember]
        public int? TerritoryId { get; set; }
        [DataMember]
        public int BillToAddressId { get; set; }
        [DataMember]
        public int ShipToAddressId { get; set; }
        [DataMember]
        public int ShipMethodId { get; set; }
        [DataMember]
        public int? CreditCardId { get; set; }
        [DataMember]
        public string CreditCardApprovalCode { get; set; }
        [DataMember]
        public int? CurrencyRateId { get; set; }
        [DataMember]
        public decimal SubTotal { get; set; }
        [DataMember]
        public decimal TaxAmt { get; set; }
        [DataMember]
        public decimal Freight { get; set; }
        [DataMember]
        public decimal TotalDue { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public Guid Rowguid { get; set; }
        [DataMember]
        public DateTime ModifiedDate { get; set; }
    }
    #endregion

    #region SalesOrder_CreateOutput structure

    ///<summary>
    /// The output structure of operation ISalesOrderService.Create.
    ///</summary>
    [DataContract]
    public class SalesOrder_CreateOutput
    {
        [DataMember]
        public int SalesOrderId { get; set; }
    }
    #endregion

    #region SalesOrder_UpdateInput_Data structure

    ///<summary>
    /// Structure of parameter Data of the input structure of operation ISalesOrderService.Update.
    ///</summary>
    [DataContract]
    public class SalesOrder_UpdateInput_Data
    {
        [DataMember]
        public byte RevisionNumber { get; set; }
        [DataMember]
        public DateTime OrderDate { get; set; }
        [DataMember]
        public DateTime DueDate { get; set; }
        [DataMember]
        public DateTime? ShipDate { get; set; }
        [DataMember]
        public byte Status { get; set; }
        [DataMember]
        public bool OnlineOrderFlag { get; set; }
        [DataMember]
        public string SalesOrderNumber { get; set; }
        [DataMember]
        public string PurchaseOrderNumber { get; set; }
        [DataMember]
        public string AccountNumber { get; set; }
        [DataMember]
        public int CustomerId { get; set; }
        [DataMember]
        public int? SalesPersonId { get; set; }
        [DataMember]
        public int? TerritoryId { get; set; }
        [DataMember]
        public int BillToAddressId { get; set; }
        [DataMember]
        public int ShipToAddressId { get; set; }
        [DataMember]
        public int ShipMethodId { get; set; }
        [DataMember]
        public int? CreditCardId { get; set; }
        [DataMember]
        public string CreditCardApprovalCode { get; set; }
        [DataMember]
        public int? CurrencyRateId { get; set; }
        [DataMember]
        public decimal SubTotal { get; set; }
        [DataMember]
        public decimal TaxAmt { get; set; }
        [DataMember]
        public decimal Freight { get; set; }
        [DataMember]
        public decimal TotalDue { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public Guid Rowguid { get; set; }
        [DataMember]
        public DateTime ModifiedDate { get; set; }
    }
    #endregion

    #region SalesOrder_ReadListInput_Criteria structure

    ///<summary>
    /// Structure of parameter Criteria of the input structure of operation ISalesOrderService.ReadList.
    ///</summary>
    [DataContract]
    public class SalesOrder_ReadListInput_Criteria
    {
        ///<summary>
        /// Comparison operator for the corresponding Revision Number criteria.
        ///</summary>
        [DataMember]
        public string RevisionNumberOperator { get; set; }
        [DataMember]
        public byte? RevisionNumber { get; set; }
        ///<summary>
        /// End of range for the corresponding Revision Number criteria for the BETWEEN operator.
        ///</summary>
        [DataMember]
        public byte? RevisionNumber2 { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Order Date criteria.
        ///</summary>
        [DataMember]
        public string OrderDateOperator { get; set; }
        [DataMember]
        public DateTime? OrderDate { get; set; }
        ///<summary>
        /// End of range for the corresponding Order Date criteria for the BETWEEN operator.
        ///</summary>
        [DataMember]
        public DateTime? OrderDate2 { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Due Date criteria.
        ///</summary>
        [DataMember]
        public string DueDateOperator { get; set; }
        [DataMember]
        public DateTime? DueDate { get; set; }
        ///<summary>
        /// End of range for the corresponding Due Date criteria for the BETWEEN operator.
        ///</summary>
        [DataMember]
        public DateTime? DueDate2 { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Ship Date criteria.
        ///</summary>
        [DataMember]
        public string ShipDateOperator { get; set; }
        [DataMember]
        public DateTime? ShipDate { get; set; }
        ///<summary>
        /// End of range for the corresponding Ship Date criteria for the BETWEEN operator.
        ///</summary>
        [DataMember]
        public DateTime? ShipDate2 { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Status criteria.
        ///</summary>
        [DataMember]
        public string StatusOperator { get; set; }
        [DataMember]
        public byte? Status { get; set; }
        ///<summary>
        /// End of range for the corresponding Status criteria for the BETWEEN operator.
        ///</summary>
        [DataMember]
        public byte? Status2 { get; set; }
        [DataMember]
        public bool? OnlineOrderFlag { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Sales Order Number criteria.
        ///</summary>
        [DataMember]
        public string SalesOrderNumberOperator { get; set; }
        [DataMember]
        public string SalesOrderNumber { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Purchase Order Number criteria.
        ///</summary>
        [DataMember]
        public string PurchaseOrderNumberOperator { get; set; }
        [DataMember]
        public string PurchaseOrderNumber { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Account Number criteria.
        ///</summary>
        [DataMember]
        public string AccountNumberOperator { get; set; }
        [DataMember]
        public string AccountNumber { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Customer Id criteria.
        ///</summary>
        [DataMember]
        public string CustomerIdOperator { get; set; }
        [DataMember]
        public int? CustomerId { get; set; }
        ///<summary>
        /// End of range for the corresponding Customer Id criteria for the BETWEEN operator.
        ///</summary>
        [DataMember]
        public int? CustomerId2 { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Sales Person Id criteria.
        ///</summary>
        [DataMember]
        public string SalesPersonIdOperator { get; set; }
        [DataMember]
        public int? SalesPersonId { get; set; }
        ///<summary>
        /// End of range for the corresponding Sales Person Id criteria for the BETWEEN operator.
        ///</summary>
        [DataMember]
        public int? SalesPersonId2 { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Territory Id criteria.
        ///</summary>
        [DataMember]
        public string TerritoryIdOperator { get; set; }
        [DataMember]
        public int? TerritoryId { get; set; }
        ///<summary>
        /// End of range for the corresponding Territory Id criteria for the BETWEEN operator.
        ///</summary>
        [DataMember]
        public int? TerritoryId2 { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Bill To Address Id criteria.
        ///</summary>
        [DataMember]
        public string BillToAddressIdOperator { get; set; }
        [DataMember]
        public int? BillToAddressId { get; set; }
        ///<summary>
        /// End of range for the corresponding Bill To Address Id criteria for the BETWEEN operator.
        ///</summary>
        [DataMember]
        public int? BillToAddressId2 { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Ship To Address Id criteria.
        ///</summary>
        [DataMember]
        public string ShipToAddressIdOperator { get; set; }
        [DataMember]
        public int? ShipToAddressId { get; set; }
        ///<summary>
        /// End of range for the corresponding Ship To Address Id criteria for the BETWEEN operator.
        ///</summary>
        [DataMember]
        public int? ShipToAddressId2 { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Ship Method Id criteria.
        ///</summary>
        [DataMember]
        public string ShipMethodIdOperator { get; set; }
        [DataMember]
        public int? ShipMethodId { get; set; }
        ///<summary>
        /// End of range for the corresponding Ship Method Id criteria for the BETWEEN operator.
        ///</summary>
        [DataMember]
        public int? ShipMethodId2 { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Credit Card Id criteria.
        ///</summary>
        [DataMember]
        public string CreditCardIdOperator { get; set; }
        [DataMember]
        public int? CreditCardId { get; set; }
        ///<summary>
        /// End of range for the corresponding Credit Card Id criteria for the BETWEEN operator.
        ///</summary>
        [DataMember]
        public int? CreditCardId2 { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Credit Card Approval Code criteria.
        ///</summary>
        [DataMember]
        public string CreditCardApprovalCodeOperator { get; set; }
        [DataMember]
        public string CreditCardApprovalCode { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Currency Rate Id criteria.
        ///</summary>
        [DataMember]
        public string CurrencyRateIdOperator { get; set; }
        [DataMember]
        public int? CurrencyRateId { get; set; }
        ///<summary>
        /// End of range for the corresponding Currency Rate Id criteria for the BETWEEN operator.
        ///</summary>
        [DataMember]
        public int? CurrencyRateId2 { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Sub Total criteria.
        ///</summary>
        [DataMember]
        public string SubTotalOperator { get; set; }
        [DataMember]
        public decimal? SubTotal { get; set; }
        ///<summary>
        /// End of range for the corresponding Sub Total criteria for the BETWEEN operator.
        ///</summary>
        [DataMember]
        public decimal? SubTotal2 { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Tax Amt criteria.
        ///</summary>
        [DataMember]
        public string TaxAmtOperator { get; set; }
        [DataMember]
        public decimal? TaxAmt { get; set; }
        ///<summary>
        /// End of range for the corresponding Tax Amt criteria for the BETWEEN operator.
        ///</summary>
        [DataMember]
        public decimal? TaxAmt2 { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Freight criteria.
        ///</summary>
        [DataMember]
        public string FreightOperator { get; set; }
        [DataMember]
        public decimal? Freight { get; set; }
        ///<summary>
        /// End of range for the corresponding Freight criteria for the BETWEEN operator.
        ///</summary>
        [DataMember]
        public decimal? Freight2 { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Total Due criteria.
        ///</summary>
        [DataMember]
        public string TotalDueOperator { get; set; }
        [DataMember]
        public decimal? TotalDue { get; set; }
        ///<summary>
        /// End of range for the corresponding Total Due criteria for the BETWEEN operator.
        ///</summary>
        [DataMember]
        public decimal? TotalDue2 { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Comment criteria.
        ///</summary>
        [DataMember]
        public string CommentOperator { get; set; }
        [DataMember]
        public string Comment { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Rowguid criteria.
        ///</summary>
        [DataMember]
        public string RowguidOperator { get; set; }
        [DataMember]
        public Guid? Rowguid { get; set; }
        ///<summary>
        /// Comparison operator for the corresponding Modified Date criteria.
        ///</summary>
        [DataMember]
        public string ModifiedDateOperator { get; set; }
        [DataMember]
        public DateTime? ModifiedDate { get; set; }
        ///<summary>
        /// End of range for the corresponding Modified Date criteria for the BETWEEN operator.
        ///</summary>
        [DataMember]
        public DateTime? ModifiedDate2 { get; set; }
    }
    #endregion

    #region SalesOrder_ReadListOutput structure

    ///<summary>
    /// The output structure of operation ISalesOrderService.ReadList.
    ///</summary>
    [DataContract]
    public class SalesOrder_ReadListOutput
    {
        [DataMember]
        public int SalesOrderId { get; set; }
        [DataMember]
        public string SalesOrderNumber { get; set; }
        [DataMember]
        public byte Status { get; set; }
        [DataMember]
        public DateTime OrderDate { get; set; }
        [DataMember]
        public DateTime? ShipDate { get; set; }
        [DataMember]
        public DateTime DueDate { get; set; }
        [DataMember]
        public decimal TotalDue { get; set; }
        [DataMember]
        public bool OnlineOrderFlag { get; set; }
        [DataMember]
        public string CustomerStore { get; set; }
        [DataMember]
        public string CustomerName { get; set; }
        [DataMember]
        public int? SalesPersonId { get; set; }
        [DataMember]
        public int? TerritoryId { get; set; }
    }
    #endregion

    #region SalesOrderDetail_ReadOutput structure

    ///<summary>
    /// The output structure of operation IDetailService.Detail_Read.
    ///</summary>
    [DataContract]
    public class SalesOrderDetail_ReadOutput
    {
        [DataMember]
        public int SalesOrderId { get; set; }
        [DataMember]
        public string CarrierTrackingNumber { get; set; }
        [DataMember]
        public short OrderQty { get; set; }
        [DataMember]
        public int SpecialOfferId { get; set; }
        [DataMember]
        public int ProductId { get; set; }
        [DataMember]
        public decimal UnitPrice { get; set; }
        [DataMember]
        public decimal UnitPriceDiscount { get; set; }
        [DataMember]
        public decimal LineTotal { get; set; }
        [DataMember]
        public Guid Rowguid { get; set; }
        [DataMember]
        public DateTime ModifiedDate { get; set; }
    }
    #endregion

    #region SalesOrderDetail_CreateInput structure

    ///<summary>
    /// The input structure of operation IDetailService.Detail_Create.
    ///</summary>
    [DataContract]
    public class SalesOrderDetail_CreateInput
    {
        [DataMember]
        public int SalesOrderId { get; set; }
        [DataMember]
        public string CarrierTrackingNumber { get; set; }
        [DataMember]
        public short OrderQty { get; set; }
        [DataMember]
        public int SpecialOfferId { get; set; }
        [DataMember]
        public int ProductId { get; set; }
        [DataMember]
        public decimal UnitPrice { get; set; }
        [DataMember]
        public decimal UnitPriceDiscount { get; set; }
        [DataMember]
        public decimal LineTotal { get; set; }
        [DataMember]
        public Guid Rowguid { get; set; }
        [DataMember]
        public DateTime ModifiedDate { get; set; }
    }
    #endregion

    #region SalesOrderDetail_CreateOutput structure

    ///<summary>
    /// The output structure of operation IDetailService.Detail_Create.
    ///</summary>
    [DataContract]
    public class SalesOrderDetail_CreateOutput
    {
        [DataMember]
        public int SalesOrderDetailId { get; set; }
    }
    #endregion

    #region SalesOrderDetail_UpdateInput_Data structure

    ///<summary>
    /// Structure of parameter Data of the input structure of operation IDetailService.Detail_Update.
    ///</summary>
    [DataContract]
    public class SalesOrderDetail_UpdateInput_Data
    {
        [DataMember]
        public string CarrierTrackingNumber { get; set; }
        [DataMember]
        public short OrderQty { get; set; }
        [DataMember]
        public int SpecialOfferId { get; set; }
        [DataMember]
        public int ProductId { get; set; }
        [DataMember]
        public decimal UnitPrice { get; set; }
        [DataMember]
        public decimal UnitPriceDiscount { get; set; }
        [DataMember]
        public decimal LineTotal { get; set; }
        [DataMember]
        public Guid Rowguid { get; set; }
        [DataMember]
        public DateTime ModifiedDate { get; set; }
    }
    #endregion

    #region SalesOrderDetail_ReadListOutput structure

    ///<summary>
    /// The output structure of operation IDetailService.Detail_ReadList.
    ///</summary>
    [DataContract]
    public class SalesOrderDetail_ReadListOutput
    {
        [DataMember]
        public int SalesOrderDetailId { get; set; }
        [DataMember]
        public string CarrierTrackingNumber { get; set; }
        [DataMember]
        public short OrderQty { get; set; }
        [DataMember]
        public int SpecialOfferId { get; set; }
        [DataMember]
        public int ProductId { get; set; }
        [DataMember]
        public decimal UnitPrice { get; set; }
        [DataMember]
        public decimal UnitPriceDiscount { get; set; }
        [DataMember]
        public decimal LineTotal { get; set; }
        [DataMember]
        public Guid Rowguid { get; set; }
        [DataMember]
        public DateTime ModifiedDate { get; set; }
    }
    #endregion

    #region SalesOrderReason_ReadOutput structure

    ///<summary>
    /// The output structure of operation IReasonService.Reason_Read.
    ///</summary>
    [DataContract]
    public class SalesOrderReason_ReadOutput
    {
        [DataMember]
        public DateTime ModifiedDate { get; set; }
    }
    #endregion

    #region SalesOrderReason_CreateInput structure

    ///<summary>
    /// The input structure of operation IReasonService.Reason_Create.
    ///</summary>
    [DataContract]
    public class SalesOrderReason_CreateInput
    {
        [DataMember]
        public int SalesOrderId { get; set; }
        [DataMember]
        public int SalesReasonId { get; set; }
        [DataMember]
        public DateTime ModifiedDate { get; set; }
    }
    #endregion

    #region SalesOrderReason_UpdateInput_Data structure

    ///<summary>
    /// Structure of parameter Data of the input structure of operation IReasonService.Reason_Update.
    ///</summary>
    [DataContract]
    public class SalesOrderReason_UpdateInput_Data
    {
        [DataMember]
        public DateTime ModifiedDate { get; set; }
    }
    #endregion

    #region SalesOrderReason_ReadListOutput structure

    ///<summary>
    /// The output structure of operation IReasonService.Reason_ReadList.
    ///</summary>
    [DataContract]
    public class SalesOrderReason_ReadListOutput
    {
        [DataMember]
        public int SalesReasonId { get; set; }
        [DataMember]
        public DateTime ModifiedDate { get; set; }
    }
    #endregion

}