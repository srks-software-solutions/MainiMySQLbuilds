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
    
    public partial class unitworkccs_tbl_livecbmparameters
    {
        public int cbmpID { get; set; }
        public int MachineID { get; set; }
        public int SensorGroupID { get; set; }
        public string IPAddress { get; set; }
        public int MemoryAddress { get; set; }
        public int SensorValue { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<int> IsDeleted { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public string CorrectedDate { get; set; }
        public Nullable<int> IsConverted { get; set; }
    }
}
