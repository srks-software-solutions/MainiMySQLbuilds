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
    
    public partial class unitworkccs_tblandondispdet
    {
        public int ANDONDISPID { get; set; }
        public string IPAddress { get; set; }
        public int PlantID { get; set; }
        public Nullable<int> ShopID { get; set; }
        public Nullable<int> CellID { get; set; }
        public System.DateTime InsertedOn { get; set; }
        public int InsertedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public int IsDeleted { get; set; }
    
        public virtual unitworkccs_tblplant unitworkccs_tblplant { get; set; }
        public virtual unitworkccs_tblshop unitworkccs_tblshop { get; set; }
    }
}
