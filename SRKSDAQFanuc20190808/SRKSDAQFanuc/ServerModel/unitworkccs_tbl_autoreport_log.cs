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
    
    public partial class unitworkccs_tbl_autoreport_log
    {
        public int AutoReportLogID { get; set; }
        public Nullable<System.DateTime> CorrectedDate { get; set; }
        public Nullable<int> AutoReportID { get; set; }
        public Nullable<System.DateTime> InsertedOn { get; set; }
        public Nullable<int> ExcelCreated { get; set; }
        public Nullable<int> MailSent { get; set; }
        public Nullable<System.DateTime> CompletedOn { get; set; }
        public Nullable<System.DateTime> ExcelCreatedTime { get; set; }
    
        public virtual unitworkccs_tbl_autoreportsetting unitworkccs_tbl_autoreportsetting { get; set; }
    }
}
