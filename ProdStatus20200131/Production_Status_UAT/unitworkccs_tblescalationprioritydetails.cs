//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Production_Status_UAT
{
    using System;
    using System.Collections.Generic;
    
    public partial class unitworkccs_tblescalationprioritydetails
    {
        public int EPId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public Nullable<int> CellId { get; set; }
        public string CellName { get; set; }
        public Nullable<int> SubCellId { get; set; }
        public string SubCellName { get; set; }
        public Nullable<int> CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Nullable<int> SMSPriorityLevel { get; set; }
        public int IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
    }
}
