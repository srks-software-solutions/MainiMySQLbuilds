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
    
    public partial class unitworkccs_tblplant
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public unitworkccs_tblplant()
        {
            this.unitworkccs_configuration_tblpmchecklist = new HashSet<unitworkccs_configuration_tblpmchecklist>();
            this.unitworkccs_configuration_tblpmchecklist1 = new HashSet<unitworkccs_configuration_tblpmchecklist>();
            this.unitworkccs_configuration_tblpmcheckpoint = new HashSet<unitworkccs_configuration_tblpmcheckpoint>();
            this.unitworkccs_configuration_tblpmcheckpoint1 = new HashSet<unitworkccs_configuration_tblpmcheckpoint>();
            this.unitworkccs_configuration_tblprimitivemaintainancescheduling = new HashSet<unitworkccs_configuration_tblprimitivemaintainancescheduling>();
            this.unitworkccs_tbl_autoreportsetting = new HashSet<unitworkccs_tbl_autoreportsetting>();
            this.unitworkccs_tblandondispdet = new HashSet<unitworkccs_tblandondispdet>();
            this.unitworkccs_tblandonimagetextscheduleddisplay = new HashSet<unitworkccs_tblandonimagetextscheduleddisplay>();
            this.unitworkccs_tblbottelneck = new HashSet<unitworkccs_tblbottelneck>();
            this.unitworkccs_tblcell = new HashSet<unitworkccs_tblcell>();
            this.unitworkccs_tblemailescalation = new HashSet<unitworkccs_tblemailescalation>();
            this.unitworkccs_tblmachinedetails = new HashSet<unitworkccs_tblmachinedetails>();
            this.unitworkccs_tblmultipleworkorder = new HashSet<unitworkccs_tblmultipleworkorder>();
            this.unitworkccs_tblshop = new HashSet<unitworkccs_tblshop>();
            this.unitworkccs_tblshiftplanner = new HashSet<unitworkccs_tblshiftplanner>();
        }
    
        public int PlantID { get; set; }
        public string PlantName { get; set; }
        public string PlantDesc { get; set; }
        public int IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public string PlantDisplayName { get; set; }
        public string PlantCode { get; set; }
        public string PlantLocation { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_configuration_tblpmchecklist> unitworkccs_configuration_tblpmchecklist { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_configuration_tblpmchecklist> unitworkccs_configuration_tblpmchecklist1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_configuration_tblpmcheckpoint> unitworkccs_configuration_tblpmcheckpoint { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_configuration_tblpmcheckpoint> unitworkccs_configuration_tblpmcheckpoint1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_configuration_tblprimitivemaintainancescheduling> unitworkccs_configuration_tblprimitivemaintainancescheduling { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_tbl_autoreportsetting> unitworkccs_tbl_autoreportsetting { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_tblandondispdet> unitworkccs_tblandondispdet { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_tblandonimagetextscheduleddisplay> unitworkccs_tblandonimagetextscheduleddisplay { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_tblbottelneck> unitworkccs_tblbottelneck { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_tblcell> unitworkccs_tblcell { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_tblemailescalation> unitworkccs_tblemailescalation { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_tblmachinedetails> unitworkccs_tblmachinedetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_tblmultipleworkorder> unitworkccs_tblmultipleworkorder { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_tblshop> unitworkccs_tblshop { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_tblshiftplanner> unitworkccs_tblshiftplanner { get; set; }
    }
}
