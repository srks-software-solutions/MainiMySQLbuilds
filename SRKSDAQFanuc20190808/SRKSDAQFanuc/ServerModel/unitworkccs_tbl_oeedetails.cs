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
    
    public partial class unitworkccs_tbl_oeedetails
    {
        public int OEEID { get; set; }
        public Nullable<int> MachineID { get; set; }
        public string CorrectedDate { get; set; }
        public Nullable<decimal> Availability { get; set; }
        public Nullable<decimal> Performance { get; set; }
        public Nullable<decimal> Quality { get; set; }
        public Nullable<decimal> OEE { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> IsDeleted { get; set; }
        public Nullable<decimal> OperatingTimeinMin { get; set; }
        public Nullable<int> TotalPartsCount { get; set; }
        public Nullable<decimal> PerformanceFactor { get; set; }
        public Nullable<decimal> TotalIDLETimeinMin { get; set; }
        public Nullable<decimal> PowerOffTimeInMinutes { get; set; }
        public Nullable<decimal> PowerOnTimeInMinutes { get; set; }
        public Nullable<decimal> TotalTimeInMinutes { get; set; }
        public Nullable<int> actualQty { get; set; }
        public string fgPartNo { get; set; }
        public Nullable<int> trialPartCount { get; set; }
        public Nullable<int> rejectionQty { get; set; }
        public Nullable<int> reworkQty { get; set; }
        public Nullable<int> dryRunQty { get; set; }
        public Nullable<int> opNo { get; set; }
        public string workOrderNo { get; set; }
        public Nullable<int> okQty { get; set; }
        public Nullable<decimal> bdTime { get; set; }
        public Nullable<decimal> MinorLossTime { get; set; }
        public Nullable<decimal> AvSumNumerator { get; set; }
        public Nullable<decimal> AvsumDenominator { get; set; }
        public Nullable<decimal> PerSumNumerator { get; set; }
        public Nullable<decimal> perSumDenominator { get; set; }
        public Nullable<decimal> QntSumNumerator { get; set; }
        public Nullable<decimal> QntSumDenominator { get; set; }
    }
}
