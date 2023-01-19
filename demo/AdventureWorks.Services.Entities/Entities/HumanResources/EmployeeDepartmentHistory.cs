//---------------------------------------------------------------------------------------------
// This file was AUTO-GENERATED by "EF Domain Objects" Xomega.Net generator.
//
// Manual CHANGES to this file WILL BE LOST when the code is regenerated.
//---------------------------------------------------------------------------------------------

using System;

namespace AdventureWorks.Services.Entities
{
    ///<summary>
    /// Employee department transfers.
    ///</summary>
    public partial class EmployeeDepartmentHistory
    {
        public EmployeeDepartmentHistory()
        {
        }

        #region Properties

        ///<summary>
        /// Employee identification number. Foreign key to Employee.BusinessEntityID.
        ///</summary>
        public int BusinessEntityId  { get; set; }

        ///<summary>
        /// Date the employee started work in the department.
        ///</summary>
        public DateTime StartDate  { get; set; }

        ///<summary>
        /// Department in which the employee worked including currently. Foreign key to Department.DepartmentID.
        ///</summary>
        public short DepartmentId  { get; set; }

        ///<summary>
        /// Identifies which 8-hour shift the employee works. Foreign key to Shift.Shift.ID.
        ///</summary>
        public byte ShiftId  { get; set; }

        ///<summary>
        /// Date the employee left the department. NULL = Current department.
        ///</summary>
        public DateTime? EndDate  { get; set; }

        ///<summary>
        /// Date and time the record was last updated.
        ///</summary>
        public DateTime ModifiedDate  { get; set; }

        #endregion

        #region Navigation Properties

        ///<summary>
        /// Employee object referenced by the field BusinessEntityId.
        ///</summary>
        public virtual Employee BusinessEntityObject { get; set; }

        ///<summary>
        /// Department object referenced by the field DepartmentId.
        ///</summary>
        public virtual Department DepartmentObject { get; set; }

        ///<summary>
        /// Shift object referenced by the field ShiftId.
        ///</summary>
        public virtual Shift ShiftObject { get; set; }

        #endregion
    }
}