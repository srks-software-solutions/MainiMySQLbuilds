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
    
    public partial class unitworkccs_tblmaterialmaster
    {
        public long materialmasterId { get; set; }
        public string materialName { get; set; }
        public string materialDescription { get; set; }
        public string plantCode { get; set; }
        public Nullable<int> plantId { get; set; }
        public string partCode { get; set; }
        public string partDescription { get; set; }
        public string UOM { get; set; }
        public string materialType { get; set; }
        public Nullable<bool> isDeleted { get; set; }
        public Nullable<bool> isActive { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<long> createdBy { get; set; }
        public Nullable<System.DateTime> modifiedOn { get; set; }
        public Nullable<long> modifiedBy { get; set; }
    }
}
