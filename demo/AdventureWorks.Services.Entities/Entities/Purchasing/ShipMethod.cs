//---------------------------------------------------------------------------------------------
// This file was AUTO-GENERATED by "EF Domain Objects" Xomega.Net generator.
//
// Manual CHANGES to this file WILL BE LOST when the code is regenerated.
//---------------------------------------------------------------------------------------------

using System;

namespace AdventureWorks.Services.Entities
{
    ///<summary>
    /// Shipping company lookup table.
    ///</summary>
    public partial class ShipMethod
    {
        public ShipMethod()
        {
        }

        #region Properties

        public int ShipMethodId  { get; set; }

        ///<summary>
        /// Shipping company name.
        ///</summary>
        public string Name  { get; set; }

        ///<summary>
        /// Minimum shipping charge.
        ///</summary>
        public decimal ShipBase  { get; set; }

        ///<summary>
        /// Shipping charge per pound.
        ///</summary>
        public decimal ShipRate  { get; set; }

        ///<summary>
        /// ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.
        ///</summary>
        public Guid Rowguid  { get; set; }

        ///<summary>
        /// Date and time the record was last updated.
        ///</summary>
        public DateTime ModifiedDate  { get; set; }

        #endregion
    }
}