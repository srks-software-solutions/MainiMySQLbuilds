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
    
    public partial class unitworkccs_tblcell
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public unitworkccs_tblcell()
        {
            this.unitworkccs_configuration_tblprimitivemaintainancescheduling = new HashSet<unitworkccs_configuration_tblprimitivemaintainancescheduling>();
            this.unitworkccs_tblmachinedetails = new HashSet<unitworkccs_tblmachinedetails>();
        }
    
        public int CellID { get; set; }
        public string CellName { get; set; }
        public string CellDesc { get; set; }
        public Nullable<int> PlantID { get; set; }
        public int ShopID { get; set; }
        public int IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public string CelldisplayName { get; set; }
        public Nullable<sbyte> defaultFlag { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_configuration_tblprimitivemaintainancescheduling> unitworkccs_configuration_tblprimitivemaintainancescheduling { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<unitworkccs_tblmachinedetails> unitworkccs_tblmachinedetails { get; set; }
        public virtual unitworkccs_tblshop unitworkccs_tblshop { get; set; }
        public virtual unitworkccs_tblplant unitworkccs_tblplant { get; set; }
    }
}
