//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SRKSDAQFanuc.ServerModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class unitworkccs_tblcontrolplan
    {
        public int cpId { get; set; }
        public Nullable<int> plantId { get; set; }
        public Nullable<int> cellId { get; set; }
        public Nullable<int> childPartNo { get; set; }
        public Nullable<int> routingNo { get; set; }
        public string fgDesc { get; set; }
        public string controlPlanNo { get; set; }
        public Nullable<decimal> revisionNo { get; set; }
        public Nullable<System.DateTime> approvedDate { get; set; }
        public System.DateTime createdOn { get; set; }
        public int createdBy { get; set; }
        public Nullable<System.DateTime> modifiedOn { get; set; }
        public Nullable<int> modifiedBy { get; set; }
        public int isDeleted { get; set; }
        public Nullable<int> machineId { get; set; }
    }
}
