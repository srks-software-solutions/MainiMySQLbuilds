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
    
    public partial class unitworkccs_tblcellpart
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public unitworkccs_tblcellpart()
        {
            this.unitworkccs_tblbottelneck = new HashSet<unitworkccs_tblbottelneck>();
        }
    
        public int cpID { get; set; }
        public int CellID { get; set; }
        public string partNo { get; set; }
        public string PartDescription { get; set; }
        public int IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public int IsDefault { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_tblbottelneck> unitworkccs_tblbottelneck { get; set; }
    }
}
