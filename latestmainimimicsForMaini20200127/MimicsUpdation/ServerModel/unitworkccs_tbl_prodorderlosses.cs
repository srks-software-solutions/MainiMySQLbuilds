//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MimicsUpdation.ServerModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class unitworkccs_tbl_prodorderlosses
    {
        public int ProdOrderlossID { get; set; }
        public int WOID { get; set; }
        public int LossID { get; set; }
        public long LossDuration { get; set; }
        public System.DateTime CorrectedDate { get; set; }
        public int LossCodeL1ID { get; set; }
        public Nullable<int> LossCodeL2ID { get; set; }
        public int MachineID { get; set; }
    }
}
