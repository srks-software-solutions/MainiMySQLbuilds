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
    
    public partial class unitworkccs_tbl_axisdetails2
    {
        public int AD2ID { get; set; }
        public Nullable<int> MachineID { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<decimal> FeedRate { get; set; }
        public Nullable<decimal> SpindleLoad { get; set; }
        public Nullable<decimal> SpindleSpeed { get; set; }
        public int IsDeleted { get; set; }
        public Nullable<System.DateTime> InsertedOn { get; set; }
        public Nullable<decimal> FeedRatePercentage { get; set; }
    }
}
