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
    
    public partial class unitworkccs_operationlog
    {
        public int idoperationlog { get; set; }
        public string OpMsg { get; set; }
        public Nullable<System.DateTime> OpDate { get; set; }
        public Nullable<System.TimeSpan> OpTime { get; set; }
        public Nullable<System.DateTime> OpDateTime { get; set; }
        public string OpReason { get; set; }
        public Nullable<int> MachineID { get; set; }
    }
}
