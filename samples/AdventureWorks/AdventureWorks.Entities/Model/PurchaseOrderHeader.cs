//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AdventureWorks.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class PurchaseOrderHeader
    {
        public int PurchaseOrderId { get; set; }
        public byte RevisionNumber { get; set; }
        public byte Status { get; set; }
        public System.DateTime OrderDate { get; set; }
        public Nullable<System.DateTime> ShipDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal Freight { get; set; }
        public decimal TotalDue { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    
        public virtual Employee EmployeeIdObject { get; set; }
        public virtual Vendor VendorIdObject { get; set; }
        public virtual ShipMethod ShipMethodIdObject { get; set; }
    }
}