//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace i_facility_IdleHandlerWithOptimization
{
    using System;
    using System.Collections.Generic;
    
    public partial class unitworkccs_tbltoollifeoperator
    {
        public int ToolLifeID { get; set; }
        public int MachineID { get; set; }
        public string ToolNo { get; set; }
        public string ToolName { get; set; }
        public string ToolCTCode { get; set; }
        public int ToolIDAdmin { get; set; }
        public Nullable<int> StandardToolLife { get; set; }
        public int toollifecounter { get; set; }
        public Nullable<System.DateTime> InsertedOn { get; set; }
        public Nullable<int> InsertedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public int IsReset { get; set; }
        public int IsDeleted { get; set; }
        public int ResetCounter { get; set; }
        public Nullable<int> HMIID { get; set; }
        public Nullable<int> Sync { get; set; }
        public Nullable<bool> IsCompleted { get; set; }
        public bool IsCycleStart { get; set; }
        public string ResetReason { get; set; }
    }
}
