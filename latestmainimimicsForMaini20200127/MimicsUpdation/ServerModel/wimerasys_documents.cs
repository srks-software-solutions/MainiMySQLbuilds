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
    
    public partial class wimerasys_documents
    {
        public int id { get; set; }
        public string documentType { get; set; }
        public string partNumber { get; set; }
        public Nullable<int> operationNumber { get; set; }
        public string serialNumber { get; set; }
        public string documentNumber { get; set; }
        public string documentUrl { get; set; }
        public Nullable<System.DateTime> uploadedDate { get; set; }
        public string revisionNumber { get; set; }
        public Nullable<System.DateTime> revisionDate { get; set; }
        public string revisionReason { get; set; }
    }
}
