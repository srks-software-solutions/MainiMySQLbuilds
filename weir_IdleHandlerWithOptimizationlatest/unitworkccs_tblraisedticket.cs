//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace i_facility_IdleHandlerWithOptimization
{
    using System;
    using System.Collections.Generic;
    
    public partial class unitworkccs_tblraisedticket
    {
        public int ticketId { get; set; }
        public string ticketNo { get; set; }
        public Nullable<int> machineId { get; set; }
        public Nullable<System.DateTime> ticketOpenDate { get; set; }
        public Nullable<int> operatorId { get; set; }
        public Nullable<int> partId { get; set; }
        public Nullable<int> classId { get; set; }
        public Nullable<int> categoryId { get; set; }
        public Nullable<int> statusId { get; set; }
        public Nullable<int> reasonId { get; set; }
        public Nullable<int> roleId { get; set; }
        public Nullable<int> status { get; set; }
        public string comments { get; set; }
        public Nullable<int> isDeleted { get; set; }
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<long> createdBy { get; set; }
        public Nullable<System.DateTime> modifiedOn { get; set; }
        public Nullable<long> modifiedBy { get; set; }
        public string correctedDate { get; set; }
        public Nullable<System.DateTime> ticketClosedDate { get; set; }
        public Nullable<int> alarmId { get; set; }
    }
}
