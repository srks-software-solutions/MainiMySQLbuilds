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
    
    public partial class unitworkccs_configuration_tblpmcheckpoint
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public unitworkccs_configuration_tblpmcheckpoint()
        {
            this.unitworkccs_configuration_tblpmchecklist = new HashSet<unitworkccs_configuration_tblpmchecklist>();
            this.unitworkccs_configuration_tblpmchecklist1 = new HashSet<unitworkccs_configuration_tblpmchecklist>();
        }
    
        public int pmcpID { get; set; }
        public string TypeofCheckpoint { get; set; }
        public string CheckList { get; set; }
        public string frequency { get; set; }
        public int CellID { get; set; }
        public string Value { get; set; }
        public Nullable<int> Isdeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public int PlantID { get; set; }
        public int ShopID { get; set; }
        public string How { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_configuration_tblpmchecklist> unitworkccs_configuration_tblpmchecklist { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_configuration_tblpmchecklist> unitworkccs_configuration_tblpmchecklist1 { get; set; }
        public virtual unitworkccs_tblplant unitworkccs_tblplant { get; set; }
        public virtual unitworkccs_tblshop unitworkccs_tblshop { get; set; }
        public virtual unitworkccs_tblplant unitworkccs_tblplant1 { get; set; }
        public virtual unitworkccs_tblshop unitworkccs_tblshop1 { get; set; }
    }
}
