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
    
    public partial class unitworkccs_tbl_pmsdashboardchecklist
    {
        public int Id { get; set; }
        public int CheckListHeaderId { get; set; }
        public int CheckListDetailsId { get; set; }
        public Nullable<int> RoleId { get; set; }
        public Nullable<bool> IsNumaric { get; set; }
        public Nullable<bool> IsText { get; set; }
        public string NumaricComment { get; set; }
        public string TextComment { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> IsDeleted { get; set; }
        public Nullable<int> IsApproved { get; set; }
    }
}
