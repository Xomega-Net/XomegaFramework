//---------------------------------------------------------------------------------------------
// This file was AUTO-GENERATED by "EF Domain Objects" Xomega.Net generator.
//
// Manual CHANGES to this file WILL BE LOST when the code is regenerated.
//---------------------------------------------------------------------------------------------

using System;

namespace AdventureWorks.Services.Entities
{
    ///<summary>
    /// State and province lookup table.
    ///</summary>
    public partial class StateProvince
    {
        public StateProvince()
        {
        }

        #region Properties

        public int StateProvinceId  { get; set; }

        ///<summary>
        /// ISO standard state or province code.
        ///</summary>
        public string StateProvinceCode  { get; set; }

        ///<summary>
        /// ISO standard country or region code. Foreign key to CountryRegion.CountryRegionCode. 
        ///</summary>
        public string CountryRegionCode  { get; set; }

        ///<summary>
        /// 0 = StateProvinceCode exists. 1 = StateProvinceCode unavailable, using CountryRegionCode.
        ///</summary>
        public bool IsOnlyStateProvinceFlag  { get; set; }

        ///<summary>
        /// State or province description.
        ///</summary>
        public string Name  { get; set; }

        ///<summary>
        /// ID of the territory in which the state or province is located. Foreign key to SalesTerritory.SalesTerritoryID.
        ///</summary>
        public int TerritoryId  { get; set; }

        ///<summary>
        /// ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.
        ///</summary>
        public Guid Rowguid  { get; set; }

        ///<summary>
        /// Date and time the record was last updated.
        ///</summary>
        public DateTime ModifiedDate  { get; set; }

        #endregion

        #region Navigation Properties

        ///<summary>
        /// CountryRegion object referenced by the field CountryRegionCode.
        ///</summary>
        public virtual CountryRegion CountryRegionCodeObject { get; set; }

        ///<summary>
        /// SalesTerritory object referenced by the field TerritoryId.
        ///</summary>
        public virtual SalesTerritory TerritoryObject { get; set; }

        #endregion
    }
}