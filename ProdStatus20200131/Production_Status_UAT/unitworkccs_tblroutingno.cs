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
    
    public partial class unitworkccs_tblroutingno
    {
        public int id { get; set; }
        public Nullable<int> plantId { get; set; }
        public string description { get; set; }
        public Nullable<int> cellId { get; set; }
        public Nullable<int> subCellId { get; set; }
        public string routingNo { get; set; }
        public Nullable<int> isDeleted { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> modifiedOn { get; set; }
        public Nullable<int> modifiedBy { get; set; }
        public Nullable<int> fgPartId { get; set; }
        public Nullable<int> childFgPartId { get; set; }
    }
}
