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
    
    public partial class unitworkccs_tbl_livetblsensorvalue
    {
        public int sensorvalueid { get; set; }
        public Nullable<int> MachineID { get; set; }
        public Nullable<int> SensorMasterID { get; set; }
        public Nullable<int> sensorValues { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> IsDeleted { get; set; }
        public Nullable<System.DateTime> ValueDateTime { get; set; }
        public string CorrectedDate { get; set; }
        public Nullable<int> IsConverted { get; set; }
    }
}
