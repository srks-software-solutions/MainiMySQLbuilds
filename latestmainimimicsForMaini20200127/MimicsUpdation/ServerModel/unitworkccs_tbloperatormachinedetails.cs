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
    
    public partial class unitworkccs_tbloperatormachinedetails
    {
        public int opertorMacDetId { get; set; }
        public Nullable<int> operatorLoginId { get; set; }
        public Nullable<int> machineId { get; set; }
        public Nullable<int> isDeleted { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public string createdBy { get; set; }
        public Nullable<System.DateTime> modifiedOn { get; set; }
        public string modifiedBy { get; set; }
    
        public virtual unitworkccs_tbloperatorlogindetails unitworkccs_tbloperatorlogindetails { get; set; }
        public virtual unitworkccs_tbloperatorlogindetails unitworkccs_tbloperatorlogindetails1 { get; set; }
    }
}
